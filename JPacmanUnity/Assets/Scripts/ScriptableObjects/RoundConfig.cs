using UnityEngine;

[CreateAssetMenu(fileName= "RoundConfig", menuName = "JPacman Data/Round Config")]
public class RoundConfig : ScriptableObject
{
    public int RoundNumber;
    public int BaseEnemyCI;
    public int BaseDotsMoveWaitTime;
    public MapConfig MapConfig;
    public LevelConfig[] LevelConfigs;
}
