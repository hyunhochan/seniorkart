using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;

public class FetchUserNickname : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        FetchNickname();
    }

    void FetchNickname()
    {
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            DocumentReference docRef = firestore.Collection("users").Document(user.UserId);
            docRef.GetSnapshotAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    DocumentSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        Debug.Log("닉네임: " + snapshot.GetValue<string>("nickname"));
                    }
                    else
                    {
                        Debug.Log("사용자 정보가 Firestore에 존재하지 않습니다.");
                    }
                }
                else
                {
                    Debug.LogError("Firestore에서 사용자 정보를 가져오는 데 실패했습니다.");
                }
            });
        }
        else
        {
            Debug.Log("사용자가 로그인되어 있지 않습니다.");
        }
    }
}