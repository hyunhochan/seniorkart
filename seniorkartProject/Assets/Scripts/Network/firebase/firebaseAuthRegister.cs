using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.UI;
using TMPro;
using Firebase.Extensions;
using Firebase;
using UnityEngine.SceneManagement;

public class AuthRegisterManager : MonoBehaviour
{
    [SerializeField] TMP_InputField nicknameField;
    [SerializeField] TMP_InputField emailField;
    [SerializeField] TMP_InputField passwordField;

    FirebaseAuth auth;
    FirebaseFirestore db;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public void Register()
    {
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                AuthResult result = task.Result;
                FirebaseUser newUser = result.User;
                Debug.Log(emailField.text + "�� ȸ������");

                WriteNewUser(newUser.UserId, nicknameField.text, newUser.Email);
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                if (task.Exception != null)
                {
                    FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                    string message = "ȸ������ ����: ";
                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            message += "�̸��� �ּҰ� �����Ǿ����ϴ�.";
                            break;
                        case AuthError.MissingPassword:
                            message += "��й�ȣ�� �����Ǿ����ϴ�.";
                            break;
                        case AuthError.WeakPassword:
                            message += "��й�ȣ�� �ʹ� ���մϴ�.";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            message += "�̹� ��� ���� �̸����Դϴ�.";
                            break;
                        default:
                            message += "�� �� ���� ������ �߻��߽��ϴ�.";
                            break;
                    }
                    Debug.LogError(message + " ���� �ڵ�: " + errorCode);
                }
                else
                {
                    Debug.LogError("ȸ������ ����. �� �� ���� ����.");
                }
            }
        });
    }

    void WriteNewUser(string userId, string nickname, string email)
    {
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "nickname", nickname },
            { "email", email }
        };

        DocumentReference docRef = db.Collection("users").Document(userId);
        docRef.SetAsync(user).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Firestore�� ����� ���� ���� �Ϸ�");
            }
            else
            {
                Debug.LogError("Firestore�� ����� ���� ���� ����");
            }
        });
    }
}