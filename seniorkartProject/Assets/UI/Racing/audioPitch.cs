using UnityEngine;

public class audioPitch : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;
    public AudioClip motorSound;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = motorSound;

        audioSource.loop = true;

        audioSource.Play();
    }

    void Update()
    {
        
        float normalizedSpeed = Mathf.Clamp(rb.velocity.magnitude / 100.0f, 0f, 1f);

        
        float adjustedSpeed = 1 - Mathf.Pow(1 - normalizedSpeed, 2);

        
        float pitch = Mathf.Lerp(0.5f, 3.0f, adjustedSpeed);

        audioSource.pitch = pitch;
    }

    
    public void ResetGoKart()
    {
        audioSource.pitch = 1.0f;
    }
}
