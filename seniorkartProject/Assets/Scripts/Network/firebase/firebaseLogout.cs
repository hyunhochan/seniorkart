using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using System;
using UnityEngine.SceneManagement;

public class LogoutManager : MonoBehaviour
{
    public void Logout()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        auth.SignOut();

        Debug.Log("����ڰ� �α׾ƿ� �Ǿ����ϴ�.");

        SceneManager.LoadScene("FirebaseAuthLogin");
    }
}
