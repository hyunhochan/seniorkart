using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using UnityEngine.UI;
using TMPro;

public class AuthManager : MonoBehaviour
{

    [SerializeField] TMP_InputField emailField;
    [SerializeField] TMP_InputField passwordField;

    Firebase.Auth.FirebaseAuth auth;

    void Awake()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }
    public void login()
    {
        auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(
            task => {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    Debug.Log(emailField.text + " 로 로그인 하셨습니다.");
                }
                else
                {
                    Debug.Log("로그인에 실패하셨습니다.");
                }
            }
        );
    }
    public void register()
    {
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(
            task => {
                if (!task.IsCanceled && !task.IsFaulted)
                {
                    Debug.Log(emailField.text + "로 회원가입\n");
                }
                else
                    Debug.Log("회원가입 실패\n");
            }
            );
    }
}