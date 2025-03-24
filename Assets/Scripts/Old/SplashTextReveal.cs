using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SplashTextReveal : MonoBehaviour
{
    public RectTransform maskRect;
    public float revealDuration = 2f;
    public string nextSceneName;

    private AsyncOperation sceneLoadOp;

    void Start()
    {
        // Start hidden
        maskRect.sizeDelta = new Vector2(0, maskRect.sizeDelta.y);

        // Begin loading the next scene in the background
        sceneLoadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        sceneLoadOp.allowSceneActivation = false;

        StartCoroutine(RevealText());
    }

    IEnumerator RevealText()
    {
        float elapsed = 0;
        float targetWidth = maskRect.parent.GetComponent<RectTransform>().rect.width;

        while (elapsed < revealDuration)
        {
            float width = Mathf.Lerp(0, targetWidth, elapsed / revealDuration);
            maskRect.sizeDelta = new Vector2(width, maskRect.sizeDelta.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        maskRect.sizeDelta = new Vector2(targetWidth, maskRect.sizeDelta.y);

        yield return new WaitForSeconds(1f);

        // Reveal done → activate scene
        sceneLoadOp.allowSceneActivation = true;
    }
}
