using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerManager : MonoBehaviour
{
    public static AudioMixerManager Instance { get; private set; }
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource buttonClickSource;
    public AudioSource engineSource;
    public AudioSource driftSource;
    public AudioSource crashSource;
    public AudioSource countdownSource;
    public AudioSource winSource;
    public AudioSource loseSource;

    // PlayerPrefs keys
    const string MUSIC_VOL_KEY = "MusicVolume";
    const string SFX_VOL_KEY = "SFXVolume";
    
    private float musicVolume = 60f;
    private float sfxVolume = 80f;

    void Awake()
    {
        // Singleton pattern - giữ object này qua tất cả scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void SetupAudioSources()
    {
        // Tự động tạo Music Source nếu chưa có
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
        
        // Tạo các SFX AudioSources
        if (buttonClickSource == null)
        {
            buttonClickSource = gameObject.AddComponent<AudioSource>();
            buttonClickSource.playOnAwake = false;
        }
        
        if (engineSource == null)
        {
            engineSource = gameObject.AddComponent<AudioSource>();
            engineSource.playOnAwake = false;
            engineSource.loop = true;
        }
        
        if (driftSource == null)
        {
            driftSource = gameObject.AddComponent<AudioSource>();
            driftSource.playOnAwake = false;
            driftSource.loop = true;
        }
        
        if (crashSource == null)
        {
            crashSource = gameObject.AddComponent<AudioSource>();
            crashSource.playOnAwake = false;
        }
        
        if (countdownSource == null)
        {
            countdownSource = gameObject.AddComponent<AudioSource>();
            countdownSource.playOnAwake = false;
        }
        
        if (winSource == null)
        {
            winSource = gameObject.AddComponent<AudioSource>();
            winSource.playOnAwake = false;
        }
        
        if (loseSource == null)
        {
            loseSource = gameObject.AddComponent<AudioSource>();
            loseSource.playOnAwake = false;
        }

        // Gán AudioMixer Groups
        if (audioMixer != null)
        {
            var musicGroup = audioMixer.FindMatchingGroups("Music");
            if (musicGroup.Length > 0)
                musicSource.outputAudioMixerGroup = musicGroup[0];
            
            var sfxGroup = audioMixer.FindMatchingGroups("SFX");
            if (sfxGroup.Length > 0)
            {
                buttonClickSource.outputAudioMixerGroup = sfxGroup[0];
                engineSource.outputAudioMixerGroup = sfxGroup[0];
                driftSource.outputAudioMixerGroup = sfxGroup[0];
                crashSource.outputAudioMixerGroup = sfxGroup[0];
                countdownSource.outputAudioMixerGroup = sfxGroup[0];
                winSource.outputAudioMixerGroup = sfxGroup[0];
                loseSource.outputAudioMixerGroup = sfxGroup[0];
            }
        }
    }

    void Start()
    {
        LoadVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        float db = VolumeToDecibel(volume);
        
        audioMixer.SetFloat("MusicVol", db);
        
        if (musicSource != null)
            musicSource.volume = volume / 100f;
        
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        float db = VolumeToDecibel(volume);
        
        audioMixer.SetFloat("SfxVol", db);
        
        // Update volume cho tất cả SFX sources
        float normalizedVolume = volume / 100f;
        if (buttonClickSource != null) buttonClickSource.volume = normalizedVolume;
        if (engineSource != null) engineSource.volume = normalizedVolume;
        if (driftSource != null) driftSource.volume = normalizedVolume;
        if (crashSource != null) crashSource.volume = normalizedVolume;
        if (countdownSource != null) countdownSource.volume = normalizedVolume;
        if (winSource != null) winSource.volume = normalizedVolume;
        if (loseSource != null) loseSource.volume = normalizedVolume;
        
        PlayerPrefs.SetFloat(SFX_VOL_KEY, volume);
        PlayerPrefs.Save();
    }

    void LoadVolumes()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 60f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOL_KEY, 80f);

        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }

    private float VolumeToDecibel(float volume)
    {
        if (volume <= 0f)
            return -80f;
        
        return Mathf.Log10(volume / 100f) * 20f;
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    // ===== Audio Playback Methods =====
    
    public void PlayButtonClick()
    {
        if (buttonClickSource != null)
            buttonClickSource.Play();
    }
    
    public void PlayEngine()
    {
        if (engineSource != null && !engineSource.isPlaying)
            engineSource.Play();
    }
    
    public void StopEngine()
    {
        if (engineSource != null)
            engineSource.Stop();
    }
    
    public void PlayDrift()
    {
        if (driftSource != null && !driftSource.isPlaying)
            driftSource.Play();
    }
    
    public void StopDrift()
    {
        if (driftSource != null)
            driftSource.Stop();
    }
    
    public void PlayCrash()
    {
        if (crashSource != null)
            crashSource.Play();
    }
    
    public void PlayCountdown()
    {
        if (countdownSource != null)
            countdownSource.Play();
    }
    
    public void PlayWin()
    {
        if (winSource != null)
            winSource.Play();
    }
    
    public void PlayLose()
    {
        if (loseSource != null)
            loseSource.Play();
    }

    public void PlayMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource != null)
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
            musicSource.UnPause();
    }
}