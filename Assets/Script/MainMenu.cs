using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;

    // Buttons
    private Button startButton;
    private Button settingsButton;
    private Button quitButton;
    private Button backButton;

    // Panels
    private VisualElement mainMenuPanel;
    private VisualElement settingsPanel;

    // Settings
    private Slider soundSlider;
    private Slider musicSlider;
    private Label soundValue;
    private Label musicValue;
    private DropdownField qualityDropdown;
    private Toggle fullscreenToggle;

    // === Thêm biến âm thanh ===
    public AudioClip buttonClickSound;
    private AudioSource audioSource;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        // Query panels
        mainMenuPanel = root.Q<VisualElement>("MainMenuPanel");
        settingsPanel = root.Q<VisualElement>("SettingsPanel");

        // Query buttons
        startButton = root.Q<Button>("StartButton");
        settingsButton = root.Q<Button>("SettingsButton");
        quitButton = root.Q<Button>("QuitButton");
        backButton = root.Q<Button>("BackButton");

        // Query settings controls
        soundSlider = root.Q<Slider>("SoundSlider");
        musicSlider = root.Q<Slider>("MusicSlider");
        soundValue = root.Q<Label>("SoundValue");
        musicValue = root.Q<Label>("MusicValue");
        qualityDropdown = root.Q<DropdownField>("QualityDropdown");
        fullscreenToggle = root.Q<Toggle>("FullscreenToggle");

        // === Lấy AudioSource trong scene ===
        audioSource = FindObjectOfType<AudioSource>();

        // Register button callbacks
        startButton?.RegisterCallback<ClickEvent>(OnStartPressed);
        settingsButton?.RegisterCallback<ClickEvent>(OnSettingsPressed);
        quitButton?.RegisterCallback<ClickEvent>(OnQuitPressed);
        backButton?.RegisterCallback<ClickEvent>(OnBackPressed);

        // Register settings callbacks
        soundSlider?.RegisterValueChangedCallback(OnSoundChanged);
        musicSlider?.RegisterValueChangedCallback(OnMusicChanged);
        qualityDropdown?.RegisterValueChangedCallback(OnQualityChanged);
        fullscreenToggle?.RegisterValueChangedCallback(OnFullscreenChanged);

        ShowMainMenu();
        LoadSettings();
    }

    void OnDisable()
    {
        startButton?.UnregisterCallback<ClickEvent>(OnStartPressed);
        settingsButton?.UnregisterCallback<ClickEvent>(OnSettingsPressed);
        quitButton?.UnregisterCallback<ClickEvent>(OnQuitPressed);
        backButton?.UnregisterCallback<ClickEvent>(OnBackPressed);

        soundSlider?.UnregisterValueChangedCallback(OnSoundChanged);
        musicSlider?.UnregisterValueChangedCallback(OnMusicChanged);
        qualityDropdown?.UnregisterValueChangedCallback(OnQualityChanged);
        fullscreenToggle?.UnregisterValueChangedCallback(OnFullscreenChanged);
    }

    // === Hàm phát âm thanh ===
    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }

    void OnStartPressed(ClickEvent evt)
    {
        PlayButtonSound();
        SaveSettings();
        SceneManager.LoadScene("Game");
    }

    void OnSettingsPressed(ClickEvent evt)
    {
        PlayButtonSound();
        ShowSettings();
    }

    void OnQuitPressed(ClickEvent evt)
    {
        PlayButtonSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnBackPressed(ClickEvent evt)
    {
        PlayButtonSound();
        SaveSettings();
        ShowMainMenu();
    }

    void ShowMainMenu()
    {
        mainMenuPanel?.RemoveFromClassList("hidden");
        settingsPanel?.AddToClassList("hidden");
    }

    void ShowSettings()
    {
        mainMenuPanel?.AddToClassList("hidden");
        settingsPanel?.RemoveFromClassList("hidden");
    }

    // Settings handlers
    void OnSoundChanged(ChangeEvent<float> evt)
    {
        if (soundValue != null)
            soundValue.text = $"{(int)evt.newValue}%";

        if (AudioMixerManager.Instance != null)
            AudioMixerManager.Instance.SetSFXVolume(evt.newValue);
    }

    void OnMusicChanged(ChangeEvent<float> evt)
    {
        if (musicValue != null)
            musicValue.text = $"{(int)evt.newValue}%";

        if (AudioMixerManager.Instance != null)
            AudioMixerManager.Instance.SetMusicVolume(evt.newValue);
    }

    void OnQualityChanged(ChangeEvent<string> evt)
    {
        int qualityIndex = evt.newValue switch
        {
            "Low" => 0,
            "Medium" => 1,
            "High" => 2,
            "Ultra" => 3,
            _ => 2
        };
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    void OnFullscreenChanged(ChangeEvent<bool> evt)
    {
        Screen.fullScreen = evt.newValue;
    }

    void LoadSettings()
    {
        if (soundSlider != null && AudioMixerManager.Instance != null)
        {
            float sound = AudioMixerManager.Instance.GetSFXVolume();
            soundSlider.value = sound;
            soundValue.text = $"{(int)sound}%";
        }

        if (musicSlider != null && AudioMixerManager.Instance != null)
        {
            float music = AudioMixerManager.Instance.GetMusicVolume();
            musicSlider.value = music;
            musicValue.text = $"{(int)music}%";
        }

        if (qualityDropdown != null)
        {
            string quality = PlayerPrefs.GetString("Quality", "High");
            qualityDropdown.value = quality;
        }

        if (fullscreenToggle != null)
        {
            bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.value = fullscreen;
        }
    }

    void SaveSettings()
    {
        if (qualityDropdown != null)
            PlayerPrefs.SetString("Quality", qualityDropdown.value);

        if (fullscreenToggle != null)
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.value ? 1 : 0);

        PlayerPrefs.Save();
    }
}