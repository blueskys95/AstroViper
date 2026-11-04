using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    public GameObject NickNameSetUI;
    public InputField inputField;

    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore db;
    public string NickName;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                AppOptions appOptions = new AppOptions
                {
                    ApiKey = "AIzaSyATTuXvX62VdR2uB4bfZU7WU4Uufszm0A0",
                    AppId = "1:921011526534:web:145836a56b77d14c002a07",
                    ProjectId = "astroviper-photon",
                    DatabaseUrl = new System.Uri("https://astroviper-photon-default-rtdb.firebaseio.com/")
                };

                FirebaseApp APP = FirebaseApp.Create(appOptions);

                db = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                GuestLogin();
                Debug.Log("Firebase ¿¬°á¿¡ ¼º°øÇÏ¿´½À´Ï´Ù.");
            }
            else
            {
                Debug.LogError("Firebase ÃÊ±âÈ­¿¡ ½ÇÆÐÇÏ¿´½À´Ï´Ù.");
            }
        });
    }

    public void GuestLogin()
    {
        string savedUID = PlayerPrefs.GetString("FirebaseUID", "");

        if (!string.IsNullOrEmpty(savedUID))
        {
            CheckOrCreateUser(savedUID);
            return;
        }

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result != null)
            {
                user = task.Result.User;

                PlayerPrefs.SetString("FirebaseUID", user.UserId);
                PlayerPrefs.Save();

                CheckOrCreateUser(user.UserId);
            }
            else
            {
                Debug.LogError("Guest Login Failed");
            }
        });
    }

    public void CheckOrCreateUser(string userID)
    {
        DocumentReference userRef = db.Collection("USERS").Document(userID);

        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                if (task.Result.ContainsField("NICKNAME"))
                {
                    NickName = task.Result.GetValue<string>("NICKNAME");
                }
                Debug.Log($"±âÁ¸ »ç¿ëÀÚ ·Î±×ÀÎ: {userID}, ´Ð³×ÀÓ: {NickName}");

                SceneManager.LoadScene("Main");
            }
            else
            {
                NickNameSetUI.SetActive(true);
            }
        });
    }
    public void SetNickName()
    {
        string nickName = inputField.text;

        if (string.IsNullOrWhiteSpace(nickName))
        {
            Debug.LogError("´Ð³×ÀÓÀ» ÀÔ·ÂÇÏ¼¼¿ä!");
            return;
        }
        if (nickName.Length < 3 || nickName.Length > 10)
        {
            Debug.LogError("´Ð³×ÀÓÀº 3ÀÚ ÀÌ»ó 12ÀÚ ÀÌÇÏ·Î ¼³Á¤ÇØ¾ß ÇÕ´Ï´Ù.");
            return;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(nickName, "^[a-zA-Z0-9°¡-ÆR]+$"))
        {
            Debug.LogError("´Ð³×ÀÓ¿¡ Æ¯¼ö ¹®ÀÚ´Â »ç¿ëÇÒ ¼ö ¾ø½À´Ï´Ù!");
            return;
        }

        CheckNicknameExists(nickName, exists =>
        {
            if (exists)
            {
                Debug.LogError("ÀÌ¹Ì Á¸ÀçÇÏ´Â ´Ð³×ÀÓÀÔ´Ï´Ù.");
            }
            else
            {
                SaveNicknameToFirestore(nickName );
            }
        });
    }

    // FireStore¿¡¼­ ´Ð³×ÀÓ Áßº¹ È®ÀÎ
    private void CheckNicknameExists(string nickName, System.Action<bool> callback)
    {
        db.Collection("USERS")      // Firestore¿¡¼­ "USERS"ÄÃ·º¼ÇÀ» ÂüÁ¶ÇÕ´Ï´Ù.
            .WhereEqualTo("NICKNAME", nickName)     // "NICKNAME" ÇÊµåÀÇ °ªÀÌ nickName°ú ¿ÏÀüÈ÷ ÀÏÄ¡ÇÏ´Â ¹®¼­¸¸ °Ë»ö WHERE NICKNAME = "Alice"
            .Limit(1)       // °Ë»ö °á°ú¿¡¼­ ÃÖ´ë 1°³ÀÇ ¹®¼­¸¸ °¡Á®¿È
            .GetSnapshotAsync()     // ºñµ¿±âÀûÀ¸·Î °á°ú¸¦ °¡Á®¿Â´Ù.
            .ContinueWithOnMainThread(task =>       // ¸ÞÀÎ½º·¹µå¿¡¼­ ÈÄ¼Ó ÀÛ¾÷À» ½ÇÇàÇÑ´Ù.
            {
                if (task.IsCompleted && task.Result.Count > 0)
                {
                    callback.Invoke(true);
                }
                else
                {
                    callback.Invoke(false);
                }
            });
    }

    private void SaveNicknameToFirestore(string nickName)
    {
        //DocumentReference userRef = db.Collection("USERS").Document(user.UserId);

        if(auth.CurrentUser == null)
        {
            Debug.LogError("·Î±×ÀÎ Á¤º¸°¡ ¾ø½À´Ï´Ù!");
            return;
        }

        string currentUserID = auth.CurrentUser.UserId;
        DocumentReference userRef = db.Collection("USERS").Document(currentUserID);

        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "NICKNAME", nickName },
            { "CREATED_AT", FieldValue.ServerTimestamp }
        };

        userRef.SetAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"´Ð³×ÀÓ ¼³Á¤ ¿Ï·á: {nickName}");
                NickName = nickName;
                SceneManager.LoadScene("Main");
            }
            else
            {
                Debug.LogError("´Ð³×ÀÓ ¼³Á¤ ½ÇÆÐ!");
            }
        });
    }
}
