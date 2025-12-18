using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ui : MonoBehaviour
{
    [Header("References")]
    public GameObject mainMenuPanel;   // kéo mainmenu vào đây
    public GameObject settingsPanel;   // kéo settingmenu vào đây
    public Button settingsBackButton;  // Back button trong settings

    [Header("Scene")]
    public string gameSceneName = "Game"; // tên scene game

    void Start()
    {
        ShowMainMenu();

        // tự tìm nút Back nếu chưa gán
        if (settingsBackButton == null && settingsPanel != null)
        {
            var backBtn = settingsPanel.GetComponentInChildren<Button>();
            if (backBtn != null && backBtn.name.ToLower().Contains("back"))
                settingsBackButton = backBtn;
        }

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnBackFromSettings);
    }

    void OnDestroy()
    {
        if (settingsBackButton != null)
            settingsBackButton.onClick.RemoveListener(OnBackFromSettings);
    }

    // nút Start: tắt menu, bắt đầu game
    public void OnStartPressed()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        // bắt đầu gameplay (có thể thêm logic khác ở đây)
    }

    // nút Setting: hiện settings, ẩn main menu
    public void OnSettingsPressed()
    {
        ShowSettings();
    }

    // nút Quit: thoát game
    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // nút Back từ Settings: quay về main menu
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

    // method public để gọi từ ngoài nếu cần quay lại menu
    public void ReturnToMainMenu()
    {
        ShowMainMenu();
        
        // tắt gameplay elements
        var player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.enabled = false;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
