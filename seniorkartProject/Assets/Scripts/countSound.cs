using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countSound : MonoBehaviour
{
    private int Timer = 0;
    private bool soundPlayed = false; // ����� ��� ���θ� ��Ÿ���� ���� �߰�

    void Start()
    {
        Timer = 0;
    }

    void Update()
    {
        if (Timer == 0 && !soundPlayed) // ������ ���߰� ���� �Ҹ��� ������� ���� ���
        {
            Time.timeScale = 0.0f;
            // ������� ���
            GetComponent<AudioSource>().Play();
            soundPlayed = true; // �Ҹ��� ��������� ǥ��
        }

        if (Timer <= 300)
        {
            Timer++;
            if (Timer >= 300)
            {
                StartCoroutine(this.LoadingEnd());
                Time.timeScale = 1.0f; // ���� ����
            }
        }
    }

    IEnumerator LoadingEnd()
    {
        yield return new WaitForSeconds(1.0f);
        // ���� ������Ʈ�� ��Ȱ��ȭ
        gameObject.SetActive(false);
    }
}