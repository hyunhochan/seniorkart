using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PUROPORO
{
    public class UIButtonInteractableHandler : MonoBehaviour
    {
        public enum modeType { Single, Group }
        public modeType mode;

        private void OnEnable()
        {
            UIButton.OnClick += HandleInteractable;
        }

        private void OnDisable()
        {
            UIButton.OnClick -= HandleInteractable;
        }

        void HandleInteractable(UIButtonAction buttonAction)
        {
            if (mode == modeType.Single)
                StartCoroutine(ChangeInteractableSingle(buttonAction.duration));
            else
                StartCoroutine(ChangeInteractableGroup(buttonAction.duration));
        }

        IEnumerator ChangeInteractableSingle(float duration)
        {
            transform.GetComponent<Button>().interactable = false;

            yield return new WaitForSeconds(duration);

            transform.GetComponent<Button>().interactable = true;
        }

        IEnumerator ChangeInteractableGroup(float duration)
        {
            foreach (Transform button in transform)
                button.GetComponent<Button>().interactable = false;

            yield return new WaitForSeconds(duration);

            foreach (Transform button in transform)
                button.GetComponent<Button>().interactable = true;
        }
    }
}
