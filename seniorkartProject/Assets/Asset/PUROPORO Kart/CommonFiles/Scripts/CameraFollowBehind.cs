using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    /// <summary>
    /// A simple implementation of a camera that smoothly follows the player behind. Perfect for driving games, for example.
    /// </summary>
    public class CameraFollowBehind : MonoBehaviour
    {
        private Vector3 m_PositionOffset;

        [SerializeField] private Transform m_Target;
        [SerializeField] private float m_FollowSpeed = 8f;
        [SerializeField] private float m_RotationSpeed = 4f;
        [SerializeField] private Vector3 m_Offset = new Vector3(0, 3, -7); // 카메라의 위치 오프셋 설정

        private void Start()
        {
            if (m_Target == null)
            {
                Debug.LogError("Target not set for CameraFollowBehind.");
                return;
            }

            m_PositionOffset = m_Offset; // m_PositionOffset 초기화
        }

        private void FixedUpdate()
        {
            HandlePosition();
            HandleRotation();
        }

        private void HandlePosition()
        {
            var targetPosition = m_Target.position + m_Target.TransformDirection(m_PositionOffset);
            transform.position = Vector3.Lerp(transform.position, targetPosition, m_FollowSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            var direction = m_Target.position - transform.position;
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, m_RotationSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform target)
        {
            m_Target = target;
        }
    }
}