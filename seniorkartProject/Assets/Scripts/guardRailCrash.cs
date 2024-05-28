using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class guardRailCrash : MonoBehaviour
{
    public AudioSource collisionSound; // 충돌 사운드를 저장할 AudioSource

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
       
        // 충돌 지점의 첫 번째 점을 가져옴
        ContactPoint contact = collision.contacts[0];

        // 충돌 지점의 위치에서 사운드를 재생
        collisionSound.transform.position = contact.point;
        print("충돌이 발생함");
        collisionSound.Play();
        
        
    }
}
