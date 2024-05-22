using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManagerMain : MonoBehaviour
{
    public static UIManagerMain instance;

    public TMP_InputField nameInputField;
    public TMP_InputField ageInputField;
    public TMP_InputField occupationInputField;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        AuthenticationManager.Instance.username = nameInputField.text;
        //StartCoroutine(AuthenticationManager.Instance.SaveUserData());
        //GetData_Button();
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
}
