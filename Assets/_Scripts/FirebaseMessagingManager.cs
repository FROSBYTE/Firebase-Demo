using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class FirebaseMessagingManager : MonoBehaviour
{
    void Awake()
    {
        // Make sure all Firebase dependencies are available
        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                var status = task.Result;
                if (status == DependencyStatus.Available)
                {
                    InitializeFCM();
                }
                else
                {
                    Debug.LogError($"[FCM] Could not resolve all Firebase dependencies: {status}");
                }
            });
    }

    void InitializeFCM()
    {
        // Check and request notification permission
        CheckAndRequestNotificationPermission();

        // Listen for registration token updates
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        // Listen for incoming messages
        FirebaseMessaging.MessageReceived += OnMessageReceived;

        // (Optional) Subscribe to a topic
        FirebaseMessaging.SubscribeAsync("news")
            .ContinueWithOnMainThread(subTask =>
            {
                if (subTask.IsCompleted && !subTask.IsFaulted)
                    Debug.Log("[FCM] Subscribed to topic: news");
                else
                    Debug.LogError($"[FCM] Failed to subscribe: {subTask.Exception}");
            });

        Debug.Log("[FCM] Initialized");
    }

    void CheckAndRequestNotificationPermission()
    {
#if UNITY_ANDROID
        // On Android, check if we have POST_NOTIFICATIONS permission (Android 13+)
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Debug.Log("[FCM] Requesting POST_NOTIFICATIONS permission on Android");
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
        else
        {
            Debug.Log("[FCM] POST_NOTIFICATIONS permission already granted on Android");
        }
#endif

        // Ask Firebase for notification permission (important for iOS)
        FirebaseMessaging.RequestPermissionAsync()
            .ContinueWithOnMainThread(permTask =>
            {
                if (permTask.IsCompleted && !permTask.IsFaulted)
                {
                    Debug.Log("[FCM] Firebase notification permission request completed successfully");
                    // On iOS, this means the user was prompted and made a choice
                    // On Android, this typically succeeds automatically
                    
                    // Request the FCM token after permission is handled
                    RequestFCMToken();
                }
                else
                {
                    Debug.LogError($"[FCM] Failed to request Firebase notification permission: {permTask.Exception}");
                }
            });
    }

    void RequestFCMToken()
    {
        // The token will be received via the OnTokenReceived event
        // This is just to explicitly trigger token generation if needed
        Debug.Log("[FCM] FCM token will be received via OnTokenReceived event");
    }

    void OnTokenReceived(object sender, TokenReceivedEventArgs e)
    {
        Debug.Log($"[FCM] Registration token received: {e.Token}");
        // TODO: send this token to your backend if you need to target this device directly
        
        // Now that we have a token, notifications should work
        Debug.Log("[FCM] Device is ready to receive notifications");
    }

    void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("[FCM] Message received");

        // Notification payload (title/body)
        if (e.Message.Notification != null)
        {
            Debug.Log($"[FCM] Notification Title: {e.Message.Notification.Title}");
            Debug.Log($"[FCM] Notification Body : {e.Message.Notification.Body}");
        }

        // Data payload (key/value pairs)
        foreach (KeyValuePair<string, string> pair in e.Message.Data)
        {
            Debug.Log($"[FCM] Data: {pair.Key} = {pair.Value}");
        }

        // TODO: display an in-game popup or local notification if needed
    }

    void OnDestroy()
    {
        // Clean up event handlers to avoid memory leaks
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }
}
