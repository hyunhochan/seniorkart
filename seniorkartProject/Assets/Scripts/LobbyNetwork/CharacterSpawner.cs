using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private Vector3[] spawnPositions;
    [SerializeField] private GameObject kartPrefab;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI currentRecordText;
    public TextMeshProUGUI finishTimeText;
    public TextMeshProUGUI countdownText; // 추가: 10초 카운트다운 텍스트
    public GameObject RaceResultUI; // 추가: 결과 창 UI
    public Image tacoBack;
    public Image tackLineBack;

    private List<GameObject> karts = new List<GameObject>();
    private const float maxSpeed = 150f;
    private GameObject localKart;

    private float elapsedTime = 0f;
    private bool timerRunning = false;

    public static CharacterSpawner Instance { get; private set; }

    private Vector3 previousPosition;
    public Vector3 Velocity { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int i = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (i >= spawnPositions.Length) break;

                var spawnPos = spawnPositions[i++];

                GameObject kart = Instantiate(kartPrefab, spawnPos, Quaternion.identity);
                kart.transform.position = spawnPos;
                NetworkObject kartNetworkObject = kart.GetComponent<NetworkObject>();
                if (kartNetworkObject != null)
                {
                    kartNetworkObject.SpawnAsPlayerObject(client.ClientId);
                    karts.Add(kart);

                    if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                    {
                        localKart = kart;
                        InitializeLocalKartAudioListener(localKart);
                    }
                }

                InitializeRigidbody(kart);
            }

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
                    InitializeRigidbody(localKart);
                    InitializeLocalKartAudioListener(localKart);
                    break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void InitializeRigidbody(GameObject kart)
    {
        var rigidbody = kart.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.isKinematic = false;
            rigidbody.position = new Vector3(rigidbody.position.x, 1.0f, rigidbody.position.z);
        }
    }

    private void FixedUpdate()
    {
        if (localKart == null) return;

        Velocity = (localKart.transform.position - previousPosition) / Time.fixedDeltaTime;
        previousPosition = localKart.transform.position;

        // 속도의 크기를 구합니다.
        float speed = Velocity.magnitude * 3.6f;
        UpdateLocalSpeedUI(speed);

        if (IsServer && timerRunning)
        {
            elapsedTime += Time.fixedDeltaTime;
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
            float alpha = Mathf.Lerp(0f, 1f, speed / maxSpeed);
            Color color = tacoBack.color;
            color.a = alpha;
            tacoBack.color = color;
        }

        if (tackLineBack != null)
        {
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

    public void UpdateFinishTimeUI(float finishTime)
    {
        if (finishTimeText != null)
        {
            int minutes = Mathf.FloorToInt(finishTime / 60f);
            int seconds = Mathf.FloorToInt(finishTime % 60f);
            int milliseconds = Mathf.FloorToInt((finishTime * 100f) % 100f);
            finishTimeText.text = string.Format("Finish Time: {0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
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