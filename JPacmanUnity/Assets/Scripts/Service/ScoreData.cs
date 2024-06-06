using Newtonsoft.Json;

public class ScoreData
{
    [JsonProperty("message")]
    public string Message;

    [JsonProperty("score")]
    public int Score;
}
