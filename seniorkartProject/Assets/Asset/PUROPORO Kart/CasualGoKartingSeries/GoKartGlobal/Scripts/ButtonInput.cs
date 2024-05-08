using UnityEngine;
using UnityEngine.EventSystems;
namespace PUROPORO
{
    public class ButtonInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public GoKartController kartController;
        public DEMODriverAnimations driverAnimations;
        public float value;
        public bool isBrakeButton = false;
        public bool isTurnButton = false;  // 회전 버튼 구분

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isBrakeButton)
            {
                kartController.buttonBrake = 1;
            }
            else if (isTurnButton)
            {
                driverAnimations.buttonTurning += value; // 회전 값을 증가시킴
                kartController.buttonSteering += value; // 필요한 경우에 추가
            }
            else
            {
                kartController.buttonSteering += value; // 가속 혹은 기타 버튼 처리
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isBrakeButton)
            {
                kartController.buttonBrake = 0;
            }
            else if (isTurnButton)
            {
                driverAnimations.buttonTurning -= value; // 회전 값을 감소시킴
                kartController.buttonSteering -= value; // 필요한 경우에 추가
            }
            else
            {
                kartController.buttonSteering -= value; // 가속 혹은 기타 버튼 처리
            }
        }

    }
}