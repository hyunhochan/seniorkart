using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI;
using TMPro;

public class AuthManager : MonoBehaviour
{
    [SerializeField] TMP_InputField emailField;
    [SerializeField] TMP_InputField passwordField;

    FirebaseAuth auth;
    DatabaseReference databaseReference;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
       
    }

    public void Login()
    {
        auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(
            task =>
            {
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
}

