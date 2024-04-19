using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class UIButtonNextFromArray : MonoBehaviour
    {
        public GameObject[] objects;
        private int m_Current;

        private void Start()
        {
            DisableAll();
            m_Current = 0;
            objects[m_Current].SetActive(true);
        }

        public void OnClick()
        {
            DisableAll();

            m_Current++;

            if (m_Current >= objects.Length)
                m_Current = 0;

            objects[m_Current].SetActive(true);
        }

        private void DisableAll()
        {
            for (int i = 0; i < objects.Length - 1; i++)
                objects[i].SetActive(false);
        }
    }
}
