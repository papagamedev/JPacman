using UnityEngine;

[CreateAssetMenu(fileName="GameConfig", menuName = "JPacman Data/Game Config")]
public class GameConfig : ScriptableObject
{
    public GameObject DotPrefab;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject WallPrefab;
    public GameObject FruitPrefab;
    public GameObject PowerupPrefab;
    public IntroConfig IntroConfig;
    public MenuConfig MenuConfig;
    public int LivesCount;
    public MapConfig[] MapConfigs;
    public RoundConfig[] RoundConfigs;
    public Color[] DotCloneColors;

    [Header("Debugging")]
    [Tooltip("-1 means go to intro and menu; 0+ means go straight to that level")]
    public int DebugStartLevel = -1;
}

