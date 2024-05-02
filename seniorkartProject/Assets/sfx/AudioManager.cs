using UnityEngine;

public class ReadyAudioManager : MonoBehaviour
{
    public static ReadyAudioManager Instance { get; private set; }

    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayHoverSound()
    {
        if (hoverSound != null)
        {
            audioSource.clip = hoverSound;
            audioSource.Play();
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.clip = clickSound;
            audioSource.Play();
        }
    }
}
