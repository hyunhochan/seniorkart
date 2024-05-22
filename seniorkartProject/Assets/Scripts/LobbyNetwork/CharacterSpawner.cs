using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private Vector3[] spawnPositions; // 스폰 위치 배열
    [SerializeField] private GameObject kartPrefab; // 카트 프리팹
    public TextMeshProUGUI speedText; // 속도 표시할 TMP 텍스트
    public TextMeshProUGUI currentRecordText; // 현재 기록을 표시할 TMP 텍스트
    public Image tacoBack; // 불투명도를 조절할 이미지
    public Image tackLineBack; // 불투명도를 조절할 이미지

    private List<GameObject> karts = new List<GameObject>(); // 생성된 카트를 참조할 리스트
    private const float maxSpeed = 150f; // 최대 속도
    private GameObject localKart; // 로컬 클라이언트의 카트를 참조할 변수

    private float elapsedTime = 0f; // 경과 시간
    private bool timerRunning = false; // 타이머 실행 상태

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int i = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (i >= spawnPositions.Length) break;

                var spawnPos = spawnPositions[i++];

                // 카트 프리팹을 스폰 위치에 생성
                GameObject kart = Instantiate(kartPrefab, spawnPos, Quaternion.identity);
                NetworkObject kartNetworkObject = kart.GetComponent<NetworkObject>();
                if (kartNetworkObject != null)
                {
                    kartNetworkObject.SpawnAsPlayerObject(client.ClientId);
                    karts.Add(kart); // 생성된 카트를 리스트에 추가

                    // 로컬 클라이언트의 카트를 참조
                    if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                    {
                        localKart = kart;
                        InitializeLocalKartAudioListener(localKart); // 로컬 카트의 AudioListener 설정
                    }
                }

                InitializeRigidbody(kart);
            }

            // 모든 카트바디가 소환된 후 타이머 시작
            StartCoroutine(StartTimer());
        }
        else
        {
            StartCoroutine(FindLocalKart());
        }
    }

    private IEnumerator FindLocalKart()
    {
        while (localKart == null)
        {
            foreach (var kart in FindObjectsOfType<NetworkObject>())
            {
                if (kart.IsOwner)
                {
                    localKart = kart.gameObject;
                    InitializeRigidbody(localKart); // 로컬 카트의 리지드바디 초기화
                    InitializeLocalKartAudioListener(localKart); // 로컬 카트의 AudioListener 설정
                    break;
                }
            }
            yield return new WaitForSeconds(0.5f); // 루프 시간 간격을 둠으로써 성능 최적화
        }
    }

    private void InitializeRigidbody(GameObject kart)
    {
        var rigidbody = kart.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.velocity = Vector3.zero;         // 속도 0으로 초기화
            rigidbody.angularVelocity = Vector3.zero;  // 각속도 0으로 초기화
            rigidbody.isKinematic = false;             // Rigidbody의 물리적 효과 활성화
            rigidbody.position = new Vector3(rigidbody.position.x, 1.0f, rigidbody.position.z); // 초기 위치 조정
        }
    }

    private void Update()
    {
        if (localKart == null) return; // 로컬 카트가 없으면 업데이트하지 않음

        var rigidbody = localKart.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            // Rigidbody의 속도를 km/h 단위로 변환
            float speed = rigidbody.velocity.magnitude * 3.6f;

            // 로컬 클라이언트의 UI를 업데이트
            UpdateLocalSpeedUI(speed);
        }

        // 서버에서 타이머 업데이트
        if (IsServer && timerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI(elapsedTime);
            SyncTimerClientRpc(elapsedTime);
        }
    }

    private void UpdateLocalSpeedUI(float speed)
    {
        if (speedText != null)
        {
            speedText.text = $"{speed:F1}";
        }

        if (tacoBack != null)
        {
            // 속도에 따라 불투명도를 조절
            float alpha = Mathf.Lerp(0f, 1f, speed / maxSpeed);
            Color color = tacoBack.color;
            color.a = alpha;
            tacoBack.color = color;
        }

        if (tackLineBack != null)
        {
            // 속도에 따라 불투명도를 조절
            float alpha = Mathf.Lerp(0f, 1f, speed / maxSpeed);
            Color color = tackLineBack.color;
            color.a = alpha;
            tackLineBack.color = color;
        }
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(1f);
        elapsedTime = 0f;
        timerRunning = true;
    }

    [ClientRpc]
    private void SyncTimerClientRpc(float time)
    {
        if (!IsServer)
        {
            elapsedTime = time;
            UpdateTimerUI(elapsedTime);
        }
    }

    private void UpdateTimerUI(float time)
    {
        if (currentRecordText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
            currentRecordText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
    }

    private void InitializeLocalKartAudioListener(GameObject kart)
    {
        
        Transform cameraTargetTransform = kart.transform.Find("Camera Target");
        if (cameraTargetTransform != null)
        {
            if (cameraTargetTransform.GetComponent<AudioListener>() == null)
            {
                cameraTargetTransform.gameObject.AddComponent<AudioListener>();
            }
        }
        else
        {
            Debug.LogError("'Camera Target' not found on local kart.");
        }
    }
}
