using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class register_to_login : MonoBehaviour
{
    public void SceneChange()
    {
        SceneManager.LoadScene("firebaseAuthLogin");
    }
}
