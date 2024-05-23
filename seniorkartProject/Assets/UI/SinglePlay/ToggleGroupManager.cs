using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MapData;
using Unity.Netcode;

public class ToggleGroupManager : NetworkBehaviour
{
    public ButtonSelect[] buttons;  // 수동으로 할당하는 버튼 배열
    public ButtonSelect currentlySelectedButton;
    public ButtonSelect initiallySelectedButton;
    public Sprite initialTrackImage;
    public Sprite initialMiniMapImage;
    public string initialTrackName;
    public int initialSelectedTrackIndex;
    public CharacterSelectDisplay characterselectdisplay;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Image targetImage; // 덧씌울 이미지 컴포넌트를 지정할 공개 변수
    [SerializeField] private Image miniMap; // 덧씌울 이미지 컴포넌트를 지정할 공개 변수
    [SerializeField] private TextMeshProUGUI targetTrackName; // 트랙 이름을 표시할 TMP Text 컴포넌트

    private void Awake()
    {
        // 버튼 배열을 trackNumber 기준으로 정렬
        Array.Sort(buttons, (a, b) => a.trackNumber.CompareTo(b.trackNumber));
        // 최초 선택된 버튼을 초기화 (0번 버튼으로 가정)
        if (buttons.Length > 0)
        {
            initiallySelectedButton = buttons[0];
            StoreAsInitialState();
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

    private void StoreAsInitialState()
    {
        if (initiallySelectedButton != null)
        {
            TrackInfo trackInfo = currentlySelectedButton.GetComponent<TrackInfo>();
            initialTrackImage = trackInfo.trackImage;
            initialMiniMapImage = trackInfo.miniMap;
            initialTrackName = trackInfo.trackName;
            initialSelectedTrackIndex = initiallySelectedButton.trackNumber;
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
        TrackInfo trackInfo = selectedButton.GetComponent<TrackInfo>();
        targetImage.sprite = trackInfo.trackImage;
        miniMap.sprite = trackInfo.miniMap;
        targetTrackName.text = trackInfo.trackName;
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
        StoreAsInitialState();
        UpdateTrackDetails(currentlySelectedButton);

        Debug.Log("ishost = " + IsHost);


        characterselectdisplay.ConfirmSelectionClientRpc(initialSelectedTrackIndex);
        Debug.Log("launched host trigger");

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

            SetSelectedButton(initiallySelectedButton);
        }
    }

    public void getNumbers(int selectedTrackIndex)
    {
        initialSelectedTrackIndex = selectedTrackIndex;
        initiallySelectedButton = buttons[selectedTrackIndex]; //initialSelectedTrackIndex를 받아와서 트랙 번호에 맞는 버튼을 찾아서 initiallySelectedButton에 대입해준다.
        currentlySelectedButton = initiallySelectedButton;
        StoreAsInitialState(); //initiallySelectedButton에 저장된 맵 데이터를 기반으로 화면 우측의 맵 정보를 업데이트하는 함수
        UpdateTrackDetails(initiallySelectedButton);
        Debug.Log("launched clientlower trigger");
        Debug.Log("number = " + selectedTrackIndex);
    }
}
