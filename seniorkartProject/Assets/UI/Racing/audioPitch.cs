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
        // 속도를 0에서 100으로 정규화
        float normalizedSpeed = Mathf.Clamp(rb.velocity.magnitude / 100.0f, 0f, 1f);

        // 피치를 계산하기 위해 1 - (1 - normalizedSpeed)^2 함수 사용
        float adjustedSpeed = 1 - Mathf.Pow(1 - normalizedSpeed, 2);

        // 최대 피치를 2.5로 설정
        float pitch = Mathf.Lerp(0.5f, 3.0f, adjustedSpeed);

        audioSource.pitch = pitch;
    }

    // 이 메서드는 GoKartReset 스크립트에서 호출됩니다.
    public void ResetGoKart()
    {
        audioSource.pitch = 1.0f;
    }
}
