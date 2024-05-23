using UnityEngine;

public class ToggleGroupManager : MonoBehaviour
{
    public ButtonSelect[] buttons;
    private ButtonSelect currentlySelectedButton;

    void Start()
    {
        // 버튼 배열 초기화
        buttons = FindObjectsOfType<ButtonSelect>();
    }

    public void ButtonSelected(ButtonSelect selectedButton)
    {
        foreach (ButtonSelect button in buttons)
        {
            if (button != selectedButton)
            {
                button.SetSelected(false);
            }
        }
        currentlySelectedButton = selectedButton;
    }

    public string GetCurrentlySelectedButtonTrackNumber()
    {
        if (currentlySelectedButton != null)
        {
            return currentlySelectedButton.GetTrackNumber();
        }
        return string.Empty; // 선택된 버튼이 없을 때 빈 문자열 반환
    }
}
