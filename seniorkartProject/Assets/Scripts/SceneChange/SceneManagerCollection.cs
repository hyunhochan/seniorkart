using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerCollection : MonoBehaviour
{
    public void MultiToBootstrap()
    {
        SceneManager.LoadScene("Bootstrap");
    }

}
