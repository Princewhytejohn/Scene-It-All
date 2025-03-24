using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Screen Panels")]
    public GameObject apiInputPanel;  // API Key input panel
    public GameObject homePanel;      // Home screen panel
    public GameObject searchPanel;    // Search screen panel
    public GameObject detailsPanel;   // Details screen panel

    [Header("Placeholder")]
    public Texture2D placeholderTexture; // Placeholder texture for images

    [Header("Screen Controllers")]
    public HomeScreenController homeController;
    public SearchScreenController searchController;
    public DetailsScreenController detailsController;
    public APIKeyController apiKeyController;

    // List of recently viewed movies.
    public List<MovieResult> recentlyViewedMovies = new List<MovieResult>();

    private string apiKey;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optionally: DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeScreens();
    }

    /// <summary>
    /// Initializes the correct screen based on whether an API Key is stored.
    /// </summary>
    void InitializeScreens()
    {
        // Disable all panels.
        apiInputPanel.SetActive(false);
        homePanel.SetActive(false);
        searchPanel.SetActive(false);
        detailsPanel.SetActive(false);

        // Default to empty string if key is not found
        string storedKey = PlayerPrefs.GetString("TMDB_API_KEY", "");
        if (string.IsNullOrEmpty(storedKey))
        {
            // No API Key stored; show API input panel.
            apiInputPanel.SetActive(true);
        }
        else
        {
            // API Key exists; show Home screen.
            homePanel.SetActive(true);
        }
    }


    /// <summary>
    /// Called by APIKeyController once a valid API Key is saved.
    /// </summary>
    public void OnAPIKeyValidated()
    {
        InitializeScreens();
    }

    public string GetApiKey()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            string enc = PlayerPrefs.GetString("TMDB_API_KEY", "");
            if (string.IsNullOrEmpty(enc)) return "";
            try
            {
                byte[] data = System.Convert.FromBase64String(enc);
                apiKey = System.Text.Encoding.UTF8.GetString(data);
            }
            catch { apiKey = ""; }
        }
        return apiKey;
    }

    // --- Screen Switching Methods ---

    public void ShowHomeScreen()
    {
        apiInputPanel.SetActive(false);
        searchPanel.SetActive(false);
        detailsPanel.SetActive(false);
        homePanel.SetActive(true);
    }

    public void ShowSearchScreen()
    {
        StartCoroutine(SlideInPanel(searchPanel, true));
    }

    public void HideSearchScreen()
    {
        StartCoroutine(SlideOutPanel(searchPanel));
    }

    public void ShowDetailsScreen(MovieResult movie)
    {
        detailsPanel.SetActive(true);
        detailsController.ShowMovieDetails(movie);
        StartCoroutine(SlideInPanel(detailsPanel, fromRight: false)); // Slide in from left
    }

    public void HideDetailsScreen(bool showSearch = false)
    {
        if (showSearch)
        {
            // Force show search screen (slide in from right)
            StartCoroutine(SlideOutPanel(detailsPanel, toRight: false));
            StartCoroutine(SlideInPanel(searchPanel, fromRight: true));
        }
        else
        {
            // Just slide out details, reveal whatever was underneath
            StartCoroutine(SlideOutPanel(detailsPanel, toRight: false));
        }
    }

    public void ShowAPIKeyInputScreen()
    {
        homePanel.SetActive(false);
        searchPanel.SetActive(false);
        detailsPanel.SetActive(false);
        apiInputPanel.SetActive(true);
    }


    // --- Screen Transition Animations ---

    IEnumerator SlideInPanel(GameObject panel, bool fromRight)
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        Vector2 offScreenPos = new Vector2(fromRight ? Screen.width : -Screen.width, 0);
        Vector2 onScreenPos = Vector2.zero;
        rt.anchoredPosition = offScreenPos;
        panel.SetActive(true);

        float duration = 0.3f;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / duration;
            rt.anchoredPosition = Vector2.Lerp(offScreenPos, onScreenPos, normalizedTime);
            yield return null;
        }
        rt.anchoredPosition = onScreenPos;
    }

    IEnumerator SlideOutPanel(GameObject panel, bool toRight = true)
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        Vector2 onScreenPos = rt.anchoredPosition;
        Vector2 offScreenPos = new Vector2(toRight ? Screen.width : -Screen.width, 0);
        float duration = 0.3f;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / duration;
            rt.anchoredPosition = Vector2.Lerp(onScreenPos, offScreenPos, normalizedTime);
            yield return null;
        }
        rt.anchoredPosition = offScreenPos;
        panel.SetActive(false);
    }

    // --- Shared Utility ---

    public IEnumerator LoadPosterImage(string imagePath, RawImage targetImage, string size = "w185", bool isBackdrop = false)
    {
        if (targetImage == null)
            yield break;

        if (placeholderTexture != null)
            targetImage.texture = placeholderTexture;

        if (string.IsNullOrEmpty(imagePath))
            yield break;

        // Backdrops typically use wider sizes like w780 or w1280
        string selectedSize = isBackdrop ? "w780" : size;
        string url = $"https://image.tmdb.org/t/p/{selectedSize}{imagePath}";

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (targetImage == null) yield break;

        if (request.result == UnityWebRequest.Result.Success)
        {
            targetImage.texture = DownloadHandlerTexture.GetContent(request);
        }
        else
        {
            Debug.LogWarning("Failed to load image, using placeholder: " + request.error);
            targetImage.texture = placeholderTexture;
        }
    }



    // --- Recently Viewed ---

    public void AddToRecentlyViewed(MovieResult movie)
    {
        if (!recentlyViewedMovies.Exists(m => m.id == movie.id))
            recentlyViewedMovies.Add(movie);
    }
}
