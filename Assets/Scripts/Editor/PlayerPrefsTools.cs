using UnityEditor;
using UnityEngine;

public class PlayerPrefsTools
{
    [MenuItem("Tools/Clear PlayerPrefs Key")]
    public static void ClearSpecificKey()
    {
        string key = "TMDB_API_KEY";
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Debug.Log($"PlayerPrefs key '{key}' deleted.");
        }
        else
        {
            Debug.LogWarning($"PlayerPrefs key '{key}' does not exist.");
        }
    }
}
