using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    [Header("Firebase References")]
    Firebase.FirebaseApp app; // Declaration of FirebaseApp variable
    Firebase.Auth.FirebaseAuth auth; // Declaration of FirebaseAuth variable
    public string username;
    public string userID;

    [Header("Login Panel References")]
    [SerializeField] TMP_InputField loginEmailID;
    [SerializeField] TMP_InputField loginPassword;
    [SerializeField] TextMeshProUGUI loginDebugText;

    [Header("Create Panel References")]
    [SerializeField] TMP_InputField createName;
    [SerializeField] TMP_InputField createEmailID;
    [SerializeField] TMP_InputField createPassword;
    [SerializeField] TextMeshProUGUI createDebugText;

    [Header("Forgot Password Panel References")]
    [SerializeField] TMP_InputField forgotPassEmailID;
    [SerializeField] TextMeshProUGUI forgotPassDebugText;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;

                // Initialize FirebaseAuth instance
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void CreateAccount()
    {
        StartCoroutine(CreateUserCoroutine());
    }

    public void LoginAccount()
    {
        StartCoroutine(LoginCoroutine());
    }

    public void ForgotPassword()
    {
        StartCoroutine(ForgotPasswordCoroutine());
    }

    #region Coroutine Functions

    private IEnumerator CreateUserCoroutine()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        var createUserTask = auth.CreateUserWithEmailAndPasswordAsync(createEmailID.text, createPassword.text);

        yield return new WaitUntil(() => createUserTask.IsCompleted);

        if (createUserTask.IsCanceled)
        {
            createDebugText.text = "Create User was canceled";
            yield break;
        }

        if (createUserTask.IsFaulted)
        {
            createDebugText.text = "Create User encountered an error: " + createUserTask.Exception;
            yield break;
        }

        // Firebase user has been created.
        Firebase.Auth.AuthResult result = createUserTask.Result;

        createDebugText.text = "User Created Succesfully";

        ClearInputFields();
       
        StartCoroutine(LoginCoroutine());
    }

    private IEnumerator LoginCoroutine()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        var signInTask = auth.SignInWithEmailAndPasswordAsync(loginEmailID.text, loginPassword.text);

        // Wait until the sign-in task is complete
        while (!signInTask.IsCompleted)
        {
            yield return null;
        }

        if (signInTask.IsCanceled)
        {
            loginDebugText.text = "Sign-In was canceled.";
            yield break;
        }

        if (signInTask.IsFaulted)
        {
            loginDebugText.text = "Incorrect Credentials";
            yield break;
        }

        // Firebase user has signed in.
        Firebase.Auth.AuthResult result = signInTask.Result;Debug.LogFormat("User signed in successfully: {0} ({1})",result.User.DisplayName, result.User.UserId);
        loginDebugText.text = "Sign-In Succesfull";

        username = result.User.DisplayName;
        userID = result.User.UserId;

        StartCoroutine(SaveUserData());

        ClearInputFields();
        UIManager.instance.SwitchScene();
    }

    private IEnumerator ForgotPasswordCoroutine()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        var forgotPasswordTask = auth.SendPasswordResetEmailAsync(forgotPassEmailID.text);

        yield return new WaitUntil(() => forgotPasswordTask.IsCompleted);

        if (forgotPasswordTask.IsCanceled)
        {
            forgotPassDebugText.text = "Password reset request was canceled";
            yield break;
        }

        if (forgotPasswordTask.IsFaulted)
        {
            forgotPassDebugText.text = "Password reset request encountered an error: " + forgotPasswordTask.Exception;
            yield break;
        }

        Debug.Log("Password reset email sent successfully to: " + forgotPassEmailID.text);
        forgotPassDebugText.text = "Password reset email sent successfully";
    }

    private IEnumerator SaveUserData()
    {
        // Reference to the Firebase Realtime Database
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        Task setNameTask = reference.Child("users").Child(userID).Child("userName").SetValueAsync(username);
        Task setIDTask = reference.Child("users").Child(userID).Child("userId").SetValueAsync(userID);

        yield return Task.WhenAll(setNameTask, setIDTask);

        if (setNameTask.IsFaulted || setIDTask.IsFaulted)
        {
            Debug.LogError("Failed to save user data: " + setNameTask.Exception + ", " + setIDTask.Exception);
        }
        else if (setNameTask.IsCompleted && setIDTask.IsCompleted)
        {
            Debug.Log("User data saved successfully!");
        }
    }

    #endregion

    public void ClearInputFields()
    {
        loginEmailID.text = "";
        loginPassword.text = "";
        createName.text = "";
        createEmailID.text = "";
        createPassword.text = "";
        forgotPassEmailID.text = "";
    }
}
