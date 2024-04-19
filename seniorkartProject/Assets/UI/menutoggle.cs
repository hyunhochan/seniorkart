using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class menutoggle : MonoBehaviour
{
    public Toggle[] toggles; // 모든 Toggle 요소를 저장할 배열
    public Sprite selectedSprite; // 선택된 상태의 스프라이트
    public Sprite unselectedSprite; // 선택되지 않은 상태의 스프라이트

    void Start()
    {
        // 각 Toggle에 대한 이벤트 리스너 추가
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener(delegate {
                ToggleClick();
            });
        }
    }

    public void ToggleClick()
    {
        foreach (var toggle in toggles)
        {
            // TextMeshPro 컴포넌트를 찾아서 색상을 변경합니다.
            var textComponent = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.color = toggle.isOn ? new Color(1f, 0.7529412f, 0f, 1f) : new Color(178f / 255f, 178f / 255f, 178f / 255f, 1f);
            }

            // Toggle의 스프라이트 이미지 변경
            toggle.GetComponent<Image>().sprite = toggle.isOn ? selectedSprite : unselectedSprite;
        }
    }



}
