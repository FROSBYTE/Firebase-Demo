using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    Firebase.FirebaseApp app; // Declaration of FirebaseApp variable
    Firebase.Auth.FirebaseAuth auth; // Declaration of FirebaseAuth variable

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

        // Set the display name of the user
        Firebase.Auth.FirebaseUser user = result.User;
        Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
        {
            DisplayName = createName.text
        };

        var profileTask = user.UpdateUserProfileAsync(profile);
        yield return new WaitUntil(() => profileTask.IsCompleted);

        if (profileTask.IsCanceled || profileTask.IsFaulted)
        {
            createDebugText.text = "Failed to set display name: " + profileTask.Exception;
            yield break;
        }

        // Display name set successfully
        Debug.LogFormat("Firebase user created successfully: {0} ({1})", user.DisplayName, user.UserId);
        createDebugText.text = "User Created Successfully";
    }


    private IEnumerator LoginCoroutine()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        var signInTask = auth.SignInWithEmailAndPasswordAsync(loginEmailID.text, loginPassword.text);

        // Wait until the sign-in task is complete
        while (!signInTask.IsCompleted)
        {
            yield return null; // Wait for the next frame
        }

        if (signInTask.IsCanceled)
        {
            Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
            yield break;
        }

        if (signInTask.IsFaulted)
        {
            Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + signInTask.Exception);
            yield break;
        }

        // Firebase user has signed in.
        Firebase.Auth.AuthResult result = signInTask.Result;
        Debug.LogFormat("User signed in successfully: {0} ({1})",
            result.User.DisplayName, result.User.UserId);
    }

    #endregion
}
