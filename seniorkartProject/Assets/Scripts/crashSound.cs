using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crashSound : MonoBehaviour
{

    public AudioSource collisionSound;

    private void start()
    {
        collisionSound = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        collisionSound.Play();
        print("충돌이 발생함");
    }
}
