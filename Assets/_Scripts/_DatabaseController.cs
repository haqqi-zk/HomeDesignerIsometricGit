using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using FullSerializer;
using Newtonsoft.Json;
using ProceduralToolkit;
using Proyecto26;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class _DatabaseController : MonoBehaviour
{
    public static _DatabaseController Instance;
    private const string PROJECT_NAME = "HomeDesigner";
    private const string NO_LOGIN_PROJECT_NAME = "NoLoginHomeDesigner";
    private const string FIREBASE_AUTH_KEY = "AIzaSyAt13J8WMnrYYDO3Rp5RPPtS16N47b6dGs";

    //AIzaSyAPtHvJVHrYauHWdT9JqeUIETpGT3OjvLY
    [SerializeField] string _databaseUrl;
    [SerializeField] string _storageUrl;
    public string StorageUrl;
    public string imageScreenshotDatabase;
    public static string DatabaseUrl;
    public static string PlayerNameDatabase;

    private const string FallbackPlayerName = "Player_Fallback42069";
    private static bool _isShouldRunning = true;
    private static string _idToken;
    private static string _localId;
    private static string _refreshToken;
    private static string _expiresIn;

    public static bool connectionState;
    public bool isOnline;
    //[HideInInspector]
    public List<DatabaseFile> interiorList = new List<DatabaseFile>();
    //[HideInInspector]
    public List<DatabaseFile> floorMapsList = new List<DatabaseFile>();
    public string PlayerName { get { return PlayerNameDatabase; } }

    public static fsSerializer serializer = new fsSerializer();

    public static bool UseSampleScene;
    public static GameObject SampleScenePrefab;

    public GameObject sampleScenePrefab;
    public bool useSampleScene = true;

    public static bool IsLogin;
    public bool isLogin = true;

    public string playerEmailBackup;
    public string playerPasswordBackup;
    static _DatabaseController()
    {
        Application.quitting += () =>
        {
            Debug.Log("Application is quitting");

            _isShouldRunning = false;
        };
        Application.unloading += () =>
        {
            Debug.Log("Application is unloading");

            _isShouldRunning = false;
        };
    }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        connectionState = isOnline;
        DatabaseUrl = _databaseUrl;
        StorageUrl = _storageUrl;

        UseSampleScene = useSampleScene;
        SampleScenePrefab = sampleScenePrefab;

        IsLogin = isLogin;
    }
    public static void DebugError(string msg)
    {
        Debug.Log(msg);
    }

    #region HQ Methods
    public async UniTask<bool> WaitFileList()
    {
        floorMapsList = await ListFileItems("FloorMaps");
        interiorList = await ListFileItems("Interior");

        return true;
    }
    public List<DatabaseFile> GetDatabaseFileList(string subFolderName)
    {
        List<DatabaseFile> fileList = new List<DatabaseFile>();
        if (subFolderName == "Interior")
        {
            fileList = interiorList;
        }
        else if (subFolderName == "FloorMaps")
        {
            fileList = floorMapsList;
        }
        return fileList;
    }
    public string GetDatabaseUrl(string subFolderName)
    {        
        if (IsLogin)
        {
            return $"{_databaseUrl}{PROJECT_NAME}/{PlayerNameDatabase}/{subFolderName}";
        }
        else
        {
            return $"{_databaseUrl}{NO_LOGIN_PROJECT_NAME}/{subFolderName}";
        }
    }
    public string GetStorageUrl()
    {
        return _storageUrl;
    }
    public async UniTask<string> GetJsonStrData(string subFolderName, string fileName)
    {
        DatabaseFile data = new DatabaseFile();
        await _DatabaseController.Instance.LoadData(subFolderName + "/").ContinueWith(res =>
        {
            data = JsonConvert.DeserializeObject<DatabaseFile>(res);
        });

        return data.jsonStr;
    }
    public void PostToFirebaseStorage(string thumbnailName, byte[] bytes = null, string subFolderName = null)
    {
        string fullName = null;
        if (thumbnailName.Contains("_persp.png"))
        {
            if (IsLogin)
            {
                fullName = $"{PlayerNameDatabase}_{thumbnailName}";
            }
            else
            {
                fullName = $"{thumbnailName}";
            }
            if (subFolderName != null)
            {
                fullName = $"{subFolderName}_{thumbnailName}";
            }
        }
        else
        {
            if (IsLogin)
            {
                fullName = $"{PlayerNameDatabase}_{thumbnailName}_persp.png";
            }
            else
            {
                fullName = $"{thumbnailName}_persp.png";
            }
            if (subFolderName != null)
            {
                fullName = $"{subFolderName}_{thumbnailName}_persp.png";
            }
        }

        var url = _storageUrl + fullName;
        if (bytes != null)
        {
            RestClient.Post(new RequestHelper
            {
                EnableDebug = true,
                Uri = url,
                ContentType = "png",
                Method = "POST",
                UploadHandler = new UploadHandlerRaw(bytes)
            }).Then(response =>
            {
                if (response.StatusCode >= 200 || response.StatusCode <= 299)
                {
                    //action after fileUpload

                }
            });
        }
    }
    public void SaveListToRealtimeDatabase(string folderName, List<DatabaseFile> databaseFiles)
    {
        SaveDataArray(folderName, databaseFiles);
    }
    public bool CheckFirebaseStorage(string fileName, string subFolderName, string ext = ".png")
    {
        bool exist = true;
        string url = null;
        if (fileName.Contains(".png"))
        {
            if (IsLogin)
            {
                url = $"{_storageUrl}{PlayerNameDatabase}_{subFolderName}_{fileName}?alt=media";
            }
            else
            {
                url = $"{_storageUrl}{subFolderName}_{fileName}?alt=media";
            }
        }
        else
        {
            if (IsLogin)
            {
                url = $"{_storageUrl}{PlayerNameDatabase}_{subFolderName}_{fileName}{ext}?alt=media";
            }
            else
            {
                url = $"{_storageUrl}{subFolderName}_{fileName}{ext}?alt=media";
            }
        }

        WebRequest webRequest = WebRequest.Create(url);
        webRequest.Timeout = 1200; // miliseconds
        webRequest.Method = "HEAD";

        try
        {
            webRequest.GetResponse();
        }
        catch
        {
            exist = false;
        }

        return exist;
    }
    public async UniTask<Texture2D> LoadTexture(string fileName, string subFolderName)
    {
        Texture2D finalTexture = null;
        string url = null;
        if (fileName.Contains(".png"))
        {
            if (IsLogin)
            {
                url = GetStorageUrl() + $"{PlayerNameDatabase}_{fileName}?alt=media";
            }
            else
            {
                url = GetStorageUrl() + $"{fileName}?alt=media";
            }
        }
        else
        {
            if (IsLogin)
            {
                url = GetStorageUrl() + $"{PlayerNameDatabase}_{fileName}.png?alt=media";
            }
            else
            {
                url = GetStorageUrl() + $"{fileName}.png?alt=media";
            }
        }
        var request = UnityWebRequestTexture.GetTexture(url);
        try
        {
            await request.SendWebRequest();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        while (!request.isDone) await Task.Yield();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
            return null;
        }
        finalTexture = DownloadHandlerTexture.GetContent(request);
        return finalTexture;
    }

    public async UniTask<Texture2D> LoadTexture2(string fileName, string subFolderName)
    {
        Texture2D finalTexture = null;
        string url = null;
        if (fileName.Contains("_persp.png"))
        {
            if (IsLogin)
            {
                url = GetStorageUrl() + $"{PlayerNameDatabase}_{fileName}?alt=media";
            }
            else
            {
                url = GetStorageUrl() + $"{fileName}?alt=media";
            }
        }
        else
        {
            if (IsLogin)
            {
                url = GetStorageUrl() + $"{PlayerNameDatabase}_{fileName}_persp.png?alt=media";
            }
            else
            {
                url = GetStorageUrl() + $"{fileName}_persp.png?alt=media";
            }
        }
        var request = UnityWebRequestTexture.GetTexture(url);
        try
        {
            await request.SendWebRequest();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        while (!request.isDone) await Task.Yield();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
            return null;
        }
        finalTexture = DownloadHandlerTexture.GetContent(request);
        return finalTexture;
    }
    public void DeleteFromFirebaseStorage(string fileName, string subFolderName)
    {
        string url = null;
        if (fileName.Contains("_persp.png"))
        {
            if (IsLogin)
            {
                url = $"{_storageUrl}{PlayerNameDatabase}_{subFolderName}_{fileName}";
            }
            else
            {
                url = $"{_storageUrl}{subFolderName}_{fileName}";
            }
        }
        else
        {
            if (IsLogin)
            {
                url = $"{_storageUrl}{PlayerNameDatabase}_{subFolderName}_{fileName}_persp.png";
            }
            else
            {
                url = $"{_storageUrl}{subFolderName}_{fileName}_persp.png";
            }
        }
        RestClient.Delete(url).Then(response =>
        {
            if (response.StatusCode >= 200 || response.StatusCode <= 299)
            {
                Debug.Log("Delete Storage Completed");
            }
        });
    }
    public async UniTask<List<DatabaseFile>> ListFileItems(string subFolderName)
    {
        List<DatabaseFile> files = new List<DatabaseFile>();
        bool done = false;

        await LoadData(subFolderName).ContinueWith(res =>
        {
            files = JsonConvert.DeserializeObject<List<DatabaseFile>>(res);
            done = true;
        });
        await UniTask.WaitUntil(() => done == true);

        return files;
    }
    #endregion

    public void SaveDataArray<T>(string key, List<T> data)
    {
        if (!_isShouldRunning) return;

        if (IsLogin)
        {
            RestClient.Put<T>($"{_databaseUrl}{PROJECT_NAME}/{PlayerNameDatabase}/{key}.json?auth={_idToken}",
                JsonConvert.SerializeObject(data)).Then(res =>
                {

                }).Catch(err =>
                {
                    var error = err as RequestException;
                    if (error.Response.Contains("TOKEN_EXPIRED"))
                    {
                        RefreshTokenId();
                        SaveDataArray(key, data);
                    }
                });
        }
        else
        {
            RestClient.Put<T>($"{_databaseUrl}{NO_LOGIN_PROJECT_NAME}/{key}.json?auth={_idToken}",
                JsonConvert.SerializeObject(data)).Then(res =>
                {

                }).Catch(err =>
                {
                    var error = err as RequestException;
                    if (error.Response.Contains("TOKEN_EXPIRED"))
                    {
                        RefreshTokenId();
                        SaveDataArray(key, data);
                    }
                });
        }
        
    }

    public static void SaveData<T, TT>(Dictionary<T, TT> data)
    {
        if (!_isShouldRunning) return;

        if (IsLogin)
        {
            foreach (var key in data.Keys)
            {
                RestClient.Put<T>($"{DatabaseUrl}{PROJECT_NAME}/{PlayerNameDatabase}/{key}.json?auth={_idToken}",
                    JsonConvert.SerializeObject(data[key].ToString()));
            }
        }
        else
        {
            foreach (var key in data.Keys)
            {
                RestClient.Put<T>($"{DatabaseUrl}{NO_LOGIN_PROJECT_NAME}/{key}.json?auth={_idToken}",
                    JsonConvert.SerializeObject(data[key].ToString()));
            }
        }
    }

    public UniTask<string> LoadData(string key)
    {
        if (!_isShouldRunning) return UniTask.FromResult(string.Empty);

        if (IsLogin)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            var requestHelper = new RequestHelper()
            {
                Uri = $"{_databaseUrl}{PROJECT_NAME}/{PlayerNameDatabase}/{key}.json?auth={_idToken}",
            };

            RestClient.Get(requestHelper).Then(res =>
            {
                taskCompletionSource.SetResult(res.Text);
            }).Catch(err => {
                taskCompletionSource.SetResult(string.Empty);
            });

            return taskCompletionSource.Task.AsUniTask();
        }
        else
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            var requestHelper = new RequestHelper()
            {
                Uri = $"{_databaseUrl}{NO_LOGIN_PROJECT_NAME}/{key}.json?auth={_idToken}",
            };

            RestClient.Get(requestHelper).Then(res =>
            {
                taskCompletionSource.SetResult(res.Text);
            }).Catch(err => {
                taskCompletionSource.SetResult(string.Empty);
            });

            return taskCompletionSource.Task.AsUniTask();
        }
    }
    public static UniTask<string> LoadPlayerName()
    {
        if (!_isShouldRunning)
            return UniTask.FromResult(string.Empty);

        var taskCompletionSource = new TaskCompletionSource<string>();

        RestClient.Get($"{DatabaseUrl}{PROJECT_NAME}.json?auth={_idToken}").Then(res =>
        {
            fsData userData = fsJsonParser.Parse(res.Text);
            Dictionary<string, UserData> users = null;
            serializer.TryDeserialize(userData, ref users);
            foreach (var user in users.Values)
            {
                if (user.LocalId == _localId)
                {
                    PlayerNameDatabase = user.Username; break;
                }
            }

            taskCompletionSource.SetResult(res.Text);
        }).Catch(err =>
        {
            var error = err as RequestException;
            Debug.Log(error.Response);
        });

        return taskCompletionSource.Task.AsUniTask();
    }
    public async UniTask<bool> CheckPlayerExist(string username)
    {
        bool exist = false;

        Dictionary<string, UserData> users = null;
        RestClient.Get($"{DatabaseUrl}{PROJECT_NAME}.json?auth={_idToken}").Then(res =>
        {
            fsData userData = fsJsonParser.Parse(res.Text);
            serializer.TryDeserialize(userData, ref users);
        });
        await UniTask.WaitUntil(() => users != null);

        foreach (var user in users.Values)
        {
            if (user.Username == username)
            {
                exist = true;
            }
        }

        return exist;
    }
    public void DeleteData(string key)
    {
        if (!_isShouldRunning) return;

        if (IsLogin)
        {
            RestClient.Delete($"{_databaseUrl}{PROJECT_NAME}/{PlayerNameDatabase}/{key}.json?auth={_idToken}");
        }
        else
        {
            RestClient.Delete($"{_databaseUrl}{NO_LOGIN_PROJECT_NAME}/{key}.json?auth={_idToken}");
        }
        
    }

    public void SaveDataArrayGlobal<T>(string key, List<T> data)
    {
        if (!_isShouldRunning) return;

        if (IsLogin)
        {
            RestClient.Put<T>($"{_databaseUrl}{PROJECT_NAME}{key}.json?auth={_idToken}",
                JsonConvert.SerializeObject(data));
        }
        else
        {
            RestClient.Put<T>($"{_databaseUrl}{NO_LOGIN_PROJECT_NAME}{key}.json?auth={_idToken}",
                JsonConvert.SerializeObject(data));
        }
    }

    public void SaveDataGlobal<T, TT>(Dictionary<T, TT> data)
    {
        if (!_isShouldRunning) return;

        if (IsLogin)
        {
            foreach (var key in data.Keys)
            {
                RestClient.Put<T>($"{_databaseUrl}{PROJECT_NAME}{key}.json?auth={_idToken}",
                    JsonConvert.SerializeObject(data));
            }
        }
        else
        {
            foreach (var key in data.Keys)
            {
                RestClient.Put<T>($"{_databaseUrl}{NO_LOGIN_PROJECT_NAME}{key}.json?auth={_idToken}",
                    JsonConvert.SerializeObject(data));
            }
        }
    }

    public UniTask<string> LoadDataGlobal(string key)
    {
        if (!_isShouldRunning) return UniTask.FromResult(string.Empty);

        if (IsLogin)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            var requestHelper = new RequestHelper()
            {
                Uri = $"{_databaseUrl}{PROJECT_NAME}{key}.json?auth={_idToken}",
            };

            RestClient.Get(requestHelper).Then(res =>
            {
                taskCompletionSource.SetResult(res.Text);
            }).Catch(err => {
                taskCompletionSource.SetResult(string.Empty);
            });

            return taskCompletionSource.Task.AsUniTask();
        }
        else
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            var requestHelper = new RequestHelper()
            {
                Uri = $"{_databaseUrl}{NO_LOGIN_PROJECT_NAME}{key}.json?auth={_idToken}",
            };

            RestClient.Get(requestHelper).Then(res =>
            {
                taskCompletionSource.SetResult(res.Text);
            }).Catch(err => {
                taskCompletionSource.SetResult(string.Empty);
            });

            return taskCompletionSource.Task.AsUniTask();
        }
    }
    public static UniTask<SignResponse> SignUpUser(string username, string email, string password)
    {
        if (!_isShouldRunning)
            return UniTask.FromResult(new SignResponse());

        var taskCompletionSource = new TaskCompletionSource<SignResponse>();

        var userData = new
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        RequestHelper requestHelper = new RequestHelper
        {
            Uri = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FIREBASE_AUTH_KEY}",
            BodyString = JsonConvert.SerializeObject(userData),
            EnableDebug = true,
            ContentType = "application/json",
        };
        PlayerNameDatabase = username;
        RestClient.Post<SignResponse>(requestHelper).Then(helper =>
        {

            _idToken = helper.idToken;
            _localId = helper.localId;
            _refreshToken = helper.refreshToken;
            _expiresIn = helper.expiresIn;

            SaveData(new Dictionary<string, string>
            {
                    {"Username", username},
                });
            SaveData(new Dictionary<string, string>
            {
                {"LocalId", _localId },
            });
            taskCompletionSource.SetResult(helper);
        }).
        Catch(ex =>
        {
            var error = ex as RequestException;

            if (error.Response.Contains("EMAIL_EXISTS"))
            {
                GameObject.FindObjectOfType<_LoginController>().PopupErrorMessage("Email already existed!");
            }
            else if (error.Response.Contains("INVALID_EMAIL"))
            {
                GameObject.FindObjectOfType<_LoginController>().PopupErrorMessage("Please correct your email address \n example: username@mail.com");
            }
            else if (error.Response.Contains("WEAK_PASSWORD"))
            {
                GameObject.FindObjectOfType<_LoginController>().PopupErrorMessage("Password can't be less than 6 characters!");
            }
        });

        return taskCompletionSource.Task.AsUniTask();
    }

    public static UniTask<SignResponse> SignUpAnonymous()
    {
        if (!_isShouldRunning)
            return UniTask.FromResult(new SignResponse());

        var taskCompletionSource = new TaskCompletionSource<SignResponse>();

        var userData = new
        {
            returnSecureToken = true
        };

        var requestHelper = new RequestHelper
        {
            Uri = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FIREBASE_AUTH_KEY}",
            BodyString = JsonConvert.SerializeObject(userData),
            EnableDebug = true,
            ContentType = "application/json",
        };
        string newRandomName = "Player_" + UnityEngine.Random.Range(0, 10000).ToString();
        PlayerNameDatabase = newRandomName;
        RestClient.Post<SignResponse>(requestHelper).Then(helper =>
        {
            _idToken = helper.idToken;
            _localId = helper.localId;
            _refreshToken = helper.refreshToken;
            _expiresIn = helper.expiresIn;

            SaveData(new Dictionary<string, string>
            {
                    {"Username", newRandomName},
                });
            SaveData(new Dictionary<string, string>
            {
                {"LocalId", _localId },
            });
            taskCompletionSource.SetResult(helper);
        }).
        Catch(ex =>
        {
            var error = ex as RequestException;

            if (error.Response.Contains("EMAIL_EXISTS"))
            {
                GameObject.FindObjectOfType<_LoginController>().PopupErrorMessage("Email already existed!");
            }
            else if (error.Response.Contains("INVALID_EMAIL"))
            {
                GameObject.FindObjectOfType<_LoginController>().PopupErrorMessage("Please correct your email address \n example: username@mail.com");
            }
            else if (error.Response.Contains("WEAK_PASSWORD"))
            {
                GameObject.FindObjectOfType<_LoginController>().PopupErrorMessage("Password can't be less than 6 characters!");
            }
        });

        return taskCompletionSource.Task.AsUniTask();
    }
    public static UniTask<SignResponse> SignInUser(string email, string password)
    {
        if (!_isShouldRunning)
            return UniTask.FromResult(new SignResponse());

        var taskCompletionSource = new TaskCompletionSource<SignResponse>();

        var userData = new
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        var requestHelper = new RequestHelper
        {
            Uri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FIREBASE_AUTH_KEY}",
            BodyString = JsonConvert.SerializeObject(userData),
            EnableDebug = true,
            ContentType = "application/json",
        };

        RestClient.Post<SignResponse>(requestHelper).Then(helper =>
        {

            _idToken = helper.idToken;
            _localId = helper.localId;
            _refreshToken = helper.refreshToken;
            _expiresIn = helper.expiresIn;

            LoadPlayerName();

            taskCompletionSource.SetResult(helper);
        }).Catch(ex =>
        {
            var error = ex as RequestException;
            Debug.Log(error.Response);
            if (error.Response.Contains("INVALID_EMAIL"))
            {
                GameObject.FindObjectOfType<_LoginController>().PopupErrorMessage("Incorrect email address!");
            }
            else if (error.Response.Contains("EMAIL_NOT_FOUND"))
            {
                GameObject.FindObjectOfType<_LoginController>().PopupErrorMessage("This email is not registered yet!");
            }
            else if (error.Response.Contains("INVALID_PASSWORD"))
            {
                GameObject.FindObjectOfType<_LoginController>().PopupErrorMessage("Incorrect password!");
            }
        });


        return taskCompletionSource.Task.AsUniTask();
    }

    public static UniTask<RefreshTokenResponse> RefreshTokenId()
    {
        if (!_isShouldRunning)
            return UniTask.FromResult(new RefreshTokenResponse());

        var taskCompletionSource = new TaskCompletionSource<RefreshTokenResponse>();

        var refreshData = new
        {
            grant_type = "refresh_token",
            refresh_token = _refreshToken
        };

        var requestHelper = new RequestHelper
        {
            Uri = $"https://securetoken.googleapis.com/v1/token?key={FIREBASE_AUTH_KEY}",
            BodyString = JsonConvert.SerializeObject(refreshData),
            EnableDebug = true,
            ContentType = "application/json",
        };

        RestClient.Post<RefreshTokenResponse>(requestHelper).Then(helper =>
        {
            _idToken = helper.id_token;
            _refreshToken = helper.refresh_token;
            _expiresIn = helper.expires_in;

            taskCompletionSource.SetResult(helper);
        });

        return taskCompletionSource.Task.AsUniTask();
    }
}

[System.Serializable]
public class DatabaseFile
{
    public string fileName;
    public string jsonStr;
    public List<string> WallColors;
    public List<string> FloorColors;
    public string backgroundColor;
}
[Serializable]
public class SignResponse
{
    public string idToken;
    public string localId;
    public string refreshToken;
    public string expiresIn;
}
[Serializable]
public class RefreshTokenResponse
{
    public string id_token;
    public string refresh_token;
    public string expires_in;
}
[Serializable]
public class UserData
{
    public DatabaseFile FloorMaps;
    public DatabaseFile Interior;
    public string LocalId;
    public string Username;
}
[Serializable]
public class ImageScreenshotData
{
    public string title;
    public string upload_json;
    public string upload_image;
    public string upload_image_extension;
}
