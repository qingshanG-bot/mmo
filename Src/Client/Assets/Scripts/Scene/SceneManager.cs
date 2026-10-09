using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SceneManager : MonoSingleton<SceneManager>
{
    public event UnityAction<float> OnProgress;

    public event UnityAction<string> OnSceneLoaded;

    [SerializeField] private LoadingUI loadingUI; 


    public void LoadScene(string name)
    {
        StartCoroutine(LoadLevel(name));
    }

    IEnumerator LoadLevel(string name)
    {
        Debug.LogFormat("LoadLevel: {0}", name);

        LoadingUI.Instance.Show();

        yield return null;

        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);

            OnProgress?.Invoke(progress);

            if (async.progress >= 0.9f)
            {
                OnProgress?.Invoke(1f);
                async.allowSceneActivation = true;
            }

            yield return null;
        }

        LoadingUI.Instance.Hide();

        // 最少改动：加载结束后通知
        OnSceneLoaded?.Invoke(name);
    }

}
