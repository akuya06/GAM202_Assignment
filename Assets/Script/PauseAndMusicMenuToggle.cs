using UnityEngine;
using UnityEngine.UIElements;

public class PauseAndMusicMenuToggle : MonoBehaviour
{
    public GameObject pauseMenuObject;           // Kéo GameObject chứa PauseMenu vào đây
    public UIDocument musicMenuUIDocument;       // Kéo UIDocument của MusicMenu vào đây

    private bool isVisible = false;

    void Start()
    {
        if (pauseMenuObject != null)
            pauseMenuObject.SetActive(false);

        if (musicMenuUIDocument != null)
            musicMenuUIDocument.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isVisible = !isVisible;

            if (pauseMenuObject != null)
                pauseMenuObject.SetActive(isVisible);

            if (musicMenuUIDocument != null)
                musicMenuUIDocument.enabled = isVisible;

            // Dừng game khi menu hiện, resume khi tắt
            Time.timeScale = isVisible ? 0f : 1f;
            UnityEngine.Cursor.visible = isVisible;
            UnityEngine.Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}