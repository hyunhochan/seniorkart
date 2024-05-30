using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PUROPORO
{
    public class GoKartSharkController : NetworkBehaviour
    {
        private GoKartController m_Controller;

        [Header("Visuals")]
        [SerializeField] private Transform m_VisualWheelFL;
        [SerializeField] private Transform m_VisualWheelFR;
        [SerializeField] private Transform m_VisualWheelRL;
        [SerializeField] private Transform m_VisualWheelRR;
        [SerializeField] private Transform m_VisualSteeringWheel;

        private void Awake()
        {
            m_Controller = GetComponent<GoKartController>();
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
            //if (m_VisualWheelFL != null) { UpdateSingleWheel(m_Controller.wheelColliderFL, m_VisualWheelFL); }
            //if (m_VisualWheelFR != null) { UpdateSingleWheel(m_Controller.wheelColliderFR, m_VisualWheelFR); }
            //if (m_VisualWheelRL != null) { UpdateSingleWheel(m_Controller.wheelColliderRL, m_VisualWheelRL); }
            //if (m_VisualWheelRR != null) { UpdateSingleWheel(m_Controller.wheelColliderRR, m_VisualWheelRR); }

            //if (m_VisualSteeringWheel != null)
            //{
            //    m_VisualSteeringWheel.localEulerAngles = new Vector3(
            //        m_VisualSteeringWheel.localEulerAngles.x,
            //        m_VisualSteeringWheel.localEulerAngles.y,
            //        -m_Controller.currentSteeringAngle);
            //}
        }

        private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
        {
            Vector3 pos;
            Quaternion rot;
            wheelCollider.GetWorldPose(out pos, out rot);
            wheelTransform.rotation = rot;
            wheelTransform.position = pos;
        }
    }
}
