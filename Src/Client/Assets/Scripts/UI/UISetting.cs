using Services;
using UnityEngine;
using UnityEngine.UI;

public class UISetting : UIWindow
{
    [Header("Setting Toggles")]
    public Toggle toggleFullscreen; // true=全屏 false=窗口
    public Toggle toggleHigh;       // true=2560x1440
    public Toggle toggleLow;        // true=1920x1080

    private const int WIDTH_HIGH = 2560;
    private const int HEIGHT_HIGH = 1440;

    private const int WIDTH_LOW = 1920;   // 你要1960就改成 1960
    private const int HEIGHT_LOW = 1080;

    private const string PREF_FULLSCREEN = "Setting_Fullscreen";
    private const string PREF_QUALITY = "Setting_Quality"; // 1=High 0=Low

    private bool _ignoreEvents = false;

    private void Start()
    {
        HookToggleEvents();
        LoadApplyAndRefreshUI();
    }

    private void HookToggleEvents()
    {
        if (toggleFullscreen != null)
        {
            toggleFullscreen.onValueChanged.RemoveListener(OnFullscreenChanged);
            toggleFullscreen.onValueChanged.AddListener(OnFullscreenChanged);
        }

        if (toggleHigh != null)
        {
            toggleHigh.onValueChanged.RemoveListener(OnHighChanged);
            toggleHigh.onValueChanged.AddListener(OnHighChanged);
        }

        if (toggleLow != null)
        {
            toggleLow.onValueChanged.RemoveListener(OnLowChanged);
            toggleLow.onValueChanged.AddListener(OnLowChanged);
        }
    }

    private void LoadApplyAndRefreshUI()
    {
        bool fullscreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
        bool high = PlayerPrefs.GetInt(PREF_QUALITY, 0) == 1; // 默认低；想默认高改成 1

        Apply(fullscreen, high);

        _ignoreEvents = true;
        if (toggleFullscreen != null) toggleFullscreen.isOn = fullscreen;
        if (toggleHigh != null) toggleHigh.isOn = high;
        if (toggleLow != null) toggleLow.isOn = !high;
        _ignoreEvents = false;
    }

    // ===== Toggle callbacks =====

    // 全屏/窗口
    public void OnFullscreenChanged(bool isFull)
    {
        if (_ignoreEvents) return;

        bool high = (toggleHigh != null) ? toggleHigh.isOn : (PlayerPrefs.GetInt(PREF_QUALITY, 0) == 1);
        Apply(isFull, high);
        Save(isFull, high);
    }

    // 高画质（2560）
    public void OnHighChanged(bool isOn)
    {
        if (_ignoreEvents) return;
        if (!isOn) return; // 只处理被打开

        _ignoreEvents = true;
        if (toggleLow != null) toggleLow.isOn = false; // 互斥
        _ignoreEvents = false;

        bool fullscreen = (toggleFullscreen != null) ? toggleFullscreen.isOn : (PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1);
        Apply(fullscreen, true);
        Save(fullscreen, true);
    }

    // 低画质（1920/1960）
    public void OnLowChanged(bool isOn)
    {
        if (_ignoreEvents) return;
        if (!isOn) return;

        _ignoreEvents = true;
        if (toggleHigh != null) toggleHigh.isOn = false; // 互斥
        _ignoreEvents = false;

        bool fullscreen = (toggleFullscreen != null) ? toggleFullscreen.isOn : (PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1);
        Apply(fullscreen, false);
        Save(fullscreen, false);
    }

    // ===== Apply & Save =====

    private void Apply(bool fullscreen, bool high)
    {
        int w = high ? WIDTH_HIGH : WIDTH_LOW;
        int h = high ? HEIGHT_HIGH : HEIGHT_LOW;

        Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(w, h, fullscreen);
    }

    private void Save(bool fullscreen, bool high)
    {
        PlayerPrefs.SetInt(PREF_FULLSCREEN, fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(PREF_QUALITY, high ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ===== 你原来的功能保持 =====

    public void ExitToCharSelect()
    {
        Managers.ChatManager.Instance.Init();
        SceneManager.Instance.LoadScene("CharSelect");
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Select);
        UserService.Instance.SendGameLeave();
    }

    public void SystemConfig()
    {
        UIManager.Instance.Show<UISystemConfig>();
        this.Close();
    }

    public void ExitGame()
    {
        UserService.Instance.SendGameLeave(true);
    }
}
