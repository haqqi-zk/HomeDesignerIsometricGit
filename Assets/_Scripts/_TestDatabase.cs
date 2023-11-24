using Exoa.Json;
using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System.Net;
using Unity.VisualScripting;
using UnityEngine.UI;
using Exoa.Designer;
using System.Threading.Tasks;

public class _TestDatabase : MonoBehaviour
{
    private string _idToken;
    public Texture2D _texture;
    public RawImage _image;
    public string username;
    public void TakeAction()
    {

        var taskCompletionSource = new TaskCompletionSource<string>();

        var requestHelper = new RequestHelper()
        {
            Uri = $"{_DatabaseController.DatabaseUrl}HomeDesigner/{username}/Username.json?auth={_idToken}",
        };
        Debug.Log(requestHelper.Uri);
        RestClient.Get(requestHelper).Then(res =>
        {
            taskCompletionSource.SetResult(res.Text);
            Debug.Log("success " + res.Text);
        }).Catch(err => {
            taskCompletionSource.SetResult(string.Empty);
            Debug.Log("fail " + err.Message);
        });

    }
    public void UploadImage()
    {
        Upload("ww", "Floormap");
        Upload("1", "Interior");
        Upload("wewew", "Interior");
        byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + "/Thumbnails/" + "Interior" + "_" + "wawaw" + "_persp.png");

        _DatabaseController.Instance.PostToFirebaseStorage("Tester", bytes);
    }
    public void Upload(string fileName, string subFolderName)
    {
        byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + "/Thumbnails/" + subFolderName + "_" + fileName + "_persp.png");

        _DatabaseController.Instance.PostToFirebaseStorage(fileName, bytes);
    }
    public async void SetTexture()
    {
        //_DatabaseController.Instance.LoadTexture2("Tester", "Interior", _image);
        //_image.texture = await _DatabaseController.Instance.LoadTexture("Tester", "Interior");

        _image.texture = await SaveSystem.Create(SaveSystem.Mode.ONLINE).LoadTextureItem("Tester", "Interior", null, ".png");
    }
    public void TestLink()
    {
        Application.OpenURL("https://www.linkedin.com/sharing/share-offsite/?url=https://mvp.fxwebapps.com/upload/screenshots/655ee28de752be1fd09d1170/655ee961e752be1fd09d117c_20231123.png");
    }
    public async void DownloadData()
    {
        int fileIndex = 0;
        DatabaseFile data = new DatabaseFile();
        await _DatabaseController.Instance.LoadData("Interior/" + fileIndex.ToString()).ContinueWith(res =>
        {
            data = JsonConvert.DeserializeObject<DatabaseFile>(res);
        });

        if (data == null) Debug.Log("null");
        else Debug.Log(data.fileName + " HQ " + data.jsonStr);

        List<DatabaseFile> files = new List<DatabaseFile>();
        await _DatabaseController.Instance.LoadData("Interior").ContinueWith(res =>
        {
            files = JsonConvert.DeserializeObject<List<DatabaseFile>>(res);
        });
        foreach (DatabaseFile file in files)
        {
            Debug.Log("this" + file.fileName);
        }
    }
    public async void DownloadData2()
    {
        List<string> fileList = new List<string>();
        List<DatabaseFile> databaseFiles = new List<DatabaseFile>();
        databaseFiles = await _DatabaseController.Instance.ListFileItems("Interior");
        foreach(string file in fileList)
        {
            Debug.Log("my file " + file);
        }
    }
    public void CheckFile(string name)
    {
        bool exist = CheckFirebaseStorage(name, "Interior");
        if (exist) Debug.Log("exist");
        else Debug.Log("not exist");
    }
    public bool CheckFirebaseStorage(string fileName, string subFolderName, string ext = ".png")
    {
        bool exist = true;

        string url = _DatabaseController.Instance.GetStorageUrl() + subFolderName + "_" + fileName + "_persp" + ext;

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
    public void CheckString()
    {
        //if ({"check" "me"} == { "check""me"}){

        //}
    }
    public async void DownloadWithWebRequest()
    {
        string videoUrl = null;
        string videoName = null;
        
        string url = videoUrl + "?alt=media";
        string vidSavePath = Application.dataPath + "/ReplayStorage/";
        vidSavePath = Path.Combine(vidSavePath, videoName + ".replay");

        if (!Directory.Exists(Path.GetDirectoryName(vidSavePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(vidSavePath));
        }

        var uwr = new UnityWebRequest(url);
        uwr.method = UnityWebRequest.kHttpVerbGET;
        var dh = new DownloadHandlerFile(vidSavePath);
        dh.removeFileOnAbort = true;
        uwr.downloadHandler = dh;
        await uwr.SendWebRequest();

        await UniTask.Delay(2000);
        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            Debug.Log("download saved to: " + vidSavePath.Replace("/", "\\") + "\r\n" + uwr.error);
            string vid = vidSavePath;
            while (!uwr.isDone) return;
            uwr.Dispose();
        }
    }
}
