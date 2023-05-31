using UnityEngine;

[CreateAssetMenu(fileName= "LevelConfig", menuName = "JPacman Data/Level Config")]
public class LevelConfig : ScriptableObject
{
    public int LevelNumber;
    public LevelConfigData.ELevelType LevelType;
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
    public FruitConfig FruitConfig;
}
