using UnityEngine;
using UnityEngine.EventSystems;  // EventTrigger 사용을 위해 필요

public class MouseHoverSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverSound;
    private AudioSource audioSource;

    public AudioClip clickSound;
    private AudioSource audioSource2;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.2f;
        audioSource.clip = hoverSound;
        audioSource.playOnAwake = false;

        audioSource2 = gameObject.AddComponent<AudioSource>();
        audioSource2.volume = 0.2f;
        audioSource2.clip = clickSound;
        audioSource2.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        audioSource.Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        audioSource2.Play();
    }
}
