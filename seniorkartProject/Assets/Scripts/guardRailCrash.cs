using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class guardRailCrash : MonoBehaviour
{
    public AudioSource collisionSound; // �浹 ���带 ������ AudioSource

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
       
        // �浹 ������ ù ��° ���� ������
        ContactPoint contact = collision.contacts[0];

        // �浹 ������ ��ġ���� ���带 ���
        collisionSound.transform.position = contact.point;
        print("�浹�� �߻���");
        collisionSound.Play();
        
        
    }
}
