using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class SearchScreenController : MonoBehaviour
{
    public GameObject searchPanel;
    public TMP_InputField searchInput;
    public Button clearButton;
    public Button backButton;

    public GameObject searchRelatedInfoContent;
    public GameObject recentlyViewedPanel;
    public GameObject trendingSearchPanel;
    public GameObject searchResultsPanel;
    public GameObject loadingText;
    public TextMeshProUGUI searchResultsLabel;

    public Transform searchResultsContent;
    public GameObject searchResultMovieItemPrefab;
    public GameObject sharedMovieItemPrefab;

    public Transform trendingSearchContent;
    public Transform recentlyViewedContent;

    public ScrollRect searchResultsScrollRect;

    private string apiKey;
    private Coroutine currentSearchCoroutine;

    // Pagination state
    private int currentPage = 1;
    private int totalPages = 1;
    private string currentQuery = "";
    private bool isFetchingMore = false;

    void OnEnable()
    {
        apiKey = UIManager.Instance.GetApiKey();
        searchInput.text = "";
        clearButton.gameObject.SetActive(false);
        ClearSearchResults();

        SetupTrendingAndRecentlyViewed();

        searchResultsPanel.SetActive(false);
        loadingText.SetActive(false);

        searchInput.onValueChanged.AddListener(OnSearchTextChanged);
        clearButton.onClick.AddListener(OnClearButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    void OnDisable()
    {
        searchInput.onValueChanged.RemoveListener(OnSearchTextChanged);
        clearButton.onClick.RemoveListener(OnClearButtonClicked);
        backButton.onClick.RemoveListener(OnBackButtonClicked);
    }

    void Update()
    {
        // Infinite scroll trigger
        if (searchResultsPanel.activeSelf && !isFetchingMore && currentPage < totalPages)
        {
            float y = searchResultsScrollRect.verticalNormalizedPosition;
            if (y <= 0.05f)
            {
                currentPage++;
                currentSearchCoroutine = StartCoroutine(SearchMovies(currentQuery, currentPage));
            }
        }
    }

    void OnSearchTextChanged(string query)
    {
        clearButton.gameObject.SetActive(!string.IsNullOrEmpty(query));

        if (string.IsNullOrEmpty(query))
        {
            SetupTrendingAndRecentlyViewed();
            searchResultsPanel.SetActive(false);
            loadingText.SetActive(false);

            if (currentSearchCoroutine != null)
            {
                StopCoroutine(currentSearchCoroutine);
                currentSearchCoroutine = null;
            }

            ClearSearchResults();
        }
        else
        {
            recentlyViewedPanel.SetActive(false);
            trendingSearchPanel.SetActive(false);
            searchResultsPanel.SetActive(true);
            loadingText.SetActive(true);

            if (currentSearchCoroutine != null)
                StopCoroutine(currentSearchCoroutine);

            currentQuery = query;
            currentPage = 1;
            totalPages = 1;

            ClearSearchResults();

            currentSearchCoroutine = StartCoroutine(SearchMovies(currentQuery, currentPage));
        }
    }

    void OnClearButtonClicked()
    {
        searchInput.text = "";
    }

    void OnBackButtonClicked()
    {
        UIManager.Instance.HideSearchScreen();
    }

    IEnumerator SearchMovies(string query, int page)
    {
        isFetchingMore = true;

        string url = $"https://api.themoviedb.org/3/search/movie?api_key={apiKey}&query={UnityWebRequest.EscapeURL(query)}&page={page}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        loadingText.SetActive(false);
        isFetchingMore = false;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Search request failed: " + request.error);
            yield break;
        }

        SearchResponse response = JsonUtility.FromJson<SearchResponse>(request.downloadHandler.text);

        totalPages = response.total_pages;
        currentPage = response.page;

        foreach (MovieResult movie in response.results)
        {
            GameObject item = Instantiate(searchResultMovieItemPrefab, searchResultsContent);

            RawImage posterImage = item.transform.Find("PosterImage").GetComponent<RawImage>();
            Transform details = item.transform.Find("Details");
            TextMeshProUGUI titleText = details.Find("TitleText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI synopsisText = details.Find("SynopsisText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI releaseDateText = details.Find("ReleaseDateText").GetComponent<TextMeshProUGUI>();

            titleText.text = movie.title;
            synopsisText.text = movie.overview;
            string year = !string.IsNullOrEmpty(movie.release_date) && movie.release_date.Length >= 4
    ? movie.release_date.Substring(0, 4)
    : "";

            string rating = movie.vote_average > 0
                ? movie.vote_average.ToString("0.0")
                : "no rating";

            releaseDateText.text = !string.IsNullOrEmpty(year) ? $"{year} | {rating}" : rating;



            StartCoroutine(UIManager.Instance.LoadPosterImage(movie.poster_path, posterImage));

            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                UIManager.Instance.AddToRecentlyViewed(movie);
                UIManager.Instance.ShowDetailsScreen(movie);
            });
        }

        if (currentPage == 1)
        {
            searchResultsLabel.gameObject.SetActive(true);
            searchResultsLabel.text = $"Search results for: <color=#FFD700>{query}</color> ({response.total_results})";

        }

        currentSearchCoroutine = null;
    }

    void ClearSearchResults()
    {
        foreach (Transform child in searchResultsContent)
        {
            Destroy(child.gameObject);
        }

        searchResultsLabel.gameObject.SetActive(false);
    }

    void SetupTrendingAndRecentlyViewed()
    {
        recentlyViewedPanel.transform.SetParent(null);
        trendingSearchPanel.transform.SetParent(null);

        ClearTrendingContent();
        ClearRecentlyViewedContent();

        bool hasRecentlyViewed = UIManager.Instance.recentlyViewedMovies.Count > 0;

        if (hasRecentlyViewed)
        {
            PopulateRecentlyViewed();
            recentlyViewedPanel.SetActive(true);
            recentlyViewedPanel.transform.SetParent(searchRelatedInfoContent.transform, false);
        }
        else
        {
            recentlyViewedPanel.SetActive(false);
        }

        StartCoroutine(PopulateTrendingMovies());
        trendingSearchPanel.SetActive(true);
        trendingSearchPanel.transform.SetParent(searchRelatedInfoContent.transform, false);
    }

    void ClearTrendingContent()
    {
        foreach (Transform child in trendingSearchContent)
        {
            Destroy(child.gameObject);
        }
    }

    void ClearRecentlyViewedContent()
    {
        foreach (Transform child in recentlyViewedContent)
        {
            Destroy(child.gameObject);
        }
    }

    public void ResetSearchScreen()
    {
        searchInput.text = "";
        ClearSearchResults();
        SetupTrendingAndRecentlyViewed();

        searchResultsPanel.SetActive(false);
        loadingText.SetActive(false);
        clearButton.gameObject.SetActive(false);
    }


    void PopulateRecentlyViewed()
    {
        foreach (var movie in UIManager.Instance.recentlyViewedMovies)
        {
            GameObject item = Instantiate(sharedMovieItemPrefab, recentlyViewedContent);

            RawImage posterImage = item.transform.Find("PosterImage").GetComponent<RawImage>();
            TextMeshProUGUI ratingText = item.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();

            ratingText.text = movie.vote_average > 0
    ? movie.vote_average.ToString("0.0")
    : "no rating";

            StartCoroutine(UIManager.Instance.LoadPosterImage(movie.poster_path, posterImage));

            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                UIManager.Instance.ShowDetailsScreen(movie);
            });
        }
    }

    IEnumerator PopulateTrendingMovies()
    {
        string url = $"https://api.themoviedb.org/3/trending/movie/day?api_key={apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to load trending movies.");
            yield break;
        }

        SearchResponse response = JsonUtility.FromJson<SearchResponse>(request.downloadHandler.text);

        foreach (var movie in response.results)
        {
            GameObject item = Instantiate(sharedMovieItemPrefab, trendingSearchContent);

            RawImage posterImage = item.transform.Find("PosterImage").GetComponent<RawImage>();
            TextMeshProUGUI ratingText = item.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();

            ratingText.text = movie.vote_average > 0
    ? movie.vote_average.ToString("0.0")
    : "no rating";

            StartCoroutine(UIManager.Instance.LoadPosterImage(movie.poster_path, posterImage));

            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                UIManager.Instance.AddToRecentlyViewed(movie);
                UIManager.Instance.ShowDetailsScreen(movie);
            });
        }
    }
}

[System.Serializable]
public class SearchResponse
{
    public int page;
    public List<MovieResult> results;
    public int total_results;
    public int total_pages;
}
