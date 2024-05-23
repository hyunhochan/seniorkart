using System;  // Array.Sort를 사용하기 위해 추가
using UnityEngine;
using UnityEngine.UI;
using MapData;
using TMPro;  // TMP 텍스트 관련 네임스페이스 추가

public class ToggleGroupManager : MonoBehaviour
{
    public ButtonSelect[] buttons;  // 수동으로 할당하는 버튼 배열
    public ButtonSelect currentlySelectedButton;
    public ButtonSelect initiallySelectedButton;
    private Sprite initialTrackImage;
    private Sprite initialMiniMapImage;
    private string initialTrackName;
    //private string initialBestRecord;
    //private string initialBestKart;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Image targetImage; // 덧씌울 이미지 컴포넌트를 지정할 공개 변수
    [SerializeField] private Image miniMap; // 덧씌울 이미지 컴포넌트를 지정할 공개 변수
    [SerializeField] private TextMeshProUGUI targetTrackName; // 트랙 이름을 표시할 TMP Text 컴포넌트
    //[SerializeField] private TextMeshProUGUI targetBestRecord; // 최고 기록을 표시할 TMP Text 컴포넌트
    //[SerializeField] private TextMeshProUGUI targetBestKart; // 최고 카트를 표시할 TMP Text 컴포넌트

    private void Awake()
    {
        // 버튼 배열을 trackNumber 기준으로 정렬
        Array.Sort(buttons, (a, b) => a.trackNumber.CompareTo(b.trackNumber));
        // 최초 선택된 버튼을 초기화 (0번 버튼으로 가정)
        if (buttons.Length > 0)
        {
            initiallySelectedButton = buttons[0];
            StoreInitialState();
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
        if (initiallySelectedButton != null)
        {
            SetSelectedButton(initiallySelectedButton);
        }
    }

    private void StoreInitialState()
    {
        if (initiallySelectedButton != null)
        {
            TrackInfo trackInfo = initiallySelectedButton.GetComponentInParent<TrackInfo>();
            if (trackInfo != null)
            {
                initialTrackImage = trackInfo.trackImage;
                initialMiniMapImage = trackInfo.miniMap;
                initialTrackName = trackInfo.trackName;
                //initialBestRecord = trackInfo.BestRecord1st;
                //initialBestKart = trackInfo.KartBody1st;
            }
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

        // Update the displayed track information
        UpdateTrackDetails(currentlySelectedButton);
    }

    private void UpdateTrackDetails(ButtonSelect selectedButton)
    {
        TrackInfo trackInfo = selectedButton.GetComponentInParent<TrackInfo>();
        if (trackInfo != null)
        {
            targetImage.sprite = trackInfo.trackImage;
            miniMap.sprite = trackInfo.miniMap;
            targetTrackName.text = trackInfo.trackName;
            //targetBestRecord.text = trackInfo.BestRecord1st;
            //targetBestKart.text = trackInfo.KartBody1st;
        }
    }

    public int GetCurrentlySelectedButtonTrackNumber()
    {
        if (currentlySelectedButton != null)
        {
            return currentlySelectedButton.trackNumber;
        }
        return -1; // 선택된 버튼이 없을 때 -1 반환
    }

    private void OnConfirm()
    {
        // 현재 상태를 유지하고, 초기 상태를 업데이트
        initiallySelectedButton = currentlySelectedButton;
        StoreInitialState();
    }

    private void OnCancel()
    {
        // 초기 상태로 되돌림
        if (initiallySelectedButton != null)
        {
            // Restore initial track details
            targetImage.sprite = initialTrackImage;
            miniMap.sprite = initialMiniMapImage;
            targetTrackName.text = initialTrackName;
            //targetBestRecord.text = initialBestRecord;
            //targetBestKart.text = initialBestKart;

            SetSelectedButton(initiallySelectedButton);
        }
    }
}
