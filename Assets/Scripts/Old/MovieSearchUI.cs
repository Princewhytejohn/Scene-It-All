using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MovieSearchUI : MonoBehaviour
{
    [Header("Search UI Elements")]
    public TMP_InputField searchInput;
    public Button searchButton;
    public Transform resultsContent;   // The Content transform of the Scroll View
    public GameObject movieItemPrefab;

    [Header("API Key UI Elements")]
    public GameObject apiKeyPanel;    // Panel that asks for API key on first launch
    public TMP_InputField apiKeyInput;
    public Button apiKeySaveButton;

    [Header("Misc UI Elements")]
    public TextMeshProUGUI messageText;  // Text UI for displaying messages (errors, no results, etc)

    void Start()
    {
        // First launch: check if API key is saved, if not, show the API key input panel
        if (PlayerPrefs.HasKey("TMDB_APIKey"))
        {
            string savedKey = PlayerPrefs.GetString("TMDB_APIKey");
            TMDBApiClient.ApiKey = savedKey;
            apiKeyPanel.SetActive(false);
        }
        else
        {
            apiKeyPanel.SetActive(true);
        }

        // Hook up button listeners
        searchButton.onClick.AddListener(OnSearchClicked);
        apiKeySaveButton.onClick.AddListener(OnSaveApiKey);
    }

    void OnSaveApiKey()
    {
        string key = apiKeyInput.text.Trim();
        if (!string.IsNullOrEmpty(key))
        {
            // Save the API key for future runs
            PlayerPrefs.SetString("TMDB_APIKey", key);
            PlayerPrefs.Save();
            TMDBApiClient.ApiKey = key;
            apiKeyPanel.SetActive(false);
            Debug.Log("API Key saved.");
        }
    }

    // Called when the Search button is clicked
    void OnSearchClicked()
    {
        string query = searchInput.text.Trim();
        if (string.IsNullOrEmpty(query))
        {
            return; // ignore empty search
        }
        if (string.IsNullOrEmpty(TMDBApiClient.ApiKey))
        {
            messageText.text = "Please enter your TMDb API key.";
            return;
        }
        // Clear any previous results from the UI
        foreach (Transform child in resultsContent)
        {
            Destroy(child.gameObject);
        }
        messageText.text = "Searching...";  // indicate that search started

        // Start coroutine to call TMDb API (asynchronously)
        StartCoroutine(TMDBApiClient.SearchMovies(query, OnSearchSuccess, OnSearchError));
    }


    void OnSearchSuccess(List<MovieInfo> movies)
    {
        // This is called when TMDBApiClient.SearchMovies successfully gets data
        messageText.text = "";  // clear status message
        if (movies == null || movies.Count == 0)
        {
            messageText.text = "No results found.";
            return;
        }
        // Populate the scroll list with results
        foreach (MovieInfo movie in movies)
        {
            // Instantiate a new UI item from the prefab
            GameObject itemGO = Instantiate(movieItemPrefab, resultsContent);
            // Set movie title text
            TextMeshProUGUI titleText = itemGO.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            titleText.text = movie.title;
            // Load and set movie poster image
            Image posterImage = itemGO.transform.Find("Poster").GetComponent<Image>();
            // Optionally: set a placeholder graphic or color on posterImage while loading
            // Start image download coroutine
            StartCoroutine(TMDBApiClient.LoadImage(movie.poster_path,
                sprite => { posterImage.sprite = sprite; },
                error => {
                    posterImage.sprite = null; // or keep placeholder
                    Debug.LogWarning("Failed to load image: " + error);
                }
            ));
        }
    }

    void OnSearchError(string errorMessage)
    {
        // This is called if the search request fails (network error, etc.)
        messageText.text = "Error: " + errorMessage;
        Debug.LogError("Search error: " + errorMessage);
        // If the error is unauthorized (likely invalid API key), prompt for API key again
        if (errorMessage.Contains("401"))
        {  // 401 Unauthorized
            PlayerPrefs.DeleteKey("TMDB_APIKey");
            apiKeyPanel.SetActive(true);
            messageText.text = "Invalid API Key. Please enter a valid key.";
        }
    }


}
