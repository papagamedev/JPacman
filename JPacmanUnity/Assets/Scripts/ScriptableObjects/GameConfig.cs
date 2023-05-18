using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName="GameConfig", menuName = "JPacman Data/Game Config")]
public class GameConfig : ScriptableObject
{
    [System.Serializable]
    public class LevelConfig
    {
        public int RoundNumber;
        public int LevelNumber;
        public float PlayerSpeed;
        public MapConfig MapConfig;
    }

    public GameObject DotPrefab;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject WallPrefab;
    public LevelConfig[] LevelConfigs;
    
    public void AssignMapIds()
    {

    }
}

