using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName="GameConfig", menuName = "JPacman Data/Game Config")]
public class GameConfig : ScriptableObject
{
    public GameObject DotPrefab;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject WallPrefab;
    public GameObject TilePrefab;
    public GameObject TunnelPrefab;
    public GameObject FruitPrefab;
    public GameObject PowerupPrefab;
    public IntroConfig IntroConfig;
    public MenuConfig MenuConfig;
    public int LivesCount;
    public MapConfig[] MapConfigs;
    public RoundConfig[] RoundConfigs;
    public MenuDotShapeConfig[] DotShapes;
    public Color[] DotCloneColors;
    public Color TileColor;
    public Color[] TunnelColor;

    [Header("Debugging")]
    [Tooltip("-1 means go to intro and menu; 0+ means go straight to that level")]
    public int DebugStartLevel = -1;

    public string GetMapDisplayName(string mapId)
    {
        foreach (var map in MapConfigs)
        {
            if (map.Map.Id == mapId)
            {
                return map.DisplayName;
            }
        }
        return null;
    }

    public string GetRoundDisplayName(int roundId)
    {
        foreach (var round in RoundConfigs)
        {
            if (round.RoundNumber == roundId)
            {
                return round.DisplayName;
            }
        }
        return null;
    }

}

