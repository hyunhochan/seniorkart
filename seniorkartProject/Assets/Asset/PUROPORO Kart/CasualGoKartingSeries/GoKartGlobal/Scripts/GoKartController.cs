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
        private float accelerationForce;
        private float brakingForce;
        private float maxSteeringAngle;
        private float autoDriveDelay;
        private float rotationSpeed;
        public float brakeLerpSpeed; // Variable to control the lerp speed for braking
        private float accelLerpSpeed; // Variable to control the lerp speed for accelerating
        private float turnDecelerationFactor; // Factor to reduce acceleration when turning
        private float recoverySpeed; // Speed at which acceleration recovers after turning

        public float buttonSteering;
        public float buttonBrake;
        public float steeringLerpSpeed = 2f; // Variable to control the lerp speed for steering
        public float rotationRecoverySpeed = 1f; // Speed at which z-rotation recovers
        private bool HostDelay = false;


        [Header("Camera")]
        public GameObject cameraMountPoint;
        private Camera playerCamera;

        private bool isreset = false;
        private bool isbrake = false;
        private bool autoDrive = false;
        public bool isGrounded = false; // Flag to check if the kart is grounded
        private Rigidbody rb;
        public float currentBrakeForce = 0f; // Variable to track the current braking force
        public float currentAccelForce = 0f; // Variable to track the current acceleration force
        private bool resetandbeforelanding; // to prevent acceling in the air right after resetting
        private int mapnumber;

        private NetworkTransform networkTransform;

        private bool ableBoost = true;

        [Header("Colliders")]
        public WheelCollider wheelColliderFL;
        public WheelCollider wheelColliderFR;
        public WheelCollider wheelColliderRL;
        public WheelCollider wheelColliderRR;

        public float currentsteerAngle;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            networkTransform = GetComponent<NetworkTransform>();
            //StartCoroutine(StartAutoDriveAfterDelay());
            currentAccelForce = 0;
            autoDrive = true;
            if (IsOwner)
            {
                SetupCamera();
                SetupButtonControls();
            }


            GameObject boostButton = GameObject.Find("Gamepad/boost");
            Image buttonImage = boostButton.GetComponent<Image>();
            buttonImage.fillAmount = 0;

            GameObject mapcheck = GameObject.Find("MapCheck");
            mapnumber = mapcheck.GetComponent<WhichMap>().MapNumber;
            switch (mapnumber)
            {
                case 0:
                    accelerationForce = 25f;
                    brakingForce = 40f;
                    maxSteeringAngle = 40f;
                    autoDriveDelay = 5f;
                    rotationSpeed = 400f;
                    brakeLerpSpeed = 0.75f;
                    accelLerpSpeed = 1f;
                    turnDecelerationFactor = 0.2f;
                    recoverySpeed = 1f;
                    break;
                case 1:
                    accelerationForce = 35f;
                    brakingForce = 60f;
                    maxSteeringAngle = 50f;
                    autoDriveDelay = 5f;
                    rotationSpeed = 600f;
                    brakeLerpSpeed = 0.75f;
                    accelLerpSpeed = 1f;
                    turnDecelerationFactor = 0.3f;
                    recoverySpeed = 1f;
                    break;
                default:
                    break;
            }
        }
        private void SetupButtonControls()
        {

            GameObject leftButton = GameObject.Find("Gamepad/Left");
            GameObject rightButton = GameObject.Find("Gamepad/Right");
            GameObject brakeButton = GameObject.Find("Gamepad/brake");
            GameObject resetButton = GameObject.Find("Gamepad/reset");
            GameObject boostButton = GameObject.Find("Gamepad/boost");

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

                EventTrigger resetTrigger = resetButton.AddComponent<EventTrigger>();
                EventTrigger.Entry resetPressEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                resetPressEntry.callback.AddListener((data) => { OnResetButtonPress(); });
                resetTrigger.triggers.Add(resetPressEntry);

                EventTrigger boostTrigger = boostButton.AddComponent<EventTrigger>();
                EventTrigger.Entry boostPressEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                boostPressEntry.callback.AddListener((data) => { OnboostButtonPress(); });
                boostTrigger.triggers.Add(boostPressEntry);
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
            // 앞바퀴를 정면으로 돌림
            currentsteerAngle = 0f;
            wheelColliderFL.steerAngle = 0f;
            wheelColliderFR.steerAngle = 0f;
        }

        private void OnBrakeButtonPress()
        {
            isbrake = true;
        }

        private void OnBrakeButtonRelease()
        {
            isbrake = false;
        }

        private void OnResetButtonPress()
        {
            if (IsOwner)
            {
                resetandbeforelanding = true;
                isGrounded = false;
                RespawnAtCheckpoint();

                currentsteerAngle = 0;
                currentAccelForce = 0;
                wheelColliderFL.steerAngle = 0;
                wheelColliderFR.steerAngle = 0;
            }
        }

        private void OnboostButtonPress()
        {
            if (ableBoost)
            {
                ableBoost = false;
                boost();
                StartCoroutine(EnableBoostAfterDelay(5f));
            }
        }

        private IEnumerator EnableBoostAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ableBoost = true;

        }

        private void boost()
        {
            // 현재 카트의 진행 방향을 기준으로 전방 30도 각도를 계산
            Vector3 forwardDirection = Quaternion.Euler(-25, 0, 0) * transform.forward;
            switch (mapnumber)
            {
                case 0:
                    // AddForce를 통해 카트를 앞으로 가볍게 날림
                    rb.AddForce(forwardDirection * 7000f, ForceMode.Impulse);
                    break;
                case 1:
                    // AddForce를 통해 카트를 앞으로 가볍게 날림
                    rb.AddForce(forwardDirection * 10000f, ForceMode.Impulse);
                    break;
                default:
                    break;
            }

            GameObject boostButton = GameObject.Find("Gamepad/boost");
            boostButton.GetComponent<Button>().interactable = false;
            if (boostButton != null)
            {
                Image buttonImage = boostButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    // fillAmount를 0으로 설정하고 코루틴 시작
                    buttonImage.fillAmount = 1;
                    StartCoroutine(FillButtonImage(buttonImage, 5f));

                }
            }
            else
            {
                Debug.LogWarning("Boost button not found!");
            }
        }

        private IEnumerator FillButtonImage(Image buttonImage, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                buttonImage.fillAmount = Mathf.Clamp01(1 - (elapsedTime / duration));
                yield return null;
            }
            GameObject boostButton = GameObject.Find("Gamepad/boost");
            boostButton.GetComponent<Button>().interactable = true;
        }

        private void RespawnAtCheckpoint()
        {
            Transform checkpointTransform = RaceManager.Instance.GetPlayerCheckpointTransform(gameObject);
            if (checkpointTransform != null)
            {
                transform.position = checkpointTransform.position;
                transform.rotation = checkpointTransform.rotation;
                isGrounded = false;
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                }
            }
        }
        private void FixedUpdate()
        {
            if (IsOwner)
            {
                GetInput();
                HandleButtonInput();

                if (isGrounded)
                {
                    // Only handle movement when the kart is grounded
                    if (autoDrive)
                    {
                        inputAcceleration = 1f; // Auto drive forward
                    }

                    HandleMovement(inputAcceleration, inputSteering, isBraking);

                    // Send input to the server
                    //SendInputToServer();
                }

                // Always recover Z rotation towards 0
                RecoverZRotation();
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                // Additional update logic for the owner
            }
        }


        private void RecoverZRotation()
        {
            Quaternion currentRotation = rb.rotation;
            Quaternion targetRotation = Quaternion.Euler(currentRotation.eulerAngles.x, currentRotation.eulerAngles.y, 0);
            rb.MoveRotation(Quaternion.Lerp(currentRotation, targetRotation, Time.fixedDeltaTime * rotationRecoverySpeed));
        }

        private void GetInput()
        {
            inputSteering = Mathf.Lerp(inputSteering, Input.GetAxis(c_Horizontal) + buttonSteering, Time.deltaTime * steeringLerpSpeed);
            inputAcceleration = Input.GetAxis(c_Vertical);
            isBraking = Input.GetKey(KeyCode.Space) || buttonBrake > 0;
        }

        private void HandleButtonInput()
        {
            inputSteering = Mathf.Clamp(inputSteering, -1f, 1f);
        }

        private void HandleMovement(float inputAcceleration, float inputSteering, bool isBraking)
        {
            Vector3 moveDirection = transform.forward * currentAccelForce * Time.fixedDeltaTime;

            // Forward/Backward Movement
            if (inputAcceleration > 0 && isGrounded)
            {
                float adjustedAcceleration = inputAcceleration;
                if (inputSteering != 0)
                {
                    adjustedAcceleration *= (1f - Mathf.Abs(inputSteering) * turnDecelerationFactor); // Reduce acceleration when turning proportionally to steering
                }
                if (resetandbeforelanding) { adjustedAcceleration = 0; }
                currentAccelForce = Mathf.Lerp(currentAccelForce, adjustedAcceleration * accelerationForce, Time.fixedDeltaTime * accelLerpSpeed);
                rb.MovePosition(rb.position + moveDirection);
            }
            else if (inputAcceleration < 0)
            {
                float adjustedAcceleration = inputAcceleration;

                if (inputSteering != 0)
                {
                    adjustedAcceleration *= (1f - Mathf.Abs(inputSteering) * turnDecelerationFactor); // Reduce acceleration when turning proportionally to steering
                }
                currentAccelForce = Mathf.Lerp(currentAccelForce, adjustedAcceleration * accelerationForce, Time.fixedDeltaTime * accelLerpSpeed);
                rb.MovePosition(rb.position + moveDirection);
            }
            else
            {
                currentAccelForce = Mathf.Lerp(currentAccelForce, 0, Time.fixedDeltaTime * accelLerpSpeed);
            }

            // Braking
            if (isbrake)
            {
                currentBrakeForce = Mathf.Lerp(currentBrakeForce, brakingForce, Time.fixedDeltaTime * brakeLerpSpeed);
                rb.MovePosition(rb.position - transform.forward * currentBrakeForce * Time.fixedDeltaTime);
            }
            else
            {
                currentBrakeForce = Mathf.Lerp(currentBrakeForce, 0, Time.fixedDeltaTime * brakeLerpSpeed);
                rb.MovePosition(rb.position - transform.forward * currentBrakeForce * Time.fixedDeltaTime);
            }

            // Steering
            if (inputSteering != 0)
            {
                currentsteerAngle = inputSteering * maxSteeringAngle;
                wheelColliderFL.steerAngle = currentsteerAngle;
                wheelColliderFR.steerAngle = currentsteerAngle;
                float turn = currentsteerAngle * Time.fixedDeltaTime;
                Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
                rb.MoveRotation(Quaternion.Lerp(rb.rotation, rb.rotation * turnRotation, rotationSpeed * Time.fixedDeltaTime));
            }
            else
            {
                currentsteerAngle = 0;
                wheelColliderFL.steerAngle = currentsteerAngle;
                wheelColliderFR.steerAngle = currentsteerAngle;
            }

            // Recovery of acceleration after steering
            if (inputSteering == 0 && currentAccelForce < accelerationForce)
            {
                currentAccelForce = Mathf.Lerp(currentAccelForce, accelerationForce, Time.fixedDeltaTime * recoverySpeed);
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
            // Process input on the server and correct client state
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

        public void ResetKart() // This method will be called from the GoKartReset script
        {
            currentAccelForce = 0f;
            currentBrakeForce = 0f;
            inputAcceleration = 0f;
            inputSteering = 0f;
            isBraking = false;
        }

        private void ServerReconciliation()
        {
            // Server reconciliation logic to correct differences between client and server states
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
            // 태그가 "Ground" 또는 "Grass"인 오브젝트와 닿았을 때
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Grass"))
            {
                isGrounded = true;
                Debug.Log("Grounded");
                resetandbeforelanding = false;
            }
            // 태그가 "Wall"인 오브젝트와 닿았을 때
            else if (collision.gameObject.CompareTag("Wall"))
            {
                // 충돌 처리:

                currentAccelForce = currentAccelForce * 0.5f; // 가속도를 0으로
            }
            else if (collision.gameObject.CompareTag("Fence") || collision.gameObject.CompareTag("Player"))
            {
                // 충돌 처리:

                currentAccelForce = currentAccelForce * 0.7f; // 가속도를 0으로
            }

        }

        private void OnCollisionStay(Collision collision)
        {
            // 태그가 "Ground" 또는 "Gress"인 오브젝트와 닿았을 때
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Grass"))
            {
                isGrounded = true;
                resetandbeforelanding = false;
            }
            else if (collision.gameObject.CompareTag("Wall"))
            {
                currentAccelForce = currentAccelForce * 0.8f; // 가속도를 0으로

            }
            else if (collision.gameObject.CompareTag("Fence") || collision.gameObject.CompareTag("Player"))
            {
                // 충돌 처리:

                currentAccelForce = currentAccelForce * 0.95f; // 가속도를 점진적으로 죽임
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