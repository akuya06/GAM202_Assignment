using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource musicSource;

    public void ChangeMusic(AudioClip newClip)
    {
        if (musicSource != null && newClip != null)
        {
            musicSource.clip = newClip;
            musicSource.Play();
        }
    }
}