using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Movable : IComponentData
{


    static FixedList32Bytes<int> m_dirX = new FixedList32Bytes<int>()
    {
        1, -1, 0, 0
    };

    static FixedList32Bytes<int> m_dirY = new FixedList32Bytes<int>()
    {
        0, 0, 1, -1
    };

    static FixedList64Bytes<float2> m_dirVector = new FixedList64Bytes<float2>()
    {
        new float2(1, 0),
        new float2(-1, 0),
        new float2(0, 1),
        new float2(0, -1),
        new float2(0, 0)
    };

    public enum Direction
    {
        Right,
        Left,
        Down,
        Up,
        None
    };

    public static Direction OppositeDir(Direction dir)
    {
        return dir switch
        {
            Direction.Left => Direction.Right,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Right => Direction.Left,
            _ => Direction.None,
        };
    }

    public struct AvailableDirections
    {
        public bool Right;
        public bool Left;
        public bool Down;
        public bool Up;

        public int Count => (Right ? 1 : 0) + (Left ? 1 : 0) + (Down ? 1 : 0) + (Up ? 1 : 0);
        public Direction First => Right ? Direction.Right : Left ? Direction.Left : Down ? Direction.Down : Up ? Direction.Up : Direction.None;
        public bool Check(Direction direction)
        {
            return direction switch
            {
                Direction.Right => Right,
                Direction.Left => Left,
                Direction.Down => Down,
                Direction.Up => Up,
                _ => false,
            };
        }
        public Direction RandomNotOpposite(Direction currentDir, ref Random random)
        {
            var oppositeDir = OppositeDir(currentDir);
            if (Count <= 1)
            {
                var first = First;
                return oppositeDir == first ? Direction.None : first;
            }

            var dir = random.NextInt(4);
            while (!Check((Direction)dir) || dir == (int) oppositeDir)
            {
                dir = (dir + 1) % 4;
            }
            return (Direction)dir;
        }

        public FixedString32Bytes ToFixedString()
        {
            var str = new FixedString32Bytes();
            if (Right) str.Append("R");
            if (Left) str.Append("L");
            if (Down) str.Append("D");
            if (Up) str.Append("U");
            if (Count == 0) str.Append("-");
            return str;
        }
    }

    public float Speed;
    public bool AllowChangeDirInMidCell;
    public Direction CurrentDir;
    public Direction DesiredDir;
    public bool ForcedDir;
    public float2 NextCellEdgeMapPos;
    public AvailableDirections NextCellEdgeAvailableDirections;
    public float2 LastCellEdgeMapPos;
    public bool Init;
    public Random Rand;

    public int GetDirX(Direction dir) => m_dirX[(int)dir];
    public int GetDirY(Direction dir) => m_dirY[(int)dir];
    public float2 GetDirVector(Direction dir) => m_dirVector[(int)dir];
}

