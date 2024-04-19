using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class AnimatePathSimple : MonoBehaviour
    {
        public Transform target;
        public float animationSpeed = 1;
        public Transform[] waypoints;
        private int m_WaypointIndex;

        private void Start()
        {
            ResetAnimation(false);
        }

        public void ResetAnimation(bool isReversed)
        {
            if (isReversed)
                m_WaypointIndex = waypoints.Length - 1;
            else
                m_WaypointIndex = 0;

            target.position = waypoints[m_WaypointIndex].transform.position;
        }

        public void StartAnimation(float delay, bool isReversed)
        {
            ResetAnimation(isReversed);

            if (isReversed)
                StartCoroutine(AnimateReverse(delay));
            else
                StartCoroutine(Animate(delay));
        }

        IEnumerator Animate(float delay)
        {
            yield return new WaitForSeconds(delay);

            while (m_WaypointIndex <= waypoints.Length - 1)
            {
                float t = Time.deltaTime * animationSpeed;

                ChangePosition(t);

                if (target.position == waypoints[m_WaypointIndex].transform.position)
                    m_WaypointIndex++;

                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator AnimateReverse(float delay)
        {
            yield return new WaitForSeconds(delay);

            while (m_WaypointIndex >= 0)
            {
                float t = Time.deltaTime * animationSpeed;

                ChangePosition(t);

                if (target.position == waypoints[m_WaypointIndex].transform.position)
                    m_WaypointIndex--;

                yield return new WaitForEndOfFrame();
            }
        }

        private void ChangePosition(float t)
        {
            target.position = Vector3.MoveTowards(target.position, waypoints[m_WaypointIndex].transform.position, t);
        }
    }
}
