using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Movable : IComponentData
{
    public float Speed;
    public float SpeedInTunnel;
    public bool AllowChangeDirInMidCell;
    public Direction CurrentDir;
    public Direction DesiredDir;
    public bool ForcedDir;
    public float2 NextCellEdgeMapPos;
    public AvailableDirections NextCellEdgeAvailableDirections;
    public float2 LastCellEdgeMapPos;
    public bool Init;
    public Random Rand;
    public bool IsInTunnel;
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

        // check tunnel teleport
        if (atCellEdge)
        {
            if (map.IsTunnelEntrance(m_movable.ValueRO.LastCellEdgeMapPos))
            {
                m_movable.ValueRW.IsInTunnel = !m_movable.ValueRW.IsInTunnel;
            }
            else if (map.IsTunnel(m_movable.ValueRO.LastCellEdgeMapPos, out var tunnelIdx))
            {
                mapPos = map.TunnelPos[MapConfigData.OppositeTunnelIdx(tunnelIdx)];
                m_movable.ValueRW.LastCellEdgeMapPos = mapPos;
            }
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
        if (currentDir == Direction.None)
        {
            return;
        }

        // check how far are we moving, and if we are going to cross a cell boundary;
        // in that case, we adjust the deltaTime to move just that amount and reevaluate the movement at that time

        var speed = m_movable.ValueRO.IsInTunnel ? m_movable.ValueRO.SpeedInTunnel : m_movable.ValueRO.Speed;
        var moveDistance = speed * deltaTime;
        if (!atCellEdge)
        {
            float distanceToNextCell = 0;
            switch (currentDir)
            {
                case Direction.Left:
                    distanceToNextCell = mapPos.x - math.floor(mapPos.x);
                    break;

                case Direction.Right:
                    distanceToNextCell = math.ceil(mapPos.x) - mapPos.x;
                    break;

                case Direction.Up:
                    distanceToNextCell = mapPos.y - math.floor(mapPos.y);
                    break;

                case Direction.Down:
                    distanceToNextCell = math.ceil(mapPos.y) - mapPos.y;
                    break;
            }
            
            if (distanceToNextCell < moveDistance)
            {
                moveDistance = distanceToNextCell;
            }
        }

        // execute the movement

        mapPos += currentDir.Vector() * moveDistance;
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
        if (desiredDir != Direction.None && currentDir != desiredDir)
        {
            bool bChangeAllowed = false;
            if (!atCellEdge)
            {
                // if not at cell edge, opposite direction is always allowed
                switch (currentDir)
                {
                    case Direction.Left:
                        if (desiredDir == Direction.Right)
                        {
                            bChangeAllowed = true;
                        }
                        break;
                    case Direction.Up:
                        if (desiredDir == Direction.Down)
                        {
                            bChangeAllowed = true;
                        }
                        break;
                    case Direction.Down:
                        if (desiredDir == Direction.Up)
                        {
                            bChangeAllowed = true;
                        }
                        break;
                    case Direction.Right:
                        if (desiredDir == Direction.Left)
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
        if (currentDir != Direction.None && !map.IsDirectionAllowed(mapX, mapY, currentDir))
        {
            m_movable.ValueRW.CurrentDir = Direction.None;
            return true;
        }

        // no direction change
        return false;
    }

    public void UpdateNextCellEdgeInfo(ref MapConfigData map, Direction currentDir)
    {
        var nextMapPos = m_movable.ValueRO.LastCellEdgeMapPos + currentDir.Vector();
        m_movable.ValueRW.NextCellEdgeMapPos = nextMapPos;
        m_movable.ValueRW.NextCellEdgeAvailableDirections = GetAvailableDirections(ref map, nextMapPos);
    }

    public AvailableDirections GetAvailableDirections(ref MapConfigData map, float2 mapPos)
    {
        int x = (int) mapPos.x;
        int y = (int) mapPos.y;
        return new AvailableDirections()
        {
            Left = map.IsDirectionAllowed(x, y, Direction.Left),
            Right = map.IsDirectionAllowed(x, y, Direction.Right),
            Up = map.IsDirectionAllowed(x, y, Direction.Up),
            Down = map.IsDirectionAllowed(x, y, Direction.Down)
        };
    }

    public static Direction ComputeFollowTargetDir(float2 movablePos, Direction currentDir, float2 targetPos, AvailableDirections nextAvailableDirs, int movableCI, ref Random random)
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

    private static Direction CheckVerticalFollowDir(float2 targetToMovable, Direction currentDir, AvailableDirections nextAvailableDirs, int movableCI, ref Random random, bool lastTry)
    {
        if (targetToMovable.y < 0)
        {
            if ((!nextAvailableDirs.Down) || ((currentDir == Direction.Up) && (movableCI < 14)))
            {
                if (lastTry)
                {
                    return nextAvailableDirs.RandomNotOpposite(currentDir, ref random);
                }
                return CheckHorizontalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, true);
            }
            return Direction.Down;
        }
        else
        {
            if ((!nextAvailableDirs.Up) || ((currentDir == Direction.Down) && (movableCI < 14)))
            {
                if (lastTry)
                {
                    return nextAvailableDirs.RandomNotOpposite(currentDir, ref random);
                }
                return CheckHorizontalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, true);
            }
            return Direction.Up;
        }
    }

    private static Direction CheckHorizontalFollowDir(float2 targetToMovable, Direction currentDir, AvailableDirections nextAvailableDirs, int movableCI, ref Random random, bool lastTry)
    {
        if (targetToMovable.x < 0)
        {
            if ((!nextAvailableDirs.Right) || ((currentDir == Direction.Left) && (movableCI < 14)))
            {
                if (lastTry)
                {
                    return nextAvailableDirs.RandomNotOpposite(currentDir, ref random);
                }
                return CheckVerticalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, true);
            }
            return Direction.Right;
        }
        else
        {
            if ((!nextAvailableDirs.Left) || ((currentDir == Direction.Right) && (movableCI < 14)))
            {
                if (lastTry)
                {
                    return nextAvailableDirs.RandomNotOpposite(currentDir, ref random);
                }
                return CheckVerticalFollowDir(targetToMovable, currentDir, nextAvailableDirs, movableCI, ref random, true);
            }
            return Direction.Left;
        }
    }
}
