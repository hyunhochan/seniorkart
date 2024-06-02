using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMap : MonoBehaviour
{
    public bool isopen = false;
    public GameObject ChangeMapSequence;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void appearChangeUI()
    {
        if (isopen==false) {
        ChangeMapSequence.SetActive(!ChangeMapSequence.activeSelf);
        isopen = true;
        }

    }

    public void disappearChangeUI()
    {
        if (isopen == true)
        {
            ChangeMapSequence.SetActive(false);
            isopen = false;
        }
    }
}
