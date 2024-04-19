using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class DEMODriverAnimations : MonoBehaviour
    {
        [SerializeField] private Animator m_DriverAnimator;
        [SerializeField] private Animator m_CabExampleAnimator;
        private float m_Acceleration;
        private float m_Speed;
        private float m_Turn;
        private float m_Turning;
        private float m_Multiplier = 2;

        private static bool m_IsDriving;
        public static bool isDriving
        {
            get { return m_IsDriving; }
            set { m_IsDriving = value; }
        }

        private void OnEnable()
        {
            UIButton.OnClick += PlayAnimation;
        }

        private void OnDisable()
        {
            UIButton.OnClick -= PlayAnimation;
        }

        private void Update()
        {
            if (m_IsDriving)
            {
                m_Turn = Input.GetAxis("Horizontal");
                m_Turning = Mathf.Lerp(m_Turning, m_Turn, Time.deltaTime * m_Multiplier);
                m_DriverAnimator.SetFloat("Turning", m_Turning);
                m_CabExampleAnimator.SetFloat("Turning", m_Turning);

                if (Input.GetKey(KeyCode.Space))
                {
                    m_Speed = Mathf.Lerp(m_Speed, 0, Time.deltaTime * m_Multiplier);
                }
                else
                {
                    m_Acceleration = Input.GetAxis("Vertical");
                    m_Speed = Mathf.Lerp(m_Speed, m_Acceleration, Time.deltaTime * m_Multiplier);
                }
                m_DriverAnimator.SetFloat("Acceleration", m_Speed);
            }
        }

        public void PlayAnimation(UIButtonAction buttonAction)
        {
            if (buttonAction.actionName == "Switch")
            {
                m_IsDriving = !m_IsDriving;

                if (m_IsDriving)
                    m_DriverAnimator.Play("JumpIn");
                else
                    m_DriverAnimator.Play("JumpOut");

                GetComponent<AnimatePathSimple>().StartAnimation(.4f, !m_IsDriving);

                return;
            }

            if (buttonAction.actionName != "Animations")
                return;

            UIButtomActionDriverAnimation BAction = (UIButtomActionDriverAnimation)buttonAction;
            m_DriverAnimator.CrossFade(BAction.animationName, .1f, BAction.layer);
            StartCoroutine(FadeLayerWeight(BAction.layer, BAction.duration));
        }

        IEnumerator FadeLayerWeight(int layer, float duration)
        {
            float weight = m_DriverAnimator.GetLayerWeight(layer);

            while (weight <= 1)
            {
                weight += 0.1f;
                m_DriverAnimator.SetLayerWeight(layer, weight);
                yield return new WaitForSeconds(.01f);
            }

            yield return new WaitForSeconds(duration - .5f);

            while (weight >= 0)
            {
                weight -= 0.1f;
                m_DriverAnimator.SetLayerWeight(layer, weight);
                yield return new WaitForSeconds(.01f);
            }

            m_DriverAnimator.CrossFade("Idle", .1f, layer);

            StopCoroutine("FadeLayerWeight");
        }
    }
}
