using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PUROPORO
{
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
        public float autoDriveDelay = 3f; // 자동 전진 지연 시간

        [Header("Colliders")]
        public WheelCollider wheelColliderFL;
        public WheelCollider wheelColliderFR;
        public WheelCollider wheelColliderRL;
        public WheelCollider wheelColliderRR;

        public float buttonSteering; // 버튼을 통한 스티어링 입력
        public float buttonBrake;    // 버튼을 통한 브레이크 입력

        [Header("Camera")]
        public GameObject cameraMountPoint; // 카메라가 장착될 지점
        private Camera playerCamera;

        private bool autoDrive = false;

        // 네트워크 변수를 추가합니다.
        private NetworkVariable<float> networkInputAcceleration = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> networkInputSteering = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> networkIsBraking = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);

        private void Start()
        {
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
                HandleButtonInput(); // 추가된 메서드 호출

                // 네트워크 변수 업데이트
                networkInputAcceleration.Value = inputAcceleration;
                networkInputSteering.Value = inputSteering;
                networkIsBraking.Value = isBraking;

                HandleAcceleration();
                HandleSteering();
                HandleBraking();
            }
            else
            {
                // 네트워크 변수를 사용하여 입력 처리
                inputAcceleration = networkInputAcceleration.Value;
                inputSteering = networkInputSteering.Value;
                isBraking = networkIsBraking.Value;

                HandleAcceleration();
                HandleSteering();
                HandleBraking();
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RespawnAtCheckpoint();
                }
            }
        }

        private void GetInput()
        {
            inputSteering = Input.GetAxis(c_Horizontal) + buttonSteering;
            inputAcceleration = Input.GetAxis(c_Vertical);
            isBraking = Input.GetKey(KeyCode.Space) || buttonBrake > 0; // 버튼 입력 추가
        }

        // 추가된 메서드: 버튼 입력 처리
        private void HandleButtonInput()
        {
            // inputSteering 값이 -1과 1 사이로 제한되도록 조정
            inputSteering = Mathf.Clamp(inputSteering, -1f, 1f);
        }

        // 브레이킹 처리를 위한 메서드
        private void HandleBraking()
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

        private void HandleAcceleration()
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

            currentBrakingForce = isBraking ? brakingForce : 0f;
            ApplyBraking();
        }

        private void ApplyBraking()
        {
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

        private void HandleSteering()
        {
            currentSteeringAngle = maxSteeringAngle * inputSteering;
            wheelColliderFL.steerAngle = currentSteeringAngle;
            wheelColliderFR.steerAngle = currentSteeringAngle;
        }

        private void SetupCamera()
        {
            // 기존 카메라를 비활성화
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.enabled = false;
            }

            // 새로운 카메라를 생성하고 카트에 장착
            playerCamera = new GameObject("PlayerCamera").AddComponent<Camera>();
            playerCamera.transform.SetParent(cameraMountPoint.transform);
            playerCamera.transform.localPosition = Vector3.zero;
            playerCamera.transform.localRotation = Quaternion.identity;

            // 카메라에 따라다니도록 설정
            var cameraFollow = playerCamera.gameObject.AddComponent<CameraFollowBehind>();
            cameraFollow.SetTarget(cameraMountPoint.transform);
        }

        private void RespawnAtCheckpoint()
        {
            Transform checkpointTransform = RaceManager.Instance.GetPlayerCheckpointTransform(gameObject);
            if (checkpointTransform != null)
            {
                transform.position = checkpointTransform.position;
                transform.rotation = checkpointTransform.rotation;
                // Rigidbody의 속도를 초기화하여 차가 멈추도록 합니다.
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}