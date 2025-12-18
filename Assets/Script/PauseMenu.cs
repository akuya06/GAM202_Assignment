using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartButton;
    public Button menuButton; // Đổi tên từ quitButton thành menuButton

    bool isPaused = false;

    void Awake()
    {
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Start()
    {
        // wire buttons
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (menuButton != null) menuButton.onClick.AddListener(GoToMenu); // Đổi thành GoToMenu
    }

    void OnDestroy()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveListener(Resume);
        if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
        if (menuButton != null) menuButton.onClick.RemoveListener(GoToMenu); // Đổi thành GoToMenu
    }


    public void Pause()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (resumeButton != null)
        {
            EventSystem.current?.SetSelectedGameObject(resumeButton.gameObject);
        }
    }

    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu"); // Đặt tên scene menu của bạn ở đây
    }
}
