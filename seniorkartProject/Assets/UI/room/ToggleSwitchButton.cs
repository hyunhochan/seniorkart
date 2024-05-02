using UnityEngine;
using UnityEngine.EventSystems; // EventSystems를 사용하기 위함
using UnityEngine.UI;

public class ToggleSwitchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Button button; // 버튼 컴포넌트
    public Sprite offSprite; // OFF 상태 스프라이트
    public Sprite onSprite; // ON 상태 스프라이트
    public Sprite offPressedSprite; // OFF 상태에서 누를 때의 스프라이트
    public Sprite onPressedSprite; // ON 상태에서 누를 때의 스프라이트

    public bool isOn = false; // 현재 ON 상태인지 여부(READY 상태가 ON임)

    public void OnPointerDown(PointerEventData eventData)
    {
        // 누를 때 상태에 따라 다른 이미지를 적용
        button.image.sprite = isOn ? onPressedSprite : offPressedSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 버튼에서 손을 떼면 상태 토글
        isOn = !isOn;
        button.image.sprite = isOn ? onSprite : offSprite;
    }

    void Start()
    {
        // 초기 스프라이트 설정
        button.image.sprite = offSprite;
    }
}
