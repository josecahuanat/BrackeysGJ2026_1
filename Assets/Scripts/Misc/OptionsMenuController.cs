using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenuController : MonoBehaviour
{
    private const string MasterVolumeKey = "options.masterVolume";
    private const string FullScreenKey = "options.fullScreen";
    private const string LanguageKey = "options.language";

    [Header("Master Volume")]
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Full Screen")]
    [SerializeField] private Toggle fullScreenToggle;

    [Header("Language")]
    [SerializeField] private Button prevLangButton;
    [SerializeField] private Button nextLangButton;
    [SerializeField] private TMP_Text langButtonText;
    [SerializeField] private Text langButtonTextLegacy;
    [SerializeField] private string[] languageCodes = { "en", "es" };
    [SerializeField] private string[] languageDisplayNames = { "English", "Spanish" };

    [Header("Defaults")]
    [SerializeField, Range(0f, 1f)] private float defaultMasterVolume = 1f;
    [SerializeField] private bool defaultFullScreen = true;
    [SerializeField] private string defaultLanguage = "es";

    private bool suppressCallbacks;
    private int currentLanguageIndex;

    public float MasterVolume { get; private set; }
    public bool IsFullScreen { get; private set; }
    public string CurrentLanguage { get; private set; }

    public event System.Action<float> MasterVolumeChanged;
    public event System.Action<bool> FullScreenChanged;
    public event System.Action<string> LanguageChanged;

    private void Awake()
    {
        LoadValues();
        BindEvents();
        ApplyValuesToUI();
        ApplyRuntimeVolumes();
        ApplyRuntimeFullScreen();
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.RemoveListener(OnFullScreenToggleChanged);

        if (prevLangButton != null)
            prevLangButton.onClick.RemoveListener(PrevLanguage);

        if (nextLangButton != null)
            nextLangButton.onClick.RemoveListener(NextLanguage);
    }

    private void BindEvents()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterSliderChanged);

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.AddListener(OnFullScreenToggleChanged);

        if (prevLangButton != null)
            prevLangButton.onClick.AddListener(PrevLanguage);

        if (nextLangButton != null)
            nextLangButton.onClick.AddListener(NextLanguage);
    }

    private void LoadValues()
    {
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, defaultMasterVolume));
        IsFullScreen = PlayerPrefs.GetInt(FullScreenKey, defaultFullScreen ? 1 : 0) == 1;
        CurrentLanguage = PlayerPrefs.GetString(LanguageKey, defaultLanguage);
    }

    private void ApplyValuesToUI()
    {
        suppressCallbacks = true;

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = MasterVolume;

        if (fullScreenToggle != null)
            fullScreenToggle.isOn = IsFullScreen;

        suppressCallbacks = false;

        currentLanguageIndex = FindLanguageIndex(CurrentLanguage);
        if (currentLanguageIndex < 0)
            currentLanguageIndex = 0;

        ApplyLanguageToUI();
    }

    private void OnMasterSliderChanged(float value)
    {
        if (suppressCallbacks)
            return;

        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.Save();

        ApplyRuntimeVolumes();
        MasterVolumeChanged?.Invoke(MasterVolume);
    }

    private void OnFullScreenToggleChanged(bool value)
    {
        if (suppressCallbacks)
            return;

        IsFullScreen = value;
        PlayerPrefs.SetInt(FullScreenKey, IsFullScreen ? 1 : 0);
        PlayerPrefs.Save();

        ApplyRuntimeFullScreen();
        FullScreenChanged?.Invoke(IsFullScreen);
    }

    public void PrevLanguage()
    {
        if (languageCodes == null || languageCodes.Length == 0)
            return;

        int nextIndex = currentLanguageIndex - 1;
        if (nextIndex < 0)
            nextIndex = languageCodes.Length - 1;

        SetLanguageByIndex(nextIndex);
    }

    public void NextLanguage()
    {
        if (languageCodes == null || languageCodes.Length == 0)
            return;

        int nextIndex = currentLanguageIndex + 1;
        if (nextIndex >= languageCodes.Length)
            nextIndex = 0;

        SetLanguageByIndex(nextIndex);
    }

    private void SetLanguageByIndex(int index)
    {
        if (languageCodes == null || languageCodes.Length == 0)
            return;

        if (index < 0 || index >= languageCodes.Length)
            return;

        currentLanguageIndex = index;
        CurrentLanguage = languageCodes[currentLanguageIndex];
        PlayerPrefs.SetString(LanguageKey, CurrentLanguage);
        PlayerPrefs.Save();

        ApplyLanguageToUI();
        LanguageChanged?.Invoke(CurrentLanguage);
    }

    private int FindLanguageIndex(string code)
    {
        if (languageCodes == null || languageCodes.Length == 0)
            return -1;

        for (int i = 0; i < languageCodes.Length; i++)
        {
            if (string.Equals(languageCodes[i], code, System.StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private void ApplyLanguageToUI()
    {
        string display = GetLanguageDisplay(currentLanguageIndex);

        if (langButtonText != null)
            langButtonText.text = display;

        if (langButtonTextLegacy != null)
            langButtonTextLegacy.text = display;
    }

    private string GetLanguageDisplay(int index)
    {
        if (languageDisplayNames != null && index >= 0 && index < languageDisplayNames.Length && !string.IsNullOrEmpty(languageDisplayNames[index]))
            return languageDisplayNames[index];

        if (languageCodes != null && index >= 0 && index < languageCodes.Length)
            return languageCodes[index].ToUpperInvariant();

        return "English";
    }

    private void ApplyRuntimeVolumes()
    {
        AudioListener.volume = MasterVolume;
    }

    private void ApplyRuntimeFullScreen()
    {
        Screen.fullScreen = IsFullScreen;
    }
}
