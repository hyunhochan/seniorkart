using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PUROPORO
{
    public class GoKartSharkController : NetworkBehaviour
    {
        private GoKartController m_Controller;
        private audioPitch m_AudioPitch;

        [Header("Visuals")]
        [SerializeField] private Transform m_VisualWheelFL;
        [SerializeField] private Transform m_VisualWheelFR;
        [SerializeField] private Transform m_VisualWheelRL;
        [SerializeField] private Transform m_VisualWheelRR;
        [SerializeField] private Transform m_VisualSteeringWheel;

        private void Awake()
        {
            m_Controller = GetComponent<GoKartController>();
            m_AudioPitch = GetComponent<audioPitch>();
        }

        private void LateUpdate()
        {
            if (!IsOwner)
            {
                return;
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (m_VisualWheelFL != null) { UpdateSingleWheel(m_Controller.wheelColliderFL, m_VisualWheelFL, true); }
            if (m_VisualWheelFR != null) { UpdateSingleWheel(m_Controller.wheelColliderFR, m_VisualWheelFR, true); }
            if (m_VisualWheelRL != null) { UpdateSingleWheel(m_Controller.wheelColliderRL, m_VisualWheelRL, false); }
            if (m_VisualWheelRR != null) { UpdateSingleWheel(m_Controller.wheelColliderRR, m_VisualWheelRR, false); }

            if (m_VisualSteeringWheel != null)
            {
                m_VisualSteeringWheel.localEulerAngles = new Vector3(
                    m_VisualSteeringWheel.localEulerAngles.x,
                    m_VisualSteeringWheel.localEulerAngles.y,
                    -m_Controller.currentsteerAngle);
            }
        }

        private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform, bool isFrontWheel)
        {
            Vector3 pos;
            Quaternion rot;
            wheelCollider.GetWorldPose(out pos, out rot);

            wheelTransform.position = pos;

            float wheelRotationSpeed = m_AudioPitch.speed / (2 * Mathf.PI * wheelCollider.radius) * 360;
            float wheelRotationAngle = wheelRotationSpeed * Time.deltaTime;

            if (isFrontWheel)
            {
                wheelTransform.rotation = rot;

                wheelTransform.Rotate(Vector3.right, wheelRotationAngle, Space.Self);
            }
            else
            {
                wheelTransform.rotation = rot * Quaternion.Euler(wheelRotationAngle, 0, 0);
            }
        }
    }
}
