using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;
    [SerializeField] Image progressBar;

    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Loading");
    }

    IEnumerator LoadScene()
    {
        yield return null;

        if (NetworkManager.Singleton != null)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(nextScene);
            asyncOperation.allowSceneActivation = false;
            float timer = 0.0f;
            while (!asyncOperation.isDone)
            {
                yield return null;
                timer += Time.deltaTime;
                if (asyncOperation.progress < 0.9f)
                {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, asyncOperation.progress, timer);
                    if (progressBar.fillAmount >= asyncOperation.progress)
                    {
                        timer = 0f;
                    }
                }
                else
                {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);
                    if (progressBar.fillAmount == 1.0f)
                    {
                        asyncOperation.allowSceneActivation = true;
                        yield break;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null. Unable to load scene.");
        }
    }
}
