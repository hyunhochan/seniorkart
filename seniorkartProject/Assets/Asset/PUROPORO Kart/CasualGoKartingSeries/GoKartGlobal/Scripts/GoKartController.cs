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
        public float accelerationForce = 11f;
        public float brakingForce = 16f;
        public float maxSteeringAngle = 24f;
        public float autoDriveDelay = 3f;
        public float rotationSpeed = 150f;
        public float brakeLerpSpeed = 1f; // Variable to control the lerp speed for braking
        public float accelLerpSpeed = 1f; // Variable to control the lerp speed for accelerating
        public float turnDecelerationFactor = 0.1f; // Factor to reduce acceleration when turning
        public float recoverySpeed = 1f; // Speed at which acceleration recovers after turning

        public float buttonSteering;
        public float buttonBrake;
        public float steeringLerpSpeed = 2f; // Variable to control the lerp speed for steering

        [Header("Camera")]
        public GameObject cameraMountPoint;
        private Camera playerCamera;

        private bool isbrake = false;
        private bool autoDrive = false;
        public bool isGrounded = false; // Flag to check if the kart is grounded
        private Rigidbody rb;
        private float currentBrakeForce = 0f; // Variable to track the current braking force
        private float currentAccelForce = 0f; // Variable to track the current acceleration force

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
                    // Only handle movement when the kart is grounded
                    if (autoDrive)
                    {
                        inputAcceleration = 1f; // Auto drive forward
                    }

                    HandleMovement(inputAcceleration, inputSteering, isBraking);

                    // Send input to the server
                    SendInputToServer();
                }
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                // Additional update logic for the owner
            }
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
            if (inputAcceleration > 0)
            {
                float adjustedAcceleration = inputAcceleration;
                if (inputSteering != 0)
                {
                    adjustedAcceleration *= (1f - Mathf.Abs(inputSteering) * turnDecelerationFactor); // Reduce acceleration when turning proportionally to steering
                }
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
                float turn = inputSteering * maxSteeringAngle * Time.fixedDeltaTime;
                Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
                rb.MoveRotation(Quaternion.Lerp(rb.rotation, rb.rotation * turnRotation, rotationSpeed * Time.fixedDeltaTime));
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
