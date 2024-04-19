using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    /// <summary>
    /// Automatically rotates object.
    /// </summary>
    public class AutoRotate : MonoBehaviour
    {
        public Vector3 speed = new Vector3(0, 48, 0);

        private void LateUpdate()
        {
            transform.Rotate(
                 speed.x * Time.deltaTime,
                 speed.y * Time.deltaTime,
                 speed.z * Time.deltaTime
            );
        }
    }
}