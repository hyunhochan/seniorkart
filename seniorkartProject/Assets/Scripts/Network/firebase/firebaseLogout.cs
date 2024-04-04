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

        Debug.Log("사용자가 로그아웃 되었습니다.");

        SceneManager.LoadScene("FirebaseAuthLogin");
    }
}
