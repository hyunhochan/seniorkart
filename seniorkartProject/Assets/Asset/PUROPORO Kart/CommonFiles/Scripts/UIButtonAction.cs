using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class UIButtonAction : MonoBehaviour
    {
        [Tooltip("The name of Action, might be helpful when breaking down ButtonActions into their own groups.")]
        public string actionName;
        [Tooltip("How long a button stays active, and whether other buttons are kept idle, for instance.")]
        public float duration;
    }
}
