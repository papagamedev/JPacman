using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName="LevelConfig", menuName = "JPacman Data/Level Config")]
public class LevelConfig : ScriptableObject {

#region Serialized Fields
	[SerializeField,Multiline(30)]
	string m_map;

	public class MapData
	{
		public Vector2 m_playerPos;
		public int m_width;
		public int m_height;
		public char[,] m_data;
	}

	MapData m_mapData;
#endregion

#region Public Accesors

	public MapData Map
	{
		get
		{
			if (m_mapData == null)
			{
				m_mapData = LoadMap ();
			}
			return m_mapData;
		}
	}
#endregion

	MapData LoadMap()
	{
		var data = new MapData ();

		// convert text into a chars matrix

		string[] lines = m_map.Split ('\n');
		int height = lines.Length;
		int width = lines[0].Length;
		char[,] mapChars = new char[width,height];

		int x = 0, y = 0;
		foreach (var line in lines) {
			foreach (var c in line) {
				mapChars [x, y] = c;
				
				if (c == MapConfigData.kPlayerChar)
				{
                    data.m_playerPos = new Vector2(x, y);
                }

                x++;
			}
			y++;
			x = 0;
		}

		data.m_width = width;
		data.m_height = height;
		data.m_data = mapChars;

		return data;
	}
}
