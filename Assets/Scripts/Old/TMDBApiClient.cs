using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class TMDBApiClient
{
    public static string ApiKey = "";  // TMDb API key will be set at runtime
    private const string BaseUrl = "https://api.themoviedb.org/3";
    private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500"; // w500 for poster width ~500px

    // Search for movies by title
    public static IEnumerator SearchMovies(string query, Action<List<MovieInfo>> onSuccess, Action<string> onError)
    {
        // Ensure API key is set
        if (string.IsNullOrEmpty(ApiKey))
        {
            onError?.Invoke("Missing API Key");
            yield break;
        }
        string url = $"{BaseUrl}/search/movie?api_key={ApiKey}&query={UnityWebRequest.EscapeURL(query)}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            // Network or HTTP error occurred
            onError?.Invoke(request.error);
        }
        else
        {
            // Parse JSON response
            string json = request.downloadHandler.text;
            MovieSearchResults searchResults = JsonUtility.FromJson<MovieSearchResults>(json);
            onSuccess?.Invoke(searchResults.results);
        }
    }

    // Load an image from TMDb (poster) and convert to a Unity Sprite
    public static IEnumerator LoadImage(string imagePath, Action<Sprite> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            onError?.Invoke("No image path");
            yield break;
        }
        string url = ImageBaseUrl + imagePath;
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
        }
        else
        {
            // Get downloaded texture and create a Sprite
            Texture2D tex = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            onSuccess?.Invoke(sprite);
        }
    }
}
