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
                    Debug.Log(emailField.text + " �� �α��� �ϼ̽��ϴ�.");
                }
                else
                {
                    Debug.Log("�α��ο� �����ϼ̽��ϴ�.");
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
                    Debug.Log(emailField.text + "�� ȸ������\n");
                }
                else
                    Debug.Log("ȸ������ ����\n");
            }
            );
    }
}