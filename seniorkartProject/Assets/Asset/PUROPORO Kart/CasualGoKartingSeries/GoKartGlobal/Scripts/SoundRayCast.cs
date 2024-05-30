using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SoundRayCast : MonoBehaviour
{
    private bool changeCount = true;
    private AudioSource audioSource;

    public AudioClip road;
    public AudioClip grass;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            if (changeCount)
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    audioSource.Stop();
                    audioSource.clip = road;
                    audioSource.Play();

                    changeCount = false;
                }
            }
            else
            {
                if (hit.collider.CompareTag("Grass"))
                {
                    audioSource.Stop();
                    audioSource.clip = grass;
                    audioSource.Play();

                    changeCount = true;
                }
            }
        }

    }
}