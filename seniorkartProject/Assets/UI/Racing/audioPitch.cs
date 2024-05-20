using UnityEngine;

public class audioPitch : MonoBehaviour
{
    private Rigidbody rigidbody;
    private AudioSource audioSource;
    public AudioClip motorSound;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = motorSound;

        audioSource.loop = true;

        
        audioSource.Play();
    }

    void Update()
    {
        
        //Debug.Log(rigidbody.velocity.magnitude);

        float pitch = Mathf.Lerp(0.5f, 2.0f, rigidbody.velocity.magnitude / 10.0f);
        audioSource.pitch = pitch;
    }
}