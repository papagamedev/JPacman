using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static LevelConfig;

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
        new float2(0, -1)
    };

    public enum Direction
    {
        Right,
        Left,
        Down,
        Up,
        None
    };

    public float Speed;
    public Direction CurrentDir;
    public Direction DesiredDir;

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

    public void Move(BlobAssetReference<MapConfigData> mapBlobRef, float deltaTime)
    {
        // check if we are in a cell edge; if that's the case, check if movement direction is changed
        ref var map = ref mapBlobRef.Value;
        var mapPos = map.WorldToMapPos(m_transform.ValueRO.Position);
        float2 mapPosAbs = math.abs(math.frac(mapPos));
        bool atCellEdge = (mapPosAbs.x < 0.001 && mapPosAbs.y < 0.001);

        if (CheckDirectionChange(ref map, mapPos, atCellEdge) && atCellEdge)
        {
            // if direction was changed at a cell edge, snap to the cell pos
            mapPos = math.round(mapPos);
            SetMapPos(ref map, mapPos);
        }

        // if the new direction is none, no more movement this frame
        var currentDir = m_movable.ValueRO.CurrentDir;
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
            Move(mapBlobRef, deltaTime);
        }
    }

    private bool CheckDirectionChange(ref MapConfigData map, float2 mapPos, bool atCellEdge)
    {
        var mapPosExact = math.round(mapPos);
        int mapX = (int)mapPosExact.x;
        int mapY = (int)mapPosExact.y;

        // if desired direction is different than current, evaluate if that direction is allowed
        var desiredDir = m_movable.ValueRO.DesiredDir;
        var currentDir = m_movable.ValueRO.CurrentDir;
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
}
