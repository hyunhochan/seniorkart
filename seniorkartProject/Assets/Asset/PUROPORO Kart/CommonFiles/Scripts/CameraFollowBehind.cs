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

        [SerializeField] private string m_TargetName = "Player";
        [SerializeField] private Transform m_Target;
        [SerializeField] private float m_FollowSpeed = 8f;
        [SerializeField] private float m_RotationSpeed = 4f;

        private void Start()
        {
            if (m_Target == null)
                m_Target = GameObject.FindGameObjectWithTag(m_TargetName).transform;

            m_PositionOffset = transform.localPosition;
        }

        private void FixedUpdate()
        {
            HandlePosition();
            HandleRotation();
        }

        private void HandlePosition()
        {
            var targetPosition = m_Target.TransformPoint(m_PositionOffset);
            transform.position = Vector3.Lerp(transform.position, targetPosition, m_FollowSpeed * Time.deltaTime);
        }
        private void HandleRotation()
        {
            var direction = m_Target.position - transform.position;
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, m_RotationSpeed * Time.deltaTime);
        }
    }
}