using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName="LevelConfig", menuName = "JPacman Data/Level Config")]
public class LevelConfig : ScriptableObject {

#region Serialized Fields
	[SerializeField]
	GameObject m_backgroundPrefab;

	[SerializeField]
	GameObject m_dotPrefab;

	[SerializeField]
	GameObject m_playerPrefab;

	[SerializeField,Multiline(30)]
	string m_map;

	public class MapData
	{
		public Vector2[] m_dotPositions;
		public Vector2 m_playerPos;
		public int m_width;
		public int m_height;
	}

	MapData m_mapData;
#endregion

#region Public Accesors
	public GameObject BackgroundPrefab
	{
		get
		{
			return m_backgroundPrefab;
		}
	}

	public GameObject DotPrefab
	{
		get
		{
			return m_dotPrefab;
		}
	}

	public GameObject PlayerPrefab
	{
		get
		{
			return m_playerPrefab;
		}
	}

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

		int x = 0, y = height - 1;
		foreach (var line in lines) {
			foreach (var c in line) {
				mapChars [x, y] = c;
				x++;
			}
			y--;
			x = 0;
		}

		// now search for relevant elements in the map layout

		var dots = new List<Vector2> ();
		for (x = 0; x < width; x++) {
			for (y = 0; y < height; y++) {

				int c = mapChars [x, y];

				switch (c) {
				// check if there is a dot (group of four #)
				case '#':
					if (mapChars [x - 1, y] == '#' &&
					    mapChars [x, y + 1] == '#' && mapChars [x - 1, y + 1] == '#') {
						dots.Add (new Vector2 (x, y));
					}
					break;

				// check for player start pos
				case 'P':
					data.m_playerPos = new Vector2 (x, y);
					break;
				}
			}
		}
		data.m_width = width;
		data.m_height = height;
		data.m_dotPositions = dots.ToArray ();

		return data;
	}
}
