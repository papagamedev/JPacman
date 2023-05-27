﻿using UnityEngine;

[CreateAssetMenu(fileName="GameConfig", menuName = "JPacman Data/Game Config")]
public class GameConfig : ScriptableObject
{
    [System.Serializable]
    public class LevelConfig
    {
        public int RoundNumber;
        public int LevelNumber;
        public bool BonusLevel;
        public bool MoveDots;
        public int MultiplyDots;
        public bool MovePowerups;
        public float PlayerSpeed;
        public float EnemySpeed;
        public float EnemySpeedInTunnel;
        public float EnemySpeedScared;
        public float EnemySpeedReturnHome;
        public float EnemyInHomeTime;
        public float EnemyScaredTime;
        public int EnemyScore;
        public int EnemyCI;
        public int FruitScore;
        public int FruitSpriteIdx;
        public float FruitWaitTime;
        public float FruitDuration;
        public MapConfig MapConfig;
    }

    public GameObject DotPrefab;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject WallPrefab;
    public GameObject FruitPrefab;
    public GameObject PowerupPrefab;
    public IntroConfig IntroConfig;
    public LevelConfig[] LevelConfigs;
}

