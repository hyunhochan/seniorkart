using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSound : MonoBehaviour
{
    private GameObject soundSpawn;
    private AudioSource bottomAudio;

    public AudioClip roadSound;
    public AudioClip grassSound;

    private void Start()
    {
        soundSpawn = GameObject.Find("BottomSound");
        bottomAudio = soundSpawn.GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            bottomAudio.Stop();
            bottomAudio.clip = roadSound;
            bottomAudio.Play();
        }
        else if (collision.gameObject.CompareTag("Grass"))
        {
            bottomAudio.Stop();
            bottomAudio.clip = grassSound;
            bottomAudio.Play();
        }
    }
}