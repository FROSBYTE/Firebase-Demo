using System.Collections;
using System.Collections.Generic;
using EasyTransition;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class UIManagerMain : MonoBehaviour
{
    public static UIManagerMain instance;

    public TransitionSettings transitionSettings;
    public TMP_InputField nameInputField;
    public TMP_InputField ageInputField;
    public TMP_InputField occupationInputField;

    public TextMeshProUGUI debugText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        nameInputField.text = AuthenticationManager.Instance.username;
        GetData_Button();
    }

    public void GetData_Button()
    {
        AuthenticationManager.Instance.GetUserData();
        DisableInputField();
    }

    public void SaveData_Button()
    {
        DisableInputField();

        AuthenticationManager.Instance.username = nameInputField.text;
        AuthenticationManager.Instance.age = ageInputField.text;
        AuthenticationManager.Instance.occupation = occupationInputField.text;

        StartCoroutine(AuthenticationManager.Instance.SaveUserData());
    }

    public void EditData_Button()
    {
        EnableInputField();
    }

    public void DisableInputField()
    {
        nameInputField.interactable = false;
        ageInputField.interactable = false;
        occupationInputField.interactable = false;
    }

    public void EnableInputField()
    {
        nameInputField.interactable = true;
        ageInputField.interactable = true;
        occupationInputField.interactable = true;
    }

    public void Logout_Button()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
            Debug.Log("User logged out successfully");
            
            // Clear the stored user data
            AuthenticationManager.Instance.username = "";
            AuthenticationManager.Instance.userID = "";
            AuthenticationManager.Instance.age = "";
            AuthenticationManager.Instance.occupation = "";
            
            // Clear the input fields
            nameInputField.text = "";
            ageInputField.text = "";
            occupationInputField.text = "";
            
            // Disable input fields after logout
            DisableInputField();

            
            
            // You can add additional logic here like switching scenes or showing login panel
            // UIManager.instance.SwitchToLoginScene();
        }
        else
        {
            Debug.Log("No user is currently signed in");
            //TransitionManager.Instance().Transition("Main", transitionSettings, 0f);
        }
        TransitionManager.Instance().Transition("Menu", transitionSettings, 0f);
    }
}
