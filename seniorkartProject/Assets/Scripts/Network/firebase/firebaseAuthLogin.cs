using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


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
    private void Start()
    {
        if (auth.CurrentUser != null)
        {
            Debug.Log("로그인 되어있습니다.");
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.Log("로그인 되어있지 않습니다.");
        }
    }

    public void Login()
    {
        auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(
            task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    Debug.Log(emailField.text + " 로 로그인 하셨습니다.");
                    AndroidToast.Show(emailField.text+" 로 로그인하셨습니다.");
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    Debug.Log("로그인에 실패하셨습니다.");
                }
            }
        );
    }
}

