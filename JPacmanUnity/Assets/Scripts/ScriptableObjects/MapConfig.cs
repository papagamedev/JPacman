using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "JPacman Data/Map Config")]
public class MapConfig : ScriptableObject
{

	[SerializeField, Multiline(30)]
	string m_map;

    public string DisplayName;

    public class MapData
	{
		public string Id;
		public int Width;
		public int Height;
		public char[,] Data;
		public MapTileData[] Tiles;
	}

    public MapData Map => BuildMapData();

	private MapData BuildMapData()
	{
        var data = new MapData();

        // convert text into a chars matrix

        string[] lines = m_map.Split("\r\n");
        int height = lines.Length;
        int width = lines[0].Length;
        char[,] mapChars = new char[width, height];

        int x = 0, y = 0;
        foreach (var line in lines)
        {
            foreach (var c in line)
            {
                mapChars[x, y] = c;
                x++;
            }
            y++;
            x = 0;
        }
        data.Id = name;
        data.Width = width;
        data.Height = height;
        data.Data = mapChars;
        var tiles = new List<MapTileData>();
        tiles.AddRange(new MapTileData.Generator(data));
        tiles.AddRange(new MapTileData.TunnelGenerator(data));
        data.Tiles = tiles.ToArray();
        return data;
    }
}
