using System.Collections;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PUROPORO
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientNetworkTransform))]
    public class GoKartController : NetworkBehaviour
    {
        [HideInInspector] public float inputAcceleration;
        [HideInInspector] public float inputSteering;
        [HideInInspector] public bool isBraking;

        private const string c_Horizontal = "Horizontal";
        private const string c_Vertical = "Vertical";

        [Header("Settings")]
        public float accelerationForce = 10f;
        public float brakingForce = 8f;
        public float maxSteeringAngle = 25f;
        public float autoDriveDelay = 3f;
        public float rotationSpeed = 150f;

        public float buttonSteering;
        public float buttonBrake;

        [Header("Camera")]
        public GameObject cameraMountPoint;
        private Camera playerCamera;

        private bool isbrake = false;
        private bool autoDrive = false;
        public bool isGrounded = false; // 땅에 닿아있는지 여부를 저장
        private Rigidbody rb;

        private NetworkTransform networkTransform;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            networkTransform = GetComponent<NetworkTransform>();
            StartCoroutine(StartAutoDriveAfterDelay());
            if (IsOwner)
            {
                SetupCamera();
                SetupButtonControls();
            }
        }

        private void SetupButtonControls()
        {
            GameObject leftButton = GameObject.Find("Gamepad/Left");
            GameObject rightButton = GameObject.Find("Gamepad/Right");
            GameObject brakeButton = GameObject.Find("Gamepad/brake");

            if (leftButton != null && rightButton != null && brakeButton != null)
            {
                EventTrigger leftTrigger = leftButton.AddComponent<EventTrigger>();
                EventTrigger.Entry leftPressEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                leftPressEntry.callback.AddListener((data) => { OnLeftButtonPress(); });
                leftTrigger.triggers.Add(leftPressEntry);

                EventTrigger.Entry leftReleaseEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                leftReleaseEntry.callback.AddListener((data) => { OnButtonRelease(); });
                leftTrigger.triggers.Add(leftReleaseEntry);

                EventTrigger rightTrigger = rightButton.AddComponent<EventTrigger>();
                EventTrigger.Entry rightPressEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                rightPressEntry.callback.AddListener((data) => { OnRightButtonPress(); });
                rightTrigger.triggers.Add(rightPressEntry);

                EventTrigger.Entry rightReleaseEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                rightReleaseEntry.callback.AddListener((data) => { OnButtonRelease(); });
                rightTrigger.triggers.Add(rightReleaseEntry);

                EventTrigger brakeTrigger = brakeButton.AddComponent<EventTrigger>();
                EventTrigger.Entry brakePressEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                brakePressEntry.callback.AddListener((data) => { OnBrakeButtonPress(); });
                brakeTrigger.triggers.Add(brakePressEntry);

                EventTrigger.Entry brakeReleaseEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                brakeReleaseEntry.callback.AddListener((data) => { OnBrakeButtonRelease(); });
                brakeTrigger.triggers.Add(brakeReleaseEntry);
            }
        }

        private void OnLeftButtonPress()
        {
            buttonSteering = -1f;
        }

        private void OnRightButtonPress()
        {
            buttonSteering = 1f;
        }

        private void OnButtonRelease()
        {
            buttonSteering = 0f;
        }

        private void OnBrakeButtonPress()
        {
            isbrake = true;
        }

        private void OnBrakeButtonRelease()
        {
            isbrake = false;
        }

        private IEnumerator StartAutoDriveAfterDelay()
        {
            yield return new WaitForSeconds(autoDriveDelay);
            autoDrive = true;
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                GetInput();
                HandleButtonInput();

                if (isGrounded)
                {
                    // 땅에 닿아있을 때만 움직임 처리
                    if (autoDrive)
                    {
                        inputAcceleration = 1f; // 자동 전진
                    }

                    HandleMovement(inputAcceleration, inputSteering, isBraking);

                    // 서버에 입력을 보내는 코드 추가
                    SendInputToServer();
                }
            }
        }


        private void Update()
        {
            if (IsOwner)
            {
                // if (Input.GetKeyDown(KeyCode.R))
                // {
                //     RespawnAtCheckpoint();
                // }
            }
        }

        private void GetInput()
        {
            inputSteering = Input.GetAxis(c_Horizontal) + buttonSteering;
            inputAcceleration = Input.GetAxis(c_Vertical);
            isBraking = Input.GetKey(KeyCode.Space) || buttonBrake > 0;
        }

        private void HandleButtonInput()
        {
            inputSteering = Mathf.Clamp(inputSteering, -1f, 1f);
        }

        private void HandleMovement(float inputAcceleration, float inputSteering, bool isBraking)
        {
            Vector3 moveDirection = transform.forward * inputAcceleration * accelerationForce * Time.fixedDeltaTime;

            // Forward/Backward Movement
            if (inputAcceleration != 0)
            {
                rb.MovePosition(rb.position + moveDirection);
            }

            // Braking
            if (isbrake)
            {
                rb.MovePosition(rb.position - moveDirection / 2);
            }

            // Steering
            if (inputSteering != 0)
            {
                float turn = inputSteering * maxSteeringAngle * Time.fixedDeltaTime;
                Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
                rb.MoveRotation(Quaternion.Lerp(rb.rotation, rb.rotation * turnRotation, rotationSpeed * Time.fixedDeltaTime));
            }
        }

        private void SendInputToServer()
        {
            var input = new KartInput
            {
                Acceleration = inputAcceleration,
                Steering = inputSteering,
                IsBraking = isBraking
            };

            SubmitInputServerRpc(input);
        }

        [ServerRpc]
        private void SubmitInputServerRpc(KartInput input)
        {
            // 서버에서 입력을 처리하고 클라이언트의 상태를 보정
            HandleMovement(input.Acceleration, input.Steering, input.IsBraking);
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.enabled = false;
            }

            playerCamera = new GameObject("PlayerCamera").AddComponent<Camera>();
            playerCamera.transform.SetParent(cameraMountPoint.transform);
            playerCamera.transform.localPosition = Vector3.zero;
            playerCamera.transform.localRotation = Quaternion.identity;

            var cameraFollow = playerCamera.gameObject.AddComponent<CameraFollowBehind>();
            cameraFollow.SetTarget(cameraMountPoint.transform);
        }

        private void ServerReconciliation()
        {
            // 서버 보정 로직: 클라이언트와 서버 간의 위치 및 상태 차이를 보정
            if (IsServer)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    var clientKart = client.PlayerObject.GetComponent<GoKartController>();
                    if (clientKart != null)
                    {
                        var serverKartPosition = clientKart.transform.position;
                        var serverKartRotation = clientKart.transform.rotation;

                        if (Vector3.Distance(clientKart.transform.position, serverKartPosition) > 0.1f ||
                            Quaternion.Angle(clientKart.transform.rotation, serverKartRotation) > 5f)
                        {
                            clientKart.transform.position = serverKartPosition;
                            clientKart.transform.rotation = serverKartRotation;
                        }
                    }
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // 태그가 "Ground"인 오브젝트와 닿았을 때
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
                Debug.Log("Grounded");
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            // 태그가 "Ground"인 오브젝트와 닿아있을 때
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            // 태그가 "Ground"인 오브젝트에서 떨어졌을 때
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = false;
            }
        }
    }

    public struct KartInput : INetworkSerializable
    {
        public float Acceleration;
        public float Steering;
        public bool IsBraking;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Acceleration);
            serializer.SerializeValue(ref Steering);
            serializer.SerializeValue(ref IsBraking);
        }
    }
}
