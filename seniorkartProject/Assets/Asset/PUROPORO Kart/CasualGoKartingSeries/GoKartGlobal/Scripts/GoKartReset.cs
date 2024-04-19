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

        void Start()
        {
            m_OriginalPosition = transform.position;
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
        }
    }
}
