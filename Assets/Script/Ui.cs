using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // <--- added

public class Ui : MonoBehaviour
{
    [Header("References")]
    public GameObject mainMenuPanel;   // panel chứa Start/Setting/Quit
    public GameObject settingsPanel;   // panel chứa cài đặt (Back button)
    public Button settingsBackButton;  // Back button (gán trong Inspector hoặc để script tự tìm)

    [Header("Scene")]
    public string gameSceneName = "Game"; // tên scene game, chỉnh trong Inspector

    void Start()
    {
        ShowMainMenu();

        // nếu chưa gán, thử tự tìm nút Back với tên "BackButton" nằm trong settingsPanel
        if (settingsBackButton == null && settingsPanel != null)
        {
            var t = settingsPanel.transform.Find("BackButton");
            if (t != null) settingsBackButton = t.GetComponent<Button>();
        }

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnBackFromSettings);
    }

    void OnDestroy()
    {
        if (settingsBackButton != null)
            settingsBackButton.onClick.RemoveListener(OnBackFromSettings);
    }

    // gọi vào OnClick của nút Start
    public void OnStartPressed()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("Game scene name is empty.");
            return;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    // gọi vào OnClick của nút Setting
    public void OnSettingsPressed()
    {
        ShowSettings();
    }

    // gọi vào OnClick của nút Quit
    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // gọi vào OnClick của nút Back trong Settings
    public void OnBackFromSettings()
    {
        ShowMainMenu();
    }

    void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void ShowSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
}
