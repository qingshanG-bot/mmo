using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoSingleton<LoadingUI>
{
    public GameObject panel;
    public Slider slider;
    public Text percentText;

    protected override void OnStart()
    {
        if (panel != null) panel.SetActive(false);
        else Debug.LogError("[LoadingUI] panel is NULL!");

        SceneManager.Instance.OnProgress += OnProgress;
    }

   

    public void Show()
    {
        if (panel == null) { Debug.LogError("[LoadingUI] panel is NULL"); return; }
        panel.SetActive(true);

        if (slider == null) Debug.LogError("[LoadingUI] slider is NULL");
        else slider.value = 0;

        if (percentText != null) percentText.text = "0%";
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    void OnProgress(float progress)
    {
        if (slider != null) slider.value = progress;
        if (percentText != null) percentText.text = (progress * 100).ToString("F0") + "%";
    }
}
