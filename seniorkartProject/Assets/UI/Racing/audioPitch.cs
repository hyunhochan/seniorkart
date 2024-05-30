using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class audioPitch : NetworkBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;
    public AudioClip motorSound;
    private CharacterSpawner characterSpawner;
    public float speed = 0;
    public bool isOwner = false;
    public Vector3 previousPosition;
    public Vector3 Velocity { get; private set; }

    private SpeedManager speedManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = motorSound;
        audioSource.loop = true;
        audioSource.Play();
        characterSpawner = CharacterSpawner.Instance;

        if (characterSpawner == null)
        {
            Debug.LogError("CharacterSpawner instance not found!");
        }
        previousPosition = rb.transform.position; // ??? ??
        if (IsOwner)
        {
            isOwner = true;
        }

        GameObject checkmyspeed = GameObject.Find("checkmyspeed");
        if (checkmyspeed != null)
        {
            speedManager = checkmyspeed.GetComponent<SpeedManager>();
        }
        else
        {
            Debug.LogError("checkmyspeed object not found!");
        }
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            if (characterSpawner != null)
            {
                Velocity = (rb.transform.position - previousPosition) / Time.fixedDeltaTime;
                previousPosition = rb.transform.position;

                float pitch = Mathf.Clamp(speed / 50.0f, 0.5f, 3.0f);
                audioSource.pitch = pitch;
            }

            // ??? ??? ????.
            speed = Velocity.magnitude * 3.6f;

            if (speedManager != null)
            {
                speedManager.speed = speed;
            }
            else
            {
                Debug.LogError("SpeedManager component not found!");
            }
        }
    }

    public void ResetGoKart()
    {
        audioSource.pitch = 1.0f;
    }
}
