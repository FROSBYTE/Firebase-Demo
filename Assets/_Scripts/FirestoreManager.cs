using System.Collections;
using Firebase.Firestore;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using Firebase.Extensions;

[System.Serializable]
[FirestoreData]
public class UserData
{
    [FirestoreProperty]
    public string UserName { get; set; }
    
    [FirestoreProperty]
    public string Age { get; set; }
    
    [FirestoreProperty]
    public string Occupation { get; set; }
}

public class FirestoreManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField userNameInputField;
    public TMP_InputField ageInputField;
    public TMP_InputField occupationInputField;
    public TextMeshProUGUI debugText;
    private FirebaseFirestore db;

    private void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void SaveToCloud()
    {
        // Create UserData from input field values
        UserData userData = new UserData
        {
            UserName = userNameInputField.text,
            Age = ageInputField.text,
            Occupation = occupationInputField.text
        };
        
        // Save to Firestore
        db.Document("users/John").SetAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                debugText.text = "Data saved successfully to cloud!";
                debugText.gameObject.SetActive(true);
                Debug.Log("User data saved successfully to Firestore");
            }
            else
            {
                debugText.text = $"Failed to save data: {task.Exception}";
                debugText.gameObject.SetActive(true);
                Debug.LogError($"Failed to save user data: {task.Exception}");
            }
        });
    }

    public void LoadFromCloud()
    {
        db
          .Document("users/John")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
              {
                  var userData = task.Result.ConvertTo<UserData>();
                  
                  // Apply loaded data to input fields
                  userNameInputField.text = userData.UserName ?? "";
                  ageInputField.text = userData.Age ?? "";
                  occupationInputField.text = userData.Occupation ?? "";
                  
                  // Update debug text
                  debugText.text = $"Data loaded successfully!"; 
                  debugText.gameObject.SetActive(true);
                  Debug.Log("User data loaded successfully from Firestore and applied to UI");
              }
              else
              {
                  debugText.text = $"Failed to load data: {task.Exception}";
                  debugText.gameObject.SetActive(true);
                  Debug.LogError($"Failed to load user data: {task.Exception}");
              }
          });
    }
}
