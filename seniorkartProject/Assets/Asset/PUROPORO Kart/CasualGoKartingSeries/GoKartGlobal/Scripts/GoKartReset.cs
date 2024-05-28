using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    /// <summary>
    /// When the R-key is pressed or the transform's position Y falls below -20, this simple code returns it to its original place.
    /// </summary>
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
            Invoke("ResetGoKart", 2.0f);
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
            // 여기에서 스케일을 설정합니다.
            new Vector3(2.0f, 0.7f, 2.5f);

            // 리지드바디 초기화
            if (m_Rigidbody != null)
            {
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Rigidbody>().useGravity = false;
            }

            // 오디오 피치 초기화
            if (m_AudioSource != null)
            {
                m_AudioSource.pitch = 1.0f;
            }

            // audioPitch 스크립트의 ResetGoKart 호출
            if (m_AudioPitch != null)
            {
                m_AudioPitch.ResetGoKart();
            }
        }
    }
}
