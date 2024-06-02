using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    
    public class GoKartReset : MonoBehaviour
    {
        private Vector3 m_OriginalPosition;
        private Rigidbody m_Rigidbody;
        private AudioSource m_AudioSource;
        private audioPitch m_AudioPitch;

        void Start()
        {
            m_OriginalPosition = transform.position;
            m_Rigidbody = GetComponent<Rigidbody>();
            m_AudioSource = GetComponent<AudioSource>();
            m_AudioPitch = GetComponent<audioPitch>();
        }

        private void OnEnable()
        {
            UIButton.OnClick += OnButtonAction;
        }

        private void OnDisable()
        {
            UIButton.OnClick -= OnButtonAction;
        }

        void LateUpdate()
        {
            if (transform.position.y < -20 || Input.GetKey(KeyCode.R))
                ResetGoKart();
        }

        public void OnButtonAction(UIButtonAction buttonAction)
        {
            if (buttonAction.actionName == "ResetGoKart")
                ResetGoKart();
        }

        public void ResetGoKart()
        {
            transform.position = m_OriginalPosition;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (m_Rigidbody != null)
            {
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
                m_Rigidbody.isKinematic = false;
            }

            if (m_AudioSource != null)
            {
                m_AudioSource.pitch = 1.0f;
            }

            if (m_AudioPitch != null)
            {
                m_AudioPitch.ResetGoKart();
            }
        }
    }
}
