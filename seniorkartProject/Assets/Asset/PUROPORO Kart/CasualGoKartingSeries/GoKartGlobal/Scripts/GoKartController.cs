using System.Collections;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace PUROPORO
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientNetworkTransform))]
    public class GoKartController : NetworkBehaviour
    {
        [HideInInspector] public float inputAcceleration;
        [HideInInspector] public float inputSteering;
        [HideInInspector] public float currentSteeringAngle;
        [HideInInspector] public float currentBrakingForce;
        [HideInInspector] public bool isBraking;

        public enum Drivetrain { FWD, RWD, AWD };
        public enum Braking { AllWheels, Handbrake };

        private const string c_Horizontal = "Horizontal";
        private const string c_Vertical = "Vertical";

        [Header("Settings")]
        public Drivetrain drivetrain;
        public Braking brakingSystem;
        public float accelerationForce = 1500f;
        public float brakingForce = 1000f;
        public float maxSteeringAngle = 30f;
        public float autoDriveDelay = 3f;

        [Header("Colliders")]
        public WheelCollider wheelColliderFL;
        public WheelCollider wheelColliderFR;
        public WheelCollider wheelColliderRL;
        public WheelCollider wheelColliderRR;

        public float buttonSteering;
        public float buttonBrake;

        [Header("Camera")]
        public GameObject cameraMountPoint;
        private Camera playerCamera;

        private bool autoDrive = false;
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
            }
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

                // 클라이언트 예측: 입력을 즉시 반영
                HandleAcceleration(inputAcceleration);
                HandleSteering(inputSteering);
                HandleBraking(isBraking);

                // 서버에 입력을 보내는 코드 추가
                SendInputToServer();
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                //if (Input.GetKeyDown(KeyCode.R))
                //{
                //    RespawnAtCheckpoint();
                //}
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

        private void HandleBraking(bool isBraking)
        {
            currentBrakingForce = isBraking ? brakingForce : 0f;
            switch (brakingSystem)
            {
                case Braking.AllWheels:
                    wheelColliderFL.brakeTorque = currentBrakingForce;
                    wheelColliderFR.brakeTorque = currentBrakingForce;
                    wheelColliderRL.brakeTorque = currentBrakingForce;
                    wheelColliderRR.brakeTorque = currentBrakingForce;
                    break;
                case Braking.Handbrake:
                    wheelColliderRL.brakeTorque = currentBrakingForce;
                    wheelColliderRR.brakeTorque = currentBrakingForce;
                    break;
                default:
                    break;
            }
        }

        private void HandleAcceleration(float inputAcceleration)
        {
            switch (drivetrain)
            {
                case Drivetrain.FWD:
                    wheelColliderFL.motorTorque = inputAcceleration * accelerationForce;
                    wheelColliderFR.motorTorque = inputAcceleration * accelerationForce;
                    break;
                case Drivetrain.RWD:
                    wheelColliderRL.motorTorque = inputAcceleration * accelerationForce;
                    wheelColliderRR.motorTorque = inputAcceleration * accelerationForce;
                    break;
                case Drivetrain.AWD:
                    wheelColliderFL.motorTorque = inputAcceleration * accelerationForce;
                    wheelColliderFR.motorTorque = inputAcceleration * accelerationForce;
                    wheelColliderRL.motorTorque = inputAcceleration * accelerationForce;
                    wheelColliderRR.motorTorque = inputAcceleration * accelerationForce;
                    break;
                default:
                    break;
            }
        }

        private void HandleSteering(float inputSteering)
        {
            currentSteeringAngle = maxSteeringAngle * inputSteering;
            wheelColliderFL.steerAngle = currentSteeringAngle;
            wheelColliderFR.steerAngle = currentSteeringAngle;
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
            HandleAcceleration(input.Acceleration);
            HandleSteering(input.Steering);
            HandleBraking(input.IsBraking);
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
        /*
        private void RespawnAtCheckpoint()
        {
            Transform checkpointTransform = RaceManager.Instance.GetPlayerCheckpointTransform(gameObject);
            if (checkpointTransform != null)
            {
                transform.position = checkpointTransform.position;
                transform.rotation = checkpointTransform.rotation;
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }*/

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