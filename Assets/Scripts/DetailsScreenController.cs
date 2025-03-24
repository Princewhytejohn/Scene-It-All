using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class DetailsScreenController : MonoBehaviour
{
    public GameObject detailsPanel;
    public RawImage posterImage;
    public TextMeshProUGUI mainTitleText;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI synopsisText;
    public TextMeshProUGUI metadataText;
    public Button backButton;
    public Button searchIconButton;

    public Transform castContent;
    public GameObject castItemPrefab;

    public void ShowMovieDetails(MovieResult movie)
    {
        // Set static fields
        mainTitleText.text = movie.title;
        subtitleText.text = movie.title;
        synopsisText.text = movie.overview;

        // Start async loads
        StartCoroutine(LoadBackdrop(movie));
        StartCoroutine(LoadCast(movie.id));
        StartCoroutine(LoadGenresAndMetadata(movie));
    }

    IEnumerator LoadBackdrop(MovieResult movie)
    {
        string imagePath = string.IsNullOrEmpty(movie.backdrop_path) ? movie.poster_path : movie.backdrop_path;
        yield return UIManager.Instance.LoadPosterImage(imagePath, posterImage, "w780", isBackdrop: true);
    }

    IEnumerator LoadCast(int movieId)
    {
        string url = $"https://api.themoviedb.org/3/movie/{movieId}/credits?api_key={UIManager.Instance.GetApiKey()}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Failed to load cast: " + request.error);
            yield break;
        }

        CreditsResponse response = JsonUtility.FromJson<CreditsResponse>(request.downloadHandler.text);

        // Clear existing cast items
        foreach (Transform child in castContent)
            Destroy(child.gameObject);

        // Show top 5 cast members
        int maxCast = Mathf.Min(response.cast.Count, 5);
        for (int i = 0; i < maxCast; i++)
        {
            CastMember cast = response.cast[i];

            GameObject item = Instantiate(castItemPrefab, castContent);
            TextMeshProUGUI nameText = item.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            RawImage photo = item.transform.Find("ProfileImage").GetComponent<RawImage>();

            nameText.text = $"{cast.name}\n<size=80%><color=#AAAAAA>{cast.character}</color></size>";

            if (!string.IsNullOrEmpty(cast.profile_path))
            {
                string imgUrl = $"https://image.tmdb.org/t/p/w185{cast.profile_path}";
                UnityWebRequest imgReq = UnityWebRequestTexture.GetTexture(imgUrl);
                yield return imgReq.SendWebRequest();

                if (imgReq.result == UnityWebRequest.Result.Success)
                    photo.texture = DownloadHandlerTexture.GetContent(imgReq);
            }
        }
    }

    IEnumerator LoadGenresAndMetadata(MovieResult movie)
    {
        string url = $"https://api.themoviedb.org/3/movie/{movie.id}?api_key={UIManager.Instance.GetApiKey()}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Failed to fetch movie details/genres: " + request.error);
            yield break;
        }

        GenreResponse response = JsonUtility.FromJson<GenreResponse>(request.downloadHandler.text);

        // Year
        string year = !string.IsNullOrEmpty(movie.release_date) && movie.release_date.Length >= 4
            ? movie.release_date.Substring(0, 4)
            : "";

        // Rating
        string rating = movie.vote_average > 0 ? movie.vote_average.ToString("0.0") : "no rating";

        // Votes
        string voteCount = movie.vote_count > 0 ? $" ({movie.vote_count})" : "";

        // Genres (limit to 3)
        List<string> genreNames = new List<string>();
        int genreLimit = Mathf.Min(response.genres.Count, 3);
        for (int i = 0; i < genreLimit; i++)
        {
            genreNames.Add(response.genres[i].name);
        }

        string genreText = genreNames.Count > 0 ? $" | {string.Join(", ", genreNames)}" : "";

        // Final metadata
        metadataText.text = $"{year} | {rating}{voteCount}{genreText}";
    }

    void OnEnable()
    {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(OnBackButtonClicked);

        searchIconButton.onClick.RemoveAllListeners();
        searchIconButton.onClick.AddListener(OnSearchButtonClicked);
    }

    void OnDisable()
    {
        backButton.onClick.RemoveAllListeners();
        searchIconButton.onClick.RemoveAllListeners();
    }

    void OnBackButtonClicked()
    {
        UIManager.Instance.HideDetailsScreen();
    }

    void OnSearchButtonClicked()
    {
        UIManager.Instance.HideDetailsScreen(showSearch: true);
    }
}

[System.Serializable]
public class CreditsResponse
{
    public List<CastMember> cast;
}

[System.Serializable]
public class CastMember
{
    public string name;
    public string character;
    public string profile_path;
}

[System.Serializable]
public class GenreResponse
{
    public List<Genre> genres;
}

[System.Serializable]
public class Genre
{
    public int id;
    public string name;
}
