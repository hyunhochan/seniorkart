using UnityEngine;
using UnityEngine.UI;
using System;

public class ToggleGroupManager : MonoBehaviour
{
    public ButtonSelect[] buttons;  // 수동으로 할당하는 버튼 배열
    public ButtonSelect currentlySelectedButton;
    public ButtonSelect initiallySelectedButton;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        // 최초 선택된 버튼을 초기화 (0번 버튼으로 가정)
        if (buttons.Length > 0)
        {
            initiallySelectedButton = buttons[0];
        }
    }

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);

        // 초기 선택 상태 설정
        if (initiallySelectedButton != null)
        {
            SetSelectedButton(initiallySelectedButton);
        }
    }

    private void OnEnable()
    {
        // 활성화될 때 초기 상태를 기억
        // 여기서 buttons 배열의 순서를 강제로 재설정
        Array.Sort(buttons, (a, b) => a.trackNumber.CompareTo(b.trackNumber));

        if (initiallySelectedButton != null)
        {
            SetSelectedButton(initiallySelectedButton);
        }
    }

    public void ButtonSelected(ButtonSelect selectedButton)
    {
        SetSelectedButton(selectedButton);
    }

    private void SetSelectedButton(ButtonSelect selectedButton)
    {
        foreach (ButtonSelect button in buttons)
        {
            if (button != selectedButton)
            {
                button.SetSelected(false);
            }
        }
        currentlySelectedButton = selectedButton;
        currentlySelectedButton.SetSelected(true);
    }

    public int GetCurrentlySelectedButtonTrackNumber()
    {
        if (currentlySelectedButton != null)
        {
            return currentlySelectedButton.trackNumber;
        }
        return 0; // 선택된 버튼이 없을 때 빈 문자열 반환
    }

    private void OnConfirm()
    {
        // 현재 상태를 유지하고, 초기 상태를 업데이트
        initiallySelectedButton = currentlySelectedButton;
    }

    private void OnCancel()
    {
        // 초기 상태로 되돌림
        if (initiallySelectedButton != null)
        {
            SetSelectedButton(initiallySelectedButton);
        }
    }
}
