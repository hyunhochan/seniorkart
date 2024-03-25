using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseAuthExample : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        // Firebase 인증 객체의 인스턴스를 초기화합니다.
        auth = FirebaseAuth.DefaultInstance;

        // 사용자를 등록하는 함수를 호출합니다.
        //RegisterUser("user@example.com", "password123");
    }

    public void RegisterUser(string email, string password)
    {
        // 이메일과 비밀번호를 사용하여 새 사용자를 생성합니다.
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // 비동기 작업이 성공적으로 완료된 경우에만 FirebaseUser에 접근합니다.
            if (task.IsCompleted)
            {
                FirebaseUser newUser = task.Result.User; // AuthResult에서 User 속성을 통해 FirebaseUser에 접근합니다.
                Debug.LogFormat("Firebase user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            }
        });
    }
}