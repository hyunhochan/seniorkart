using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace PUROPORO
{
    public class ButtonInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public GoKartController kartController;
        public DEMODriverAnimations driverAnimations;
        public float value = 0.005f; // 점진적으로 증가할 값의 크기
        public float maxInputValue = 1.0f; // 최대 입력 값
        public bool isBrakeButton = false;
        public bool isTurnButton = false;

        private bool isButtonPressed = false;
        private float currentValue = 0;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isButtonPressed)
            {
                StopAllCoroutines();
                isButtonPressed = true;
                StartCoroutine(IncrementValue());
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isButtonPressed)
            {
                isButtonPressed = false;
                StartCoroutine(DecrementValue());
            }
        }

        private IEnumerator DecrementValue()
        {
            if (maxInputValue < 0) //음수, 좌측으로 기울어 있을 때
            {
                while (currentValue < 0)
                {
                    currentValue += value;
                    if (currentValue > 0)
                    {
                        currentValue = 0;
                    }
                    ApplyInput(currentValue);
                    yield return null;
                }
            }
            else if (maxInputValue > 0) //양수, 우측으로 기울어 있을 때
            {
                while (currentValue > 0)
                {
                    currentValue -= value;
                    if (currentValue < 0)
                    {
                        currentValue = 0;
                    }
                    ApplyInput(currentValue);
                    yield return null;
                }
            }

                // 매 프레임마다 value만큼 증가
        }


        private IEnumerator IncrementValue()
        {
            while (isButtonPressed)
            {
                if (maxInputValue < 0)
                {
                    currentValue -= value;
                    if (currentValue < maxInputValue)
                    {
                        currentValue = maxInputValue;
                    }
                }
                else if (maxInputValue > 0)
                {
                    currentValue += value;
                    if (currentValue > maxInputValue)
                    {
                        currentValue = maxInputValue;
                    }
                }
                 // 매 프레임마다 value만큼 증가
                
                ApplyInput(currentValue);
                yield return null;
            }
        }



        private void ApplyInput(float inputValue)
        {
            if (isBrakeButton)
            {
                kartController.buttonBrake = inputValue;
            }
            else if (isTurnButton)
            {
                driverAnimations.buttonTurning = inputValue;
                kartController.buttonSteering = inputValue; // 스티어링 값을 갱신
            }
            else
            {
                kartController.buttonSteering = inputValue;
            }
        }

    }
}
