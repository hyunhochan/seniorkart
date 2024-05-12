using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class GoKartController : MonoBehaviour
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

        private bool autoDrive = false;

        private void Start()
        {
            StartCoroutine(StartAutoDriveAfterDelay());
        }

        private IEnumerator StartAutoDriveAfterDelay()
        {
            yield return new WaitForSeconds(autoDriveDelay);
            autoDrive = true;
        }

        private void FixedUpdate()
        {
            GetInput();
            if (autoDrive)
            {
                inputAcceleration = 1f; // 자동 전진을 위한 가속 설정
            }
            HandleButtonInput(); // 추가된 메서드 호출
            HandleAcceleration();
            HandleSteering();
            HandleBraking(); // 브레이킹 처리 메서드 분리
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
    }
}