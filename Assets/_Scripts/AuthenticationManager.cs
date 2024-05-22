using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance;

    [Header("Firebase References")]
    Firebase.FirebaseApp app; // Declaration of FirebaseApp variable
    Firebase.Auth.FirebaseAuth auth; // Declaration of FirebaseAuth variable

    public string username;
    public string userID;
    public string age;
    public string occupation;

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
        Instance = this;
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
        StartCoroutine(LoginCoroutine(loginEmailID.text, loginPassword.text));
    }

    public void ForgotPassword()
    {
        StartCoroutine(ForgotPasswordCoroutine());
    }

    public void GetUserData()
    {
        StartCoroutine(ReadUserDataCoroutine(userID));
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
        Firebase.Auth.FirebaseUser newUser = result.User;

        if (newUser == null)
        {
            createDebugText.text = "Create User encountered an error: user is null";
            yield break;
        }

        createDebugText.text = "User Created Successfully";

        // Set the display name
        string displayName = createName.text;
        Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
        {
            DisplayName = displayName
        };

        var updateProfileTask = newUser.UpdateUserProfileAsync(profile);

        yield return new WaitUntil(() => updateProfileTask.IsCompleted);

        if (updateProfileTask.IsCanceled)
        {
            createDebugText.text = "Update user profile was canceled";
            yield break;
        }

        if (updateProfileTask.IsFaulted)
        {
            createDebugText.text = "Update user profile encountered an error: " + updateProfileTask.Exception;
            yield break;
        }

        if (updateProfileTask.IsCompleted)
        {
            createDebugText.text = "User profile updated successfully";
        }

        ClearInputFields();

        // Optionally, log the user in
        //StartCoroutine(LoginCoroutine(createEmailID.text, createPassword.text));
    }


    private IEnumerator LoginCoroutine(string emailID,string password)
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        var signInTask = auth.SignInWithEmailAndPasswordAsync(emailID, password);

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

        userID = result.User.UserId;
        username = result.User.DisplayName;

        //StartCoroutine(SaveUserData());

        //ClearInputFields();
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

    public IEnumerator SaveUserData()
    {
        // Reference to the Firebase Realtime Database
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        Task setNameTask = reference.Child("users").Child(userID).Child("UserName").SetValueAsync(username);
        Task setIDTask = reference.Child("users").Child(userID).Child("UserId").SetValueAsync(userID);
        Task setAgeTask = reference.Child("users").Child(userID).Child("Age").SetValueAsync(age);
        Task setOccupationTask = reference.Child("users").Child(userID).Child("Occupation").SetValueAsync(occupation);


        yield return Task.WhenAll(setNameTask, setIDTask,setAgeTask,setOccupationTask);

        if (setNameTask.IsFaulted || setIDTask.IsFaulted)
        {
            Debug.LogError("Failed to save user data: " + setNameTask.Exception + ", " + setIDTask.Exception);
        }
        else if (setNameTask.IsCompleted && setIDTask.IsCompleted)
        {
            Debug.Log("User data saved successfully!");
        }
    }

    public IEnumerator ReadUserDataCoroutine(string userId)
    {
        // Reference to the Firebase Realtime Database
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        // Retrieve user data from the database for the specified userId
        var userDataTask = reference.Child("users").Child(userId).GetValueAsync();

        yield return new WaitUntil(() => userDataTask.IsCompleted);

        if (userDataTask.IsCanceled)
        {
            Debug.LogError("ReadUserData was canceled.");
            yield break;
        }

        if (userDataTask.IsFaulted)
        {
            Debug.LogError("ReadUserData encountered an error: " + userDataTask.Exception);
            yield break;
        }

        // Retrieve user data from the database
        DataSnapshot snapshot = userDataTask.Result;
        if (snapshot != null && snapshot.Exists)
        {
            // Extract user data
            username = snapshot.Child("UserName").Value.ToString();
            userID = snapshot.Child("UserId").Value.ToString();
            age = snapshot.Child("Age").Value.ToString();
            occupation = snapshot.Child("Occupation").Value.ToString();

            UIManagerMain.instance.nameInputField.text = username;
            UIManagerMain.instance.ageInputField.text = age;
            UIManagerMain.instance.occupationInputField.text = occupation;

            // Do something with the retrieved data
            Debug.Log("User data retrieved successfully - UserName: " + username + ", UserID: " + userID);
        }
        else
        {
            Debug.LogError("No user data found for UserID: " + userId);
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
