using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class UIButton : MonoBehaviour
    {
        public delegate void ActionClick(UIButtonAction ButtonAction);
        public static event ActionClick OnClick;

        [SerializeField] private UIButtonAction m_UIButtonAction;

        private void Start()
        {
            if (m_UIButtonAction == null)
                m_UIButtonAction = GetComponent<UIButtonAction>();
        }

        public virtual void OnButtonClick()
        {
            OnClick?.Invoke(m_UIButtonAction);
        }
    }
}
