using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
[System.Serializable]
public struct Car
{
    public GameObject model;
    public string name;
}

public class garageManage : MonoBehaviour
{
    public Car[] cars;
    public TMP_Text carNameText;
    private int currentCarIndex = 0;
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        LoadCarSelection();
    }

    public void NextCar()
    {
        currentCarIndex = (currentCarIndex + 1) % cars.Length;
        UpdateCarDisplay();
    }

    public void PreviousCar()
    {
        currentCarIndex = (currentCarIndex - 1 + cars.Length) % cars.Length;
        UpdateCarDisplay();
    }

    void UpdateCarDisplay()
    {
        foreach (var car in cars)
        {
            car.model.SetActive(false);
        }

        cars[currentCarIndex].model.SetActive(true);
        carNameText.text = "���� �̸�: " + cars[currentCarIndex].name;
    }

    public void SaveCarSelection()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            DocumentReference docRef = firestore.Collection("users").Document(user.UserId);
            Dictionary<string, object> carData = new Dictionary<string, object>
        {
            { "selectedCarIndex", currentCarIndex }
        };
            docRef.UpdateAsync(carData).ContinueWith(task => {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    Debug.Log("���� ������ �� ��ȣ�� ������Ʈ�Ǿ����ϴ�.");
                }
                else
                {
                    Debug.LogError("�� ��ȣ ������Ʈ�� �����߽��ϴ�.");
                }
            });
        }
        else
        {
            Debug.Log("����ڰ� �α��εǾ� ���� �ʽ��ϴ�.");
        }
    }

    void LoadCarSelection()
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
                        Debug.Log("���� ���� ������ �ε��߽��ϴ�.");
                        currentCarIndex = snapshot.GetValue<int>("selectedCarIndex");
                        UpdateCarDisplay();
                    }
                    else
                    {
                        Debug.Log("Firestore�� ���� ���� ������ �������� �ʽ��ϴ�.");
                    }
                }
                else
                {
                    Debug.LogError("Firestore���� ���� ���� ������ �������� �� �����߽��ϴ�.");
                }
            });
        }
        else
        {
            Debug.Log("����ڰ� �α��εǾ� ���� �ʽ��ϴ�.");
        }
    }
}