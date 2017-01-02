using UnityEngine;
using UnityEditor;
using System.Collections;

[CreateAssetMenu(fileName="LevelConfig", menuName = "JPacman Data/Level Config")]
public class LevelConfig : ScriptableObject {

	[SerializeField]
	Sprite m_background;

	[SerializeField,Multiline(30)]
	string m_map;

}
