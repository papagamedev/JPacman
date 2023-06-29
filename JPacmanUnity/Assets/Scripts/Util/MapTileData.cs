using System;
using System.Collections.Generic;
using System.Linq;

public struct MapTileData
{
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;
    public int TunnelIdx;
    public Direction TunnelDir;

    public class Generator : IEnumerable<MapTileData>
    {
        protected MapConfig.MapData m_map;

        public Generator(MapConfig.MapData map)
        {
            m_map = map;
        }

        public virtual IEnumerator<MapTileData> GetEnumerator()
        {
            return new Iterator(m_map, MapConfigData.TileChars);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public class Iterator : IEnumerator<MapTileData>
        {
            protected MapConfig.MapData m_map;
            protected char[,] m_workingData;
            protected char[] m_validChars;
            protected MapTileData m_current;

            public Iterator(MapConfig.MapData map, char[] validChars)
            {
                m_map = map;
                m_validChars = validChars;
                Reset();
            }

            public MapTileData Current 
            {
                get
                {
                    if (m_current.MinX == -1 || m_current.MinY >= m_map.Height)
                    {
                        throw new InvalidOperationException();
                    }
                    return m_current;
                }
            }

            object System.Collections.IEnumerator.Current => Current;

            public void Dispose()
            {
                m_map = null;
                m_workingData = null;
                m_validChars = null;
            }

            protected bool IsValidChar(int x, int y, out char validChar)
            {
                validChar = m_workingData[x, y];
                return m_validChars.Contains(validChar);
            }

            public bool MoveNext()
            {
                // check if just starting
                if (m_current.MinX == -1)
                {
                    m_current.MinX++;
                }

                // scan rows searching for min x,y corner
                while (m_current.MinY < m_map.Height)
                {
                    if (IsValidChar(m_current.MinX, m_current.MinY, out var c))
                    {
                        // found a tile, scan the max bounds
                        FindMaxX();
                        FindMaxY();

                        // notify any child class of tile found for additional info to be added
                        OnTileFound();

                        // clear the tile from the working data, so we don't repeat the tiles
                        ClearWorkingTile();
                        return true;
                    }

                    m_current.MinX++;
                    if (m_current.MinX == m_map.Width)
                    {
                        m_current.MinX = 0;
                        m_current.MinY++;
                    }
                }
                return false;
            }

            public void Reset()
            {
                m_workingData = (char[,]) m_map.Data.Clone();
                m_current.MinX = -1;
                m_current.MinY = 0;
            }

            private void FindMaxX()
            {
                m_current.MaxX = m_current.MinX;
                while (m_current.MaxX < m_map.Width - 1)
                {
                    int testX = m_current.MaxX + 1;
                    if (!IsValidChar(testX, m_current.MinY, out var _))
                    {
                        return;
                    }
                    m_current.MaxX = testX;
                }
            }

            private void FindMaxY()
            {
                m_current.MaxY = m_current.MinY;
                while (m_current.MaxY < m_map.Height - 1)
                {
                    int testY = m_current.MaxY + 1;
                    for (int x = m_current.MinX; x <= m_current.MaxX; x++)
                    {
                        if (!IsValidChar(x, testY, out var _))
                        {
                            return;
                        }
                    }
                    m_current.MaxY = testY;
                }
            }

            public virtual void OnTileFound()
            {
                m_current.TunnelIdx = -1;
            }

            public void ClearWorkingTile()
            {
                for (int x = m_current.MinX; x <= m_current.MaxX; x++)
                {
                    for (int y = m_current.MinY; y <= m_current.MaxY; y++)
                    {
                        m_workingData[x, y] = MapConfigData.kWallChar;
                    }
                }
            }
        }
    }

    public class TunnelGenerator : Generator
    {
        public TunnelGenerator(MapConfig.MapData map) : base(map)
        {

        }

        public override IEnumerator<MapTileData> GetEnumerator()
        {
            return new TunnelIterator(m_map, MapConfigData.TunnelChars);
        }

        class TunnelIterator : Iterator
        {
            private int m_tunnelIdx;

            public TunnelIterator(MapConfig.MapData map, char[] validChars) : base(map, validChars)
            {
            }

            public override void OnTileFound()
            {
                // TODO
                m_current.TunnelIdx = m_tunnelIdx++;
                m_current.TunnelDir = Direction.Right;
            }
        }
    }
}