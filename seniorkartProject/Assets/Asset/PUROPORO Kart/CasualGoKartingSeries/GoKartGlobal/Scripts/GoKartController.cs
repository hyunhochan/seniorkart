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
        public float brakeLerpSpeed; 
        private float accelLerpSpeed;
        private float turnDecelerationFactor; 
        private float recoverySpeed;

        public float buttonSteering;
        public float buttonBrake;
        public float steeringLerpSpeed = 2f;
        public float rotationRecoverySpeed = 1f;
        private bool HostDelay = false;


        [Header("Camera")]
        public GameObject cameraMountPoint;
        private Camera playerCamera;

        private bool isreset = false;
        private bool isbrake = false;
        private bool autoDrive = false;
        public bool isGrounded = false;
        private Rigidbody rb;
        public float currentBrakeForce = 0f;
        public float currentAccelForce = 0f;
        private bool resetandbeforelanding;
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
            Vector3 forwardDirection = Quaternion.Euler(-7, 0, 0) * transform.forward;
            switch (mapnumber)
            {
                case 0:
                    rb.AddForce(forwardDirection * 10000f, ForceMode.Impulse);
                    break;
                case 1:
                    rb.AddForce(forwardDirection * 12000f, ForceMode.Impulse);
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
                    if (autoDrive)
                    {
                        inputAcceleration = 1f; 
                    }

                    HandleMovement(inputAcceleration, inputSteering, isBraking);

                }

                RecoverZRotation();
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
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

            if (inputAcceleration > 0 && isGrounded)
            {
                float adjustedAcceleration = inputAcceleration;
                if (inputSteering != 0)
                {
                    adjustedAcceleration *= (1f - Mathf.Abs(inputSteering) * turnDecelerationFactor); 
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
                    adjustedAcceleration *= (1f - Mathf.Abs(inputSteering) * turnDecelerationFactor); 
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

        public void ResetKart()
        {
            currentAccelForce = 0f;
            currentBrakeForce = 0f;
            inputAcceleration = 0f;
            inputSteering = 0f;
            isBraking = false;
        }

        private void ServerReconciliation()
        {
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
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Grass"))
            {
                isGrounded = true;
                Debug.Log("Grounded");
                resetandbeforelanding = false;
            }
            else if (collision.gameObject.CompareTag("Wall"))
            {

                currentAccelForce = currentAccelForce * 0.5f;
            }
            else if (collision.gameObject.CompareTag("Fence") || collision.gameObject.CompareTag("Player"))
            {
                currentAccelForce = currentAccelForce * 0.7f;
            }

        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Grass"))
            {
                isGrounded = true;
                resetandbeforelanding = false;
            }
            else if (collision.gameObject.CompareTag("Wall"))
            {
                currentAccelForce = currentAccelForce * 0.8f;

            }
            else if (collision.gameObject.CompareTag("Fence") || collision.gameObject.CompareTag("Player"))
            {
                currentAccelForce = currentAccelForce * 0.95f;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
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