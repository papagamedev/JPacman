using System;
using UnityEngine;

// Player options from the Options menu, persisted in PlayerPrefs (the 1999 version kept them in Config.dat).
public static class GameSettings
{
    const string kStartRoundKey = "StartRound";
    const string kMovingCollectiblesKey = "MovingCollectibles";
    const string kMusicOnKey = "MusicOn";
    const string kSoundOnKey = "SoundOn";

    public static event Action Changed;

    // Index into GameConfig.RoundConfigs (0 = Fácil, 1 = Normal, 2 = Difícil)
    public static int StartRound
    {
        get => PlayerPrefs.GetInt(kStartRoundKey, 0);
        set => SetInt(kStartRoundKey, value);
    }

    // Whether dots and powerups start moving around the maze after a while ("Mov. Puntos")
    public static bool MovingCollectibles
    {
        get => GetBool(kMovingCollectiblesKey);
        set => SetBool(kMovingCollectiblesKey, value);
    }

    public static bool MusicOn
    {
        get => GetBool(kMusicOnKey);
        set => SetBool(kMusicOnKey, value);
    }

    public static bool SoundOn
    {
        get => GetBool(kSoundOnKey);
        set => SetBool(kSoundOnKey, value);
    }

    private static bool GetBool(string key) => PlayerPrefs.GetInt(key, 1) != 0;

    private static void SetBool(string key, bool value) => SetInt(key, value ? 1 : 0);

    private static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
