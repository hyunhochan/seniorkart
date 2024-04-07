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
        carNameText.text = "차량 이름: " + cars[currentCarIndex].name;
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
                    Debug.Log("현재 선택한 차 번호가 업데이트되었습니다.");
                }
                else
                {
                    Debug.LogError("차 번호 업데이트에 실패했습니다.");
                }
            });
        }
        else
        {
            Debug.Log("사용자가 로그인되어 있지 않습니다.");
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
                        Debug.Log("차량 선택 정보를 로드했습니다.");
                        currentCarIndex = snapshot.GetValue<int>("selectedCarIndex");
                        UpdateCarDisplay();
                    }
                    else
                    {
                        Debug.Log("Firestore에 차량 선택 정보가 존재하지 않습니다.");
                    }
                }
                else
                {
                    Debug.LogError("Firestore에서 차량 선택 정보를 가져오는 데 실패했습니다.");
                }
            });
        }
        else
        {
            Debug.Log("사용자가 로그인되어 있지 않습니다.");
        }
    }
}