using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

#region Serialized Fields

	[SerializeField]
	LevelConfig m_levelConfig;

#endregion

	GameObject[] m_dots;
	GameObject m_player;

	void Start () {

		InitLevel ();

	}
	
	void InitLevel () {
	
		var dotPrefab = m_levelConfig.DotPrefab;
		var bgPrefab = m_levelConfig.BackgroundPrefab;
		var playerPrefab = m_levelConfig.PlayerPrefab;
		var map = m_levelConfig.Map;

		// instantiate background
		Vector3 centerPos = new Vector3(map.m_width*0.5f - 0.5f,map.m_height*0.5f - 2.0f,0);
		Instantiate(bgPrefab,centerPos,Quaternion.identity,transform);

		// instantiate dots
		m_dots = new GameObject[map.m_dotPositions.Length];
		int k = 0;
		foreach (var dotPos in map.m_dotPositions) {

			m_dots[k++] = Instantiate(dotPrefab,dotPos,Quaternion.identity,transform) as GameObject;
		}

		// instantiate the player
		m_player = Instantiate(playerPrefab,map.m_playerPos,Quaternion.identity,transform) as GameObject;

		// locate the camera
		Vector3 cameraPos = centerPos;
		cameraPos.z = -10.0f;
		Camera.main.transform.position = cameraPos;
	}

}
