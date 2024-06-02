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

    public GameObject optionSequence;

    void Start()
    {
        optionSequence.SetActive(false);
        RectTransform rectTransform = playgamebtn.GetComponent<RectTransform>();

        float btnwidth = rectTransform.rect.width;
        float btnheight = rectTransform.rect.height;

        mainRectTransform.DOAnchorPosX(btnwidth*2/3, 0.5f);
    }

    void Update()
    {
        
    }

    public void clickPlay()
    {
        RectTransform rectTransform = playgamebtn.GetComponent<RectTransform>();

        float btnwidth = rectTransform.rect.width;
        float btnheight = rectTransform.rect.height;

        mainRectTransform.DOAnchorPosX(0, 0.5f);
        playRectTransform.DOAnchorPosX(btnwidth * 2 / 3, 0.5f);

    }

    public void clickPlayBack()
    {
        RectTransform rectTransform = playgamebtn.GetComponent<RectTransform>();

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
