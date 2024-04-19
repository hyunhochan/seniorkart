using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    /// <summary>
    /// Automatically rotates object.
    /// </summary>
    public class AutoRotateKart: MonoBehaviour
    {
        public Vector3 rotationSpeed = new Vector3(0, 48, 0);

        private void LateUpdate()
        {
            transform.Rotate(
                 rotationSpeed.x * Time.deltaTime,
                 rotationSpeed.y * Time.deltaTime,
                 rotationSpeed.z * Time.deltaTime
            );
        }
    }
}