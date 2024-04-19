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

        [Header("Colliders")]
        public WheelCollider wheelColliderFL;
        public WheelCollider wheelColliderFR;
        public WheelCollider wheelColliderRL;
        public WheelCollider wheelColliderRR;

        private void FixedUpdate()
        {
            GetInput();
            HandleAcceleration();
            HandleSteering();
        }

        private void GetInput()
        {
            inputSteering = Input.GetAxis(c_Horizontal);
            inputAcceleration = Input.GetAxis(c_Vertical);
            isBraking = Input.GetKey(KeyCode.Space);
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
