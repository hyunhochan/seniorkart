using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainMove : MonoBehaviour
{
    public Button playgamebtn;
    public GameObject mainsequence;
    public GameObject playsequence;
    public RectTransform mainRectTransform;
    public RectTransform playRectTransform;

    public GameObject optionSequence; // 활성화/비활성화할 오브젝트

    // Start is called before the first frame update
    void Start()
    {
        optionSequence.SetActive(false);
        // Button의 RectTransform을 가져옴
        RectTransform rectTransform = playgamebtn.GetComponent<RectTransform>();

        // 높이와 너비를 출력
        float btnwidth = rectTransform.rect.width;
        float btnheight = rectTransform.rect.height;

        mainRectTransform.DOAnchorPosX(btnwidth*2/3, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void clickPlay()
    {
        // Button의 RectTransform을 가져옴
        RectTransform rectTransform = playgamebtn.GetComponent<RectTransform>();

        // 높이와 너비를 출력
        float btnwidth = rectTransform.rect.width;
        float btnheight = rectTransform.rect.height;

        mainRectTransform.DOAnchorPosX(0, 0.5f);
        playRectTransform.DOAnchorPosX(btnwidth * 2 / 3, 0.5f);

    }

    public void clickPlayBack()
    {
        // Button의 RectTransform을 가져옴
        RectTransform rectTransform = playgamebtn.GetComponent<RectTransform>();

        // 높이와 너비를 출력
        float btnwidth = rectTransform.rect.width;
        float btnheight = rectTransform.rect.height;

        playRectTransform.DOAnchorPosX(0, 0.5f);
        mainRectTransform.DOAnchorPosX(btnwidth * 2 / 3, 0.5f);

    }

    public void appearoption()
    {
        optionSequence.SetActive(!optionSequence.activeSelf);
        mainsequence.SetActive(false);

    }

    public void disappearoption()
    {
        optionSequence.SetActive(false);
        mainsequence.SetActive(!mainsequence.activeSelf);
    }

}
