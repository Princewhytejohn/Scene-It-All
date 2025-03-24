using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Collections;

public class APIKeyController : MonoBehaviour
{
    public TMP_InputField apiInputField;
    public Button saveButton;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI loadingText; // Assign in Inspector

    private const string APIKeyPref = "TMDB_API_KEY";

    void Start()
    {
        instructionText.text = "Enter your API Key:";
        errorText.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);

        saveButton.onClick.AddListener(OnSaveButtonClicked);
    }

    void OnSaveButtonClicked()
    {
        errorText.text = "";
        errorText.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);

        string key = apiInputField.text.Trim();

        if (string.IsNullOrEmpty(key))
        {
            errorText.text = "API Key cannot be empty.";
            errorText.gameObject.SetActive(true);
            return;
        }

        StartCoroutine(ValidateAndSaveToken(key));
    }

    IEnumerator ValidateAndSaveToken(string key)
    {
        saveButton.interactable = false;
        loadingText.gameObject.SetActive(true);

        string testUrl = $"https://api.themoviedb.org/3/configuration?api_key={key}";

        UnityWebRequest req = UnityWebRequest.Get(testUrl);
        req.timeout = 10;

        yield return req.SendWebRequest();

        loadingText.gameObject.SetActive(false);
        saveButton.interactable = true;

        if (req.result == UnityWebRequest.Result.Success)
        {
            if (req.downloadHandler.text.Contains("\"status_code\":"))
            {
                TMDbError errorResponse = JsonUtility.FromJson<TMDbError>(req.downloadHandler.text);
                errorText.text = errorResponse.status_message ?? "Invalid API Key.";
                errorText.gameObject.SetActive(true);
                yield break;
            }

            string encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key));
            PlayerPrefs.SetString(APIKeyPref, encoded);
            PlayerPrefs.Save();

            UIManager.Instance.OnAPIKeyValidated();
        }
        else
        {
            string message;

            try
            {
                TMDbError errorResponse = JsonUtility.FromJson<TMDbError>(req.downloadHandler.text);
                message = errorResponse.status_message ?? "An error occurred.";
            }
            catch
            {
                message = req.result == UnityWebRequest.Result.ConnectionError
        ? "Network connection error. Please check your internet and try again."
        : req.error ?? "An unknown error occurred.";
            }

            errorText.text = message;
            errorText.gameObject.SetActive(true);

            Debug.LogWarning($"API Key validation failed: {message}");
        }
    }

    [Serializable]
    public class TMDbError
    {
        public int status_code;
        public string status_message;
    }
}
