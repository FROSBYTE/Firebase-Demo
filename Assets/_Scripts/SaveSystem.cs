using System.Collections;
using Firebase.Firestore;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using Firebase.Extensions;

[FirestoreData]
public class SaveData
{
    private string userNmae = "John";
    private int health = 100;
    private float difficultyModifier = 1.0f;

    [FirestoreProperty]
    public string UserNmae { get => userNmae; set => userNmae = value; }

    [FirestoreProperty]
    public int Health { get => health; set => health = value; }

    [FirestoreProperty]
    public float DifficultyModifier { get => difficultyModifier; set => difficultyModifier = value; }
}

public class SaveSystem : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    private FirebaseFirestore db;

    private void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void SaveToCloud()
    {
        SaveData saveData = new SaveData();
        db.Document("users/John").SetAsync(saveData);
        // Save the game data to the cloud
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
              var saveData = task.Result.ConvertTo<SaveData>();
              debugText.text = $"User: {saveData.UserNmae}\n" +
                               $"Health: {saveData.Health}\n" +
                               $"Diff: {saveData.DifficultyModifier}";
              debugText.gameObject.SetActive(true);
              Debug.Log("Save data loaded successfully and UI updated");
          }
          else
          {
              Debug.LogError($"Failed to load save: {task.Exception}");
          }
      });
}

}
