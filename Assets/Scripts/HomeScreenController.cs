using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class HomeScreenController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject moviesPanelParent; // Parent of both Trending and Popular sections
    public Button retryButton;

    public GameObject homePanel;
    public Button searchIconButton;

    [Header("Movies Sections")]
    public TextMeshProUGUI loadingText;
    public GameObject spinner;
    public Transform popularMoviesContent;
    public GameObject popularMovieItemPrefab;
    public Transform trendingMoviesContent;
    public GameObject trendingMovieItemPrefab;

    private string apiKey;
    private bool popularLoaded = false;
    private bool trendingLoaded = false;

    void OnEnable()
    {
        apiKey = UIManager.Instance.GetApiKey();
        popularLoaded = false;
        trendingLoaded = false;

        if (moviesPanelParent != null)
            moviesPanelParent.SetActive(false);

        /*loadingText.text = "Loading...";
        loadingText.gameObject.SetActive(true);*/
        spinner.SetActive(true);
        retryButton.gameObject.SetActive(false);

        foreach (Transform child in popularMoviesContent)
            Destroy(child.gameObject);
        foreach (Transform child in trendingMoviesContent)
            Destroy(child.gameObject);

        StartCoroutine(FetchPopularMovies());
        StartCoroutine(FetchTrendingMovies());

        searchIconButton.onClick.RemoveAllListeners();
        searchIconButton.onClick.AddListener(() => UIManager.Instance.ShowSearchScreen());

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(RetryLoading);
    }

    void RetryLoading()
    {
        OnEnable();
    }

    IEnumerator FetchPopularMovies()
    {
        string url = $"https://api.themoviedb.org/3/movie/popular?api_key={apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 10;
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.downloadHandler.text.Contains("\"status_code\":"))
            {
                HandleApiError(request);
                popularLoaded = false;
                CheckLoadingComplete();
                yield break;
            }

            PopularMoviesResponse response = JsonUtility.FromJson<PopularMoviesResponse>(request.downloadHandler.text);
            foreach (MovieResult movie in response.results)
            {
                GameObject item = Instantiate(popularMovieItemPrefab, popularMoviesContent);
                RawImage posterImage = item.transform.Find("PosterImage").GetComponent<RawImage>();
                TextMeshProUGUI ratingText = item.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();

                ratingText.text = movie.vote_average > 0 ? movie.vote_average.ToString("0.0") : "no rating";
                StartCoroutine(UIManager.Instance.LoadPosterImage(movie.poster_path, posterImage));

                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    UIManager.Instance.AddToRecentlyViewed(movie);
                    UIManager.Instance.ShowDetailsScreen(movie);
                });
            }

            popularLoaded = true;
            CheckLoadingComplete();
        }
        else
        {
            HandleApiError(request);
            popularLoaded = false;
            CheckLoadingComplete();
        }
    }

    IEnumerator FetchTrendingMovies()
    {
        string url = $"https://api.themoviedb.org/3/trending/movie/day?api_key={apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 10;
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.downloadHandler.text.Contains("\"status_code\":"))
            {
                HandleApiError(request);
                trendingLoaded = false;
                CheckLoadingComplete();
                yield break;
            }

            TrendingMoviesResponse response = JsonUtility.FromJson<TrendingMoviesResponse>(request.downloadHandler.text);
            foreach (MovieResult movie in response.results)
            {
                GameObject item = Instantiate(trendingMovieItemPrefab, trendingMoviesContent);
                RawImage posterImage = item.transform.Find("PosterImage").GetComponent<RawImage>();
                TextMeshProUGUI ratingText = item.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();

                ratingText.text = movie.vote_average > 0 ? movie.vote_average.ToString("0.0") : "no rating";
                StartCoroutine(UIManager.Instance.LoadPosterImage(movie.poster_path, posterImage));

                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    UIManager.Instance.AddToRecentlyViewed(movie);
                    UIManager.Instance.ShowDetailsScreen(movie);
                });
            }

            trendingLoaded = true;
            CheckLoadingComplete();
        }
        else
        {
            HandleApiError(request);
            trendingLoaded = false;
            CheckLoadingComplete();
        }
    }

    void CheckLoadingComplete()
    {
        if (popularLoaded && trendingLoaded)
        {
            //loadingText.gameObject.SetActive(false);
            spinner.SetActive(false);
            retryButton.gameObject.SetActive(false);
            moviesPanelParent.SetActive(true);
        }
        else
        {
            retryButton.gameObject.SetActive(true);
        }
    }

    void HandleApiError(UnityWebRequest request)
    {
        string message;

        try
        {
            TMDbError error = JsonUtility.FromJson<TMDbError>(request.downloadHandler.text);
            message = error.status_message ?? "An unknown error occurred.";

            if (!string.IsNullOrEmpty(message) && message.ToLower().Contains("api key"))
            {
                UIManager.Instance.ShowAPIKeyInputScreen();
                return;
            }
        }
        catch
        {
            message = request.error ?? "A network error occurred.";
        }

        //loadingText.text = message;
        Debug.LogWarning($"API error: {message}");
    }
}

[System.Serializable]
public class TMDbError
{
    public int status_code;
    public string status_message;
}

[System.Serializable]
public class PopularMoviesResponse
{
    public int page;
    public List<MovieResult> results;
}

[System.Serializable]
public class TrendingMoviesResponse
{
    public int page;
    public List<MovieResult> results;
}

[System.Serializable]
public class MovieResult
{
    public int id;
    public string title;
    public string overview;
    public string poster_path;
    public string backdrop_path;
    public string release_date;
    public float vote_average;
    public int vote_count;
}
