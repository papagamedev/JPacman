using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BackendClient
{
#if DEBUG
//    const string BaseUrl = "http://192.168.100.118:7071/api/";
            const string BaseUrl = "https://papagamedevjpacman.azurewebsites.net/api/";
#else
        const string BaseUrl = "https://papagamedevjpacman.azurewebsites.net/api/";
#endif
    public static BackendClient Instance { get; private set; }
    public static void Create() { Instance = new BackendClient(); }
    private BackendClient() { }
    private string GetFullUrl(string url) => BaseUrl + url;
    private HttpClient m_httpClient = new HttpClient();
    public Task<List<ScoreData>> GetScores(string mapId, int round)
    {
        return GetAsync<List<ScoreData>>($"scores/{mapId}/{round}");
    }

    public async Task<ScoreData> AddScore(string mapId, int round, string message, int score)
    {
        var payload = new ScoreData()
        {
            Message = message,
            Score = score
        };
        var data = await PostAsync<ScoreData>($"scores/{mapId}/{round}", payload);
        return data;
    }

    private async Task<T> PostAsync<T>(string endpoint, object payload) where T : class
    {
        var jsonObject = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
        var result = await RequestAsync<T>(endpoint, HttpMethod.Post, content);
        return result;
    }
    private async Task<T> RequestAsync<T>(string endpoint, HttpMethod method, HttpContent content = null) where T : class
    {
        try
        {
            var fullUrl = GetFullUrl(endpoint);
            var message = new HttpRequestMessage()
            {
                Content = content,
                Method = method,
                RequestUri = new Uri(fullUrl)
            };
            var response = await m_httpClient.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError("Request failed:" + response.StatusCode);
                return null;
            }
            var stream = await response.Content.ReadAsStreamAsync();
            var resultBody = await new StreamReader(stream).ReadToEndAsync();
            var result = JsonConvert.DeserializeObject<T>(resultBody);
            return result as T;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    private async Task<T> GetAsync<T>(string endpoint) where T : class
    {
        return await RequestAsync<T>(endpoint, HttpMethod.Get);
    }
}

