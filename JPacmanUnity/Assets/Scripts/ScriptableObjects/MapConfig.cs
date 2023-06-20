using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "JPacman Data/Map Config")]
public class MapConfig : ScriptableObject
{

	[SerializeField, Multiline(30)]
	string m_map;

	public class MapData
	{
		public string m_id;
		public int m_width;
		public int m_height;
		public char[,] m_data;
	}

	public MapData Map
	{
		get
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
			data.m_id = name;
			data.m_width = width;
			data.m_height = height;
			data.m_data = mapChars;

			return data;
		}
	}
}
