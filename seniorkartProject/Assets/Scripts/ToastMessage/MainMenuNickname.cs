using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class ToastManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    public TMP_Text toastText;

    public float displayTime = 2.0f;
    string nickname;

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
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    DocumentSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        nickname = snapshot.GetValue<string>("nickname");
                        ShowToast(nickname + ", Hello");
                    }
                }
                else
                {
                    Debug.LogError("Firestore���� ����� ������ �������� �� �����߽��ϴ�.");
                }
            });
        }
        else
        {
            Debug.Log("����ڰ� �α��εǾ� ���� �ʽ��ϴ�.");
        }
    }

    public void ShowToast(string message)
    {
        toastText.text = message;
        StartCoroutine(ShowToastCoroutine());
    }

    private IEnumerator ShowToastCoroutine()
    {
        toastText.gameObject.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        toastText.gameObject.SetActive(false);
    }
}