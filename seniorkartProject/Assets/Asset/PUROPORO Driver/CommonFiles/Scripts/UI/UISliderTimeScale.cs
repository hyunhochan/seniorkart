using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PUROPORO
{
    public class UISliderTimeScale : MonoBehaviour, IEndDragHandler
    {
        private Slider m_slider;

        private void Start()
        {
            if (m_slider == null)
                m_slider = GetComponent<Slider>();
        }

        private void Update()
        {
            Time.timeScale = m_slider.value;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            float temp = m_slider.value;
            double currentTime = Math.Round(temp, 1);
            m_slider.value = (float)currentTime;
        }
    }
}
