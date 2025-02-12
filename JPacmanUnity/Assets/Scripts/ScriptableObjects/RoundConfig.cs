﻿using UnityEngine;

[CreateAssetMenu(fileName= "RoundConfig", menuName = "JPacman Data/Round Config")]
public class RoundConfig : ScriptableObject
{
    public string DisplayName;
    public int RoundNumber;
    public int BaseEnemyCI;
    public int BaseDotsMoveWaitTime;
    public int BasePowerupsMoveWaitTime;
    public MapConfig MapConfig;
    public LevelConfig[] LevelConfigs;
}