public readonly partial struct MovableAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;

    public void SetMapPos(ref MapConfigData map, float2 mapPos)
    {
        m_transform.ValueRW.Position = map.MapToWorldPos(mapPos);
    }

    public void Move(BlobAssetReference<MapsConfigData> mapBlobRef, int mapId, float deltaTime)
    {
        // check if we are in a cell edge; if that's the case, check if movement direction is changed
        ref var map = ref mapBlobRef.Value.MapsData[mapId];
        var mapPos = map.WorldToMapPos(m_transform.ValueRO.Position);
        float2 mapPosAbs = math.abs(math.frac(mapPos));
        bool atCellEdge = (mapPosAbs.x < 0.001 && mapPosAbs.y < 0.001);
        if (atCellEdge || !m_movable.ValueRO.Init)
        {
            m_movable.ValueRW.Init = true;
            m_movable.ValueRW.LastCellEdgeMapPos = math.round(mapPos);
        }

        if (m_movable.ValueRO.AllowChangeDirInMidCell || atCellEdge)
        {
            var dirChanged = CheckDirectionChange(ref map, mapPos, atCellEdge);
            if (dirChanged && atCellEdge)
            {
                // if direction was changed at a cell edge, snap to the cell pos
                mapPos = math.round(mapPos);
                SetMapPos(ref map, mapPos);
            }
        }

        // if the new direction is none, no more movement this frame
        var currentDir = m_movable.ValueRO.CurrentDir;
        UpdateNextCellEdgeInfo(ref map, currentDir);
        if (currentDir == Movable.Direction.None)
        {
            return;
        }

        // check how far are we moving, and if we are going to cross a cell boundary;
        // in that case, we adjust the deltaTime to move just that amount and reevaluate the movement at that time

        var speed = m_movable.ValueRO.Speed;
        var moveDistance = speed * deltaTime;
        if (!atCellEdge)
        {
            float distanceToNextCell = 0;
            switch (currentDir)
            {
                case Movable.Direction.Left:
                    distanceToNextCell = mapPos.x - math.floor(mapPos.x);
                    break;

                case Movable.Direction.Right:
                    distanceToNextCell = math.ceil(mapPos.x) - mapPos.x;
                    break;

                case Movable.Direction.Up:
                    distanceToNextCell = mapPos.y - math.floor(mapPos.y);
                    break;

                case Movable.Direction.Down:
                    distanceToNextCell = math.ceil(mapPos.y) - mapPos.y;
                    break;
            }
            
            if (distanceToNextCell < moveDistance)
            {
                moveDistance = distanceToNextCell;
            }
        }

        // execute the movement

        mapPos += m_movable.ValueRO.GetDirVector(currentDir) * moveDistance;
        SetMapPos(ref map, mapPos);


        // if a partial movement was done, complete the movement frame

        deltaTime -= moveDistance / speed;
        if (deltaTime > 0.001)
        {
            Move(mapBlobRef, mapId, deltaTime);
        }
    }

    private bool CheckDirectionChange(ref MapConfigData map, float2 mapPos, bool atCellEdge)
    {
        var mapPosExact = math.round(mapPos);
        int mapX = (int)mapPosExact.x;
        int mapY = (int)mapPosExact.y;

        var forcedDir = m_movable.ValueRO.ForcedDir;
        var desiredDir = m_movable.ValueRO.DesiredDir;
        var currentDir = m_movable.ValueRO.CurrentDir;

        // if desired direction is different than current, evaluate if that direction is allowed
        if (desiredDir != Movable.Direction.None && currentDir != desiredDir)
        {
            bool bChangeAllowed = false;
            if (!atCellEdge)
            {
                // if not at cell edge, opposite direction is always allowed
                switch (currentDir)
                {
                    case Movable.Direction.Left:
                        if (desiredDir == Movable.Direction.Right)
                        {
                            bChangeAllowed = true;
                        }
                        break;
                    case Movable.Direction.Up:
                        if (desiredDir == Movable.Direction.Down)
                        {
                            bChangeAllowed = true;
                        }
                        break;
                    case Movable.Direction.Down:
                        if (desiredDir == Movable.Direction.Up)
                        {
                            bChangeAllowed = true;
                        }
                        break;
                    case Movable.Direction.Right:
                        if (desiredDir == Movable.Direction.Left)
                        {
                            bChangeAllowed = true;
                        }
                        break;
                }
            }
            else if (forcedDir)
            {
                m_movable.ValueRW.ForcedDir = false;
                bChangeAllowed = true;
            }
            else
            {
                bChangeAllowed = map.IsDirectionAllowed(mapX, mapY, desiredDir);
            }

            if (bChangeAllowed)
            {
                m_movable.ValueRW.CurrentDir = desiredDir;
                return true;
            }
        }

        // if not at cell edge, just continue with current direction
        if (!atCellEdge)
        {
            return false;
        }

        // check if the current direction is no longer valid
        if (currentDir != Movable.Direction.None && !map.IsDirectionAllowed(mapX, mapY, currentDir))
        {
            m_movable.ValueRW.CurrentDir = Movable.Direction.None;
            return true;
        }

        // no direction change
        return false;
    }

    public void UpdateNextCellEdgeInfo(ref MapConfigData map, Movable.Direction currentDir)
    {
        var nextMapPos = m_movable.ValueRO.LastCellEdgeMapPos + m_movable.ValueRO.GetDirVector(currentDir);
        m_movable.ValueRW.NextCellEdgeMapPos = nextMapPos;
        m_movable.ValueRW.NextCellEdgeAvailableDirections = GetAvailableDirections(ref map, nextMapPos);
    }

    public Movable.AvailableDirections GetAvailableDirections(ref MapConfigData map, float2 mapPos)
    {
        int x = (int) mapPos.x;
        int y = (int) mapPos.y;
        return new Movable.AvailableDirections()
        {
            Left = map.IsDirectionAllowed(x, y, Movable.Direction.Left),
            Right = map.IsDirectionAllowed(x, y, Movable.Direction.Right),
            Up = map.IsDirectionAllowed(x, y, Movable.Direction.Up),
            Down = map.IsDirectionAllowed(x, y, Movable.Direction.Down)
        };
    }

    public static Movable.Direction ComputeFollowTargetDir(float2 movablePos, Movable.Direction currentDir, float2 targetPos, Movable.AvailableDirections nextAvailableDirs, int movableCI, ref Random random)
    {
        if (nextAvailableDirs.Count == 1)
        {
            return nextAvailableDirs.First;
        }

        if (movableCI < 4 || random.NextInt(movableCI) < 4)
        {
            return nextAvailableDirs.RandomNotOpposite(currentDir, ref random);
        }

        var targetToMovable =  movablePos - targetPos;
        var absVector = math.abs(targetToMovable);
        if (absVector.x < absVector.y)
        {
            return CheckVerticalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, false);
        }
        else
        {
            return CheckHorizontalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, false);
        }
    }

    private static Movable.Direction CheckVerticalFollowDir(float2 targetToMovable, Movable.Direction currentDir, Movable.AvailableDirections nextAvailableDirs, int movableCI, ref Random random, bool lastTry)
    {
        if (targetToMovable.y < 0)
        {
            if ((!nextAvailableDirs.Down) || ((currentDir == Movable.Direction.Up) && (movableCI < 14)))
            {
                if (lastTry)
                {
                    return nextAvailableDirs.RandomNotOpposite(currentDir, ref random);
                }
                return CheckHorizontalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, true);
            }
            return Movable.Direction.Down;
        }
        else
        {
            if ((!nextAvailableDirs.Up) || ((currentDir == Movable.Direction.Down) && (movableCI < 14)))
            {
                if (lastTry)
                {
                    return nextAvailableDirs.RandomNotOpposite(currentDir, ref random);
                }
                return CheckHorizontalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, true);
            }
            return Movable.Direction.Up;
        }
    }

    private static Movable.Direction CheckHorizontalFollowDir(float2 targetToMovable, Movable.Direction currentDir, Movable.AvailableDirections nextAvailableDirs, int movableCI, ref Random random, bool lastTry)
    {
        if (targetToMovable.x < 0)
        {
            if ((!nextAvailableDirs.Right) || ((currentDir == Movable.Direction.Left) && (movableCI < 14)))
            {
                if (lastTry)
                {
                    return nextAvailableDirs.RandomNotOpposite(currentDir, ref random);
                }
                return CheckVerticalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, true);
            }
            return Movable.Direction.Right;
        }
        else
        {
            if ((!nextAvailableDirs.Left) || ((currentDir == Movable.Direction.Right) && (movableCI < 14)))
            {
                if (lastTry)
                {
                    return nextAvailableDirs.RandomNotOpposite(currentDir, ref random);
                }
                return CheckVerticalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, true);
            }
            return Movable.Direction.Left;
        }
    }
}
