using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class UIButtomActionDriverAnimation : UIButtonAction
    {
        public string animationName;
        public int layer;

        private void Awake()
        {
            actionName = "Animations";

            if (duration <= 0)
                duration = 4;
        }
    }
}
