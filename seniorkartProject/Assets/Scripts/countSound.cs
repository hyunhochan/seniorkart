using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countSound : MonoBehaviour
{
    private int Timer = 0;
    private bool soundPlayed = false; // 오디오 재생 여부를 나타내는 변수 추가

    void Start()
    {
        Timer = 0;
    }

    void Update()
    {
        if (Timer == 0 && !soundPlayed) // 게임이 멈추고 아직 소리를 재생하지 않은 경우
        {
            Time.timeScale = 0.0f;
            // 오디오를 재생
            GetComponent<AudioSource>().Play();
            soundPlayed = true; // 소리를 재생했음을 표시
        }

        if (Timer <= 300)
        {
            Timer++;
            if (Timer >= 300)
            {
                StartCoroutine(this.LoadingEnd());
                Time.timeScale = 1.0f; // 게임 시작
            }
        }
    }

    IEnumerator LoadingEnd()
    {
        yield return new WaitForSeconds(1.0f);
        // 게임 오브젝트를 비활성화
        gameObject.SetActive(false);
    }
}