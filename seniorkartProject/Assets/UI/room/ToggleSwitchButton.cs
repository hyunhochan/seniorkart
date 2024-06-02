using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleSwitchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Button button; 
    public Sprite offSprite; 
    public Sprite onSprite; 
    public Sprite offPressedSprite;
    public Sprite onPressedSprite; 

    public bool isOn = false; // 현재 ON 상태인지 여부(READY 상태가 ON임)

    public void OnPointerDown(PointerEventData eventData)
    {
        button.image.sprite = isOn ? onPressedSprite : offPressedSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isOn = !isOn;
        button.image.sprite = isOn ? onSprite : offSprite;
    }

    void Start()
    {
        button.image.sprite = offSprite;
    }
}
