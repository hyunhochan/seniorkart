using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    /// <summary>
    /// A simple implementation of a top-down camera that smoothly follows the player.
    /// </summary>
    public class CameraFollowTopDown : MonoBehaviour
    {
        private Vector3 m_PositionOffset;
        private Vector3 m_RotationOffset;

        [SerializeField] private string m_TargetName = "Player";
        [SerializeField] private Transform m_Target;
        [SerializeField] private float m_FollowSpeed = 8f;
        [SerializeField] private float m_RotationSpeed = 4f;
        [SerializeField] private bool m_FollowRotation;

        private void Start()
        {
            if (m_Target == null)
                m_Target = GameObject.FindGameObjectWithTag(m_TargetName).transform;

            m_PositionOffset = transform.localPosition;
            m_RotationOffset = transform.localEulerAngles;
        }

        private void FixedUpdate()
        {
            HandlePosition();

            if (m_FollowRotation)
                HandleRotation();
        }

        private void HandlePosition()
        {
            transform.position = Vector3.Lerp(transform.position, m_Target.position + m_PositionOffset, Time.deltaTime * m_FollowSpeed);
        }
        private void HandleRotation()
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(m_Target.eulerAngles + m_RotationOffset), Time.deltaTime * m_RotationSpeed);
        }
    }
}
