using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class UIContainerAnimationButtons : MonoBehaviour
    {
        public GameObject contentDrivingAnimBtns;
        public GameObject contentStandingAnimBtns;

        private void OnEnable()
        {
            UIButton.OnClick += HandleContainer;

            contentDrivingAnimBtns.SetActive(DEMODriverAnimations.isDriving);
            contentStandingAnimBtns.SetActive(!DEMODriverAnimations.isDriving);
        }

        private void OnDisable()
        {
            UIButton.OnClick -= HandleContainer;
        }

        private void HandleContainer(UIButtonAction buttonAction)
        {
            if (buttonAction.actionName == "Switch")
                StartCoroutine(ChangeContainer(buttonAction.duration));
        }

        IEnumerator ChangeContainer(float duration)
        {
            yield return new WaitForSeconds(duration + .05f);

            contentDrivingAnimBtns.SetActive(DEMODriverAnimations.isDriving);
            contentStandingAnimBtns.SetActive(!DEMODriverAnimations.isDriving);
        }
    }
}
