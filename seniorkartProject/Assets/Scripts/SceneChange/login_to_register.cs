using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class login_to_register: MonoBehaviour
{
    public void SceneChange()
    {
        SceneManager.LoadScene("FirebaseRegister");
    }
}
