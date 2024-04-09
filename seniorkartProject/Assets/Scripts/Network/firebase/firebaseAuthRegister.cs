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
                Debug.Log(emailField.text + "로 회원가입");

                WriteNewUser(newUser.UserId, nicknameField.text, newUser.Email);
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                if (task.Exception != null)
                {
                    FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                    string message = "회원가입 실패: ";
                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            message += "이메일 주소가 누락되었습니다.";
                            break;
                        case AuthError.MissingPassword:
                            message += "비밀번호가 누락되었습니다.";
                            break;
                        case AuthError.WeakPassword:
                            message += "비밀번호가 너무 약합니다.";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            message += "이미 사용 중인 이메일입니다.";
                            break;
                        default:
                            message += "알 수 없는 에러가 발생했습니다.";
                            break;
                    }
                    Debug.LogError(message + " 에러 코드: " + errorCode);
                }
                else
                {
                    Debug.LogError("회원가입 실패. 알 수 없는 에러.");
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
                Debug.Log("Firestore에 사용자 정보 저장 완료");
            }
            else
            {
                Debug.LogError("Firestore에 사용자 정보 저장 실패");
            }
        });
    }
}