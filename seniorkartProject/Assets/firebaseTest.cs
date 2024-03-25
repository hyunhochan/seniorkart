using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseAuthExample : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        // Firebase ���� ��ü�� �ν��Ͻ��� �ʱ�ȭ�մϴ�.
        auth = FirebaseAuth.DefaultInstance;

        // ����ڸ� ����ϴ� �Լ��� ȣ���մϴ�.
        //RegisterUser("user@example.com", "password123");
    }

    public void RegisterUser(string email, string password)
    {
        // �̸��ϰ� ��й�ȣ�� ����Ͽ� �� ����ڸ� �����մϴ�.
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

            // �񵿱� �۾��� ���������� �Ϸ�� ��쿡�� FirebaseUser�� �����մϴ�.
            if (task.IsCompleted)
            {
                FirebaseUser newUser = task.Result.User; // AuthResult���� User �Ӽ��� ���� FirebaseUser�� �����մϴ�.
                Debug.LogFormat("Firebase user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            }
        });
    }
}