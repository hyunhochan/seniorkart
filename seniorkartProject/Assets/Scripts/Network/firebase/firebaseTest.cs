using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseAuthExample : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        
        auth = FirebaseAuth.DefaultInstance;

      
        //RegisterUser("user@example.com", "password123");
    }

    public void RegisterUser(string email, string password)
    {
        
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

            
            if (task.IsCompleted)
            {
                FirebaseUser newUser = task.Result.User;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            }
        });
    }
}