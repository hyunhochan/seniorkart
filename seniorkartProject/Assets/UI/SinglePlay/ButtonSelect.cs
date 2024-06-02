using UnityEngine;
using UnityEngine.UI;
using MapData;

public class ButtonSelect : MonoBehaviour
{
    public int trackNumber;

    private bool isSelected = false;
    private TrackInfo trackInfo;


    void Start()
    {
        trackInfo = GetComponent<TrackInfo>();
        if (trackInfo != null && int.TryParse(trackInfo.trackNumber, out int parsedTrackNumber))
        {
            trackNumber = parsedTrackNumber;
        }
    }

    public void OnButtonClick()
    {
        isSelected = !isSelected;

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

}
