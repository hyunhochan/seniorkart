using UnityEngine;
using UnityEngine.UI;
using MapData;

public class ButtonSelect : MonoBehaviour
{
    private bool isSelected = false;
    private TrackInfo trackInfo;

    void Start()
    {
        trackInfo = GetComponent<TrackInfo>();
    }

    public void OnButtonClick()
    {
        // 현재 선택된 상태를 반전
        isSelected = !isSelected;


        // 다른 버튼들이 선택된 상태를 업데이트하도록 ToggleGroupManager에 알림
        FindObjectOfType<ToggleGroupManager>().ButtonSelected(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    
    public bool IsSelected()
    {
        return isSelected;
    }

    public string GetTrackNumber()
    {
        return trackInfo != null ? trackInfo.trackNumber : string.Empty;
    }
}
