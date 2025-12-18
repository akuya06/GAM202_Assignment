using UnityEngine;
using UnityEngine.UIElements;

public class MusicMenuUI : MonoBehaviour
{
    public AudioClip[] musicClips; // Kéo file nhạc vào đây
    public string[] musicTitles = {
        "Boom Kitty, TOKYO MACHINE, Warriyo",
        "DEAF KEV - Invincible Glitch Hop",
        "gabriawl - Recall House NCS",
        "Janji - Heroes Tonight (feat. Johnning)",
        "waera - harinezumi [NCS Release]"
    };
    // public SoundManager soundManager;
    private AudioMixerManager audioMixerManager;

    private Button[] songButtons = new Button[5];
    private Label nowPlayingLabel;
    private Button playButton;
    private int selectedIndex = -1;
    private int playingIndex = -1;

    void OnEnable()
    {
        audioMixerManager = FindObjectOfType<AudioMixerManager>();

        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        nowPlayingLabel = root.Q<Label>("NowPlayingLabel");
        playButton = root.Q<Button>("MusicPlayButton");
        var musicList = root.Q<VisualElement>("MusicList");

        // Get all song buttons
        for (int i = 0; i < 5; i++)
        {
            songButtons[i] = root.Q<Button>($"SongButton{i}");
            int idx = i;
            if (songButtons[i] != null)
            {
                songButtons[i].clicked += () => OnSongButtonClicked(idx);
            }
        }

        if (playButton != null)
        {
            playButton.clicked += OnPlayButtonClicked;
        }

        UpdateSelectionVisual();
        UpdateNowPlayingLabel();
    }


    void OnSongButtonClicked(int idx)
    {
        selectedIndex = idx;
        UpdateSelectionVisual();
    }

    void OnPlayButtonClicked()
    {
        if (audioMixerManager == null)
            audioMixerManager = FindObjectOfType<AudioMixerManager>();

        if (selectedIndex >= 0 && selectedIndex < musicClips.Length)
        {
            playingIndex = selectedIndex;
            if (audioMixerManager != null && audioMixerManager.musicSource != null)
            {
                audioMixerManager.musicSource.clip = musicClips[playingIndex];
                audioMixerManager.musicSource.Play();
            }
            UpdateNowPlayingLabel();
            StopAllCoroutines();
            StartCoroutine(MarqueeTitle(musicTitles[playingIndex]));
        }

        Debug.Log("audioMixerManager: " + audioMixerManager);
        Debug.Log("musicSource: " + (audioMixerManager != null ? audioMixerManager.musicSource : null));
        Debug.Log("musicClips: " + (musicClips != null ? musicClips.Length.ToString() : "null"));
    }

    void UpdateSelectionVisual()
    {
        for (int i = 0; i < songButtons.Length; i++)
        {
            if (songButtons[i] != null)
            {
                if (i == selectedIndex)
                    songButtons[i].AddToClassList("selected");
                else
                    songButtons[i].RemoveFromClassList("selected");
            }
        }
    }

    void UpdateNowPlayingLabel()
    {
        if (nowPlayingLabel != null)
        {
            if (playingIndex >= 0 && playingIndex < musicTitles.Length)
                nowPlayingLabel.text = $"Đang phát: {musicTitles[playingIndex]}";
            else
                nowPlayingLabel.text = "Đang phát: ";
        }
    }

    // Hiệu ứng chạy chữ tiêu đề bài hát
    System.Collections.IEnumerator MarqueeTitle(string title)
    {
        if (nowPlayingLabel == null) yield break;
        string baseText = $"Đang phát: {title}   ";
        while (playingIndex >= 0 && musicTitles[playingIndex] == title)
        {
            nowPlayingLabel.text = baseText;
            for (int i = 0; i < title.Length + 10; i++)
            {
                nowPlayingLabel.text = "Đang phát: " + title.Substring(i % title.Length) + " " + title.Substring(0, i % title.Length);
                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}