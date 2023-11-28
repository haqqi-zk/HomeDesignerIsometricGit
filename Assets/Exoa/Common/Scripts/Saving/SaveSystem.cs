using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace Exoa.Designer
{
    public class SaveSystem
    {
        public enum Mode { FILE_SYSTEM, RESOURCES, ONLINE };
        public Mode mode;

        public static string defaultSubFolderName = null;
        public static string defaultFileToOpen = null;
        private string resourcesFolderLocation = "/Resources/";

        public string ResourcesFolderLocation { get => resourcesFolderLocation; set => resourcesFolderLocation = value; }

        public SaveSystem(Mode mode)
        {
            this.mode = mode;
        }



        [Serializable]
        public struct FileList
        {
            public List<string> list;
        }
        public static SaveSystem Create(Mode mode)
        {
            return new SaveSystem(mode);
        }
        public string GetBasePath(string subFolder)
        {
            string path = "";
            if (mode == Mode.RESOURCES || mode == Mode.FILE_SYSTEM)
            {
                if (mode == Mode.RESOURCES)
                    path = Application.dataPath;
                else
                    path = Application.persistentDataPath + "/";

                if (!string.IsNullOrEmpty(subFolder))
                    path += subFolder + "/";

                try
                {
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Debug.LogError("Could not create folder:" + e.Message);
                }
            }
            else
            {
                //if online
                path = _DatabaseController.Instance.GetDatabaseUrl(subFolder) + "/";
            }
            return path;
        }
        public void RefreshUnityDB()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        public T LoadFileItem<T>(string fileName, string subFolderName, Action<T> pCallback = null, string ext = ".json")
        {
            HDLogger.Log("LoadFileItem " + subFolderName + "/" + fileName, HDLogger.LogCategory.FileSystem);

            string json = LoadFileItem(fileName, subFolderName, null, ext);
            T p = JsonUtility.FromJson<T>(json);
            pCallback?.Invoke(p);

            return p;
        }
        public string LoadFileItem(string fileName, string subFolderName, Action<string> pCallback = null, string ext = ".json")
        {
            string finalContent = null;
            if (mode == Mode.RESOURCES || mode == Mode.FILE_SYSTEM)
            {
                HDLogger.Log("LoadFileItem " + GetBasePath(subFolderName) + "/" + fileName, HDLogger.LogCategory.FileSystem);

                string content = null;

                try
                {
                    if (mode == Mode.RESOURCES)
                    {
                        TextAsset o = Resources.Load<TextAsset>(subFolderName + "/" + fileName);
                        content = o != null ? o.text : null;
                    }
                    else
                    {
                        StreamReader stream = File.OpenText(GetBasePath(subFolderName) + fileName + ext);
                        content = stream.ReadToEnd();
                        stream.Close();
                    }
                }
                catch (System.Exception e)
                {
                    HDLogger.LogError("Error loading " + subFolderName + "/" + fileName + " " + e.Message, HDLogger.LogCategory.FileSystem);
                    AlertPopup.ShowAlert("error", "Error", "Error loading " + subFolderName + "/" + fileName + " " + e.Message);
                }

                if (!string.IsNullOrEmpty(content))
                    pCallback?.Invoke(content);

                finalContent = content;
            }
            else
            {
                List<DatabaseFile> fileList = _DatabaseController.Instance.GetDatabaseFileList(subFolderName);
                foreach(DatabaseFile file in fileList)
                {
                    if(file.fileName == fileName)
                    {
                        finalContent = file.jsonStr;
                        if (_DatabaseController.UseSampleScene)
                        {
                            if(file.WallColors != null || file.FloorColors != null)
                            {
                                PopupColor.Instance.SetBaseSceneColor(file.WallColors, file.FloorColors);
                                Debug.Log(file.WallColors[0]);
                            }
                            if(file.backgroundColor != null)
                            {
                                _BackgroundColorController.Instance.SetBackgroundColorData(file.backgroundColor);
                            }
                            else
                            {
                                _BackgroundColorController.Instance.SetBackgroundColorToDefault();
                            }
                        }
                        break;
                    }
                }
                pCallback?.Invoke(finalContent);
            }
            

            return finalContent;
        }

        public bool Exists(string fileName, string subFolderName, string ext = ".png")
        {
            bool exists = false;
            string path = null;
            if (mode == Mode.RESOURCES || mode == Mode.FILE_SYSTEM)
            {
                if (mode == Mode.RESOURCES)
                {
                    path = subFolderName + "/" + fileName;
                    UnityEngine.Object o = Resources.Load(path);
                    exists = o != null;
                }
                else
                {
                    path = GetBasePath(subFolderName) + fileName + ext;
                    exists = File.Exists(path);
                }
                HDLogger.Log("Exists " + path + " : " + exists, HDLogger.LogCategory.FileSystem);
            }
            else
            {
                //with the logic of all exist file in database will have their thumbnail file so ext = .png or .json doesnt matter
                if(subFolderName == "Thumbnails")
                {
                    List<DatabaseFile> fileList = _DatabaseController.Instance.GetDatabaseFileList("Interior");
                    if(fileList != null)
                    {
                        foreach (DatabaseFile file in fileList)
                        {
                            if (file.fileName == fileName)
                            {
                                exists = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    List<DatabaseFile> fileList = _DatabaseController.Instance.GetDatabaseFileList(subFolderName);
                    if(fileList != null)
                    {
                        foreach (DatabaseFile file in fileList)
                        {
                            if (file.fileName == fileName)
                            {
                                exists = true;
                                break;
                            }
                        }
                    }
                }
            }
            return exists;
        }

        public async UniTask<Texture2D> LoadTextureItem(string fileName, string subFolderName, Action callback, string ext = ".png", int width = 100, int height = 100)
        {
            Texture2D finalTex = null;
            
            if (mode == Mode.RESOURCES || mode == Mode.FILE_SYSTEM)
            {
                HDLogger.Log("LoadTextureItem " + fileName, HDLogger.LogCategory.FileSystem);
                Texture2D tex = null;

                if (mode == Mode.RESOURCES)
                {
                    tex = Resources.Load<Texture2D>(subFolderName + "/" + fileName);
                }
                else
                {
                    string path = GetBasePath(subFolderName) + fileName + ext;


                    byte[] fileData;

                    if (File.Exists(path))
                    {
                        fileData = File.ReadAllBytes(path);
                        tex = new Texture2D(width, height);
                        tex.LoadImage(fileData);

                    }
                }
                finalTex = tex;
            }
            else
            {
                finalTex = await _DatabaseController.Instance.LoadTexture(fileName, subFolderName);
            }
            
            return finalTex;

        }

        public async void SaveFileItem(string fileName, string subFolderName, string json, Action<string> pCallback = null)
        {
            
            if(mode == Mode.FILE_SYSTEM || mode == Mode.RESOURCES)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(json);
                SaveFileItem(fileName, subFolderName, bytes, pCallback);
            }
            else
            {
                string[] strings = fileName.Split('.');
                fileName = strings[0];
                List<DatabaseFile> fileList = _DatabaseController.Instance.GetDatabaseFileList(subFolderName);
                if(fileList != null)
                {
                    int fileCount = fileList.Count;
                    for (int i = 0; i < fileCount; i++)
                    {
                        if (fileList[i].fileName == fileName)
                        {
                            if (_DatabaseController.UseSampleScene)
                            {
                                fileList[i].WallColors = PopupColor.Instance.GetStringData("Wall");
                                fileList[i].FloorColors = PopupColor.Instance.GetStringData("Floor");
                                fileList[i].backgroundColor = _BackgroundColorController.Instance.GetBackgroundColorData();
                            }
                            fileList[i].jsonStr = json;
                            break;
                        }
                        else
                        {
                            if (i == fileCount - 1)
                            {
                                DatabaseFile newFile = new DatabaseFile();
                                newFile.fileName = fileName;
                                newFile.jsonStr = json;
                                if (_DatabaseController.UseSampleScene)
                                {
                                    newFile.WallColors = PopupColor.Instance.GetStringData("Wall");
                                    newFile.FloorColors = PopupColor.Instance.GetStringData("Floor");
                                    newFile.backgroundColor = _BackgroundColorController.Instance.GetBackgroundColorData();
                                }
                                fileList.Add(newFile);
                            }
                        }
                    }
                }
                else
                {
                    DatabaseFile newFile = new DatabaseFile();
                    newFile.fileName = fileName;
                    newFile.jsonStr = json;
                    if (_DatabaseController.UseSampleScene)
                    {
                        newFile.WallColors = PopupColor.Instance.GetStringData("Wall");
                        newFile.FloorColors = PopupColor.Instance.GetStringData("Floor");
                        newFile.backgroundColor = _BackgroundColorController.Instance.GetBackgroundColorData();
                    }
                    fileList = new List<DatabaseFile>();
                    fileList.Add(newFile);
                }
                //refresh database list
                //_DatabaseController.Instance.DeleteData(subFolderName);
                _DatabaseController.Instance.SaveListToRealtimeDatabase(subFolderName, fileList);
                await Task.Delay(2000);
                await _DatabaseController.Instance.WaitFileList();
                pCallback?.Invoke(fileName);
            }
        }
        public void SaveFileItem(string fileName, string subFolderName, byte[] bytes, Action<string> pCallback = null)
        {
            
            if (mode == Mode.RESOURCES || mode == Mode.FILE_SYSTEM)
            {
                if (bytes == null)
                {
                    HDLogger.LogWarning("Warning saving " + subFolderName + "/" + fileName + ", nothing to save (empty content)", HDLogger.LogCategory.FileSystem);

                    return;
                }
                HDLogger.Log("SaveFileItem " + GetBasePath(subFolderName) + fileName, HDLogger.LogCategory.FileSystem);

                bool success = false;

                try
                {
                    File.WriteAllBytes(GetBasePath(subFolderName) + fileName, bytes);

                    if (mode == Mode.RESOURCES)
                    {
                        RefreshUnityDB();
                    }
                    success = true;
                }
                catch (System.Exception e)
                {
                    HDLogger.LogError("Error saving " + subFolderName + "/" + fileName + " " + e.Message, HDLogger.LogCategory.FileSystem);
                    AlertPopup.ShowAlert("error", "Error", "Error loading " + subFolderName + "/" + fileName + " " + e.Message);
                }

                if (success)
                    pCallback?.Invoke(fileName);
            }
            else
            {
                bool success = false;
                if (bytes == null)
                {
                    Debug.Log("nothing to save");

                    return;
                }
                if(subFolderName == "Thumbnails")
                {
                    _DatabaseController.Instance.PostToFirebaseStorage(fileName, bytes);
                }
               
                success = true;
                if (success)
                    pCallback?.Invoke(fileName);
            }
            

        }

        public async void DeleteFileItem(string fileName, string subFolderName, Action pCallback = null, string ext = ".json")
        {
            if (mode == Mode.RESOURCES || mode == Mode.FILE_SYSTEM)
            {
                HDLogger.Log("DeleteFileItem " + fileName, HDLogger.LogCategory.FileSystem);

                try
                {
                    FileInfo fi = new FileInfo(GetBasePath(subFolderName) + fileName + ext);
                    fi.Delete();

                    pCallback?.Invoke();
                }
                catch (System.Exception e)
                {
                    HDLogger.LogError("Error deleting " + subFolderName + "/" + fileName + " " + e.Message, HDLogger.LogCategory.FileSystem);
                    AlertPopup.ShowAlert("error", "Error", e.Message);
                }
                RefreshUnityDB();
            }
            else
            {
                List<DatabaseFile> fileList = _DatabaseController.Instance.GetDatabaseFileList(subFolderName);
                int fileCount = fileList.Count;

                for(int i = 0; i < fileCount; i++)
                {
                    if (fileList[i].fileName == fileName)
                    {
                        fileList.Remove(fileList[i]);
                        break;
                    }
                }
                _DatabaseController.Instance.DeleteFromFirebaseStorage(fileName, subFolderName);
                _DatabaseController.Instance.SaveListToRealtimeDatabase(subFolderName, fileList);
                await Task.Delay(2000);
                await _DatabaseController.Instance.WaitFileList();
                pCallback?.Invoke();
            }
        }

        public async void RenameFileItem(string fileName, string newName, string subFolderName, Action pCallback = null, string ext = ".json")
        {
            if (mode == Mode.RESOURCES || mode == Mode.FILE_SYSTEM)
            {
                HDLogger.Log("RenameFileItem " + fileName + " " + newName, HDLogger.LogCategory.FileSystem);

                FileInfo fi = null;
                try
                {
                    fi = new FileInfo(GetBasePath(subFolderName) + fileName + ext);
                    fi.MoveTo(GetBasePath(subFolderName) + newName + ext);

                    pCallback?.Invoke();
                }
                catch (System.Exception e)
                {
                    AlertPopup.ShowAlert("error", "Error", e.Message);
                    HDLogger.LogError("Error renaming " + subFolderName + "/" + fileName + " " + e.Message, HDLogger.LogCategory.FileSystem);
                }
                RefreshUnityDB();
            }
            else
            {
                if(subFolderName != "Thumbnails")
                {
                    List<DatabaseFile> fileList = _DatabaseController.Instance.GetDatabaseFileList(subFolderName);
                    int fileCount = fileList.Count;
                    for (int i = 0; i < fileCount; i++)
                    {
                        if (fileList[i].fileName == fileName)
                        {
                            fileList[i].fileName = newName; break;
                        }
                    }
                    Texture2D tex = await _DatabaseController.Instance.LoadTexture2(fileName, subFolderName);
                    //Debug.Log("old file name " + fileName + subFolderName);
                    //Debug.Log("new file naem " + newName + subFolderName);
                    //delete old file
                    if (tex != null)
                    {
                        _DatabaseController.Instance.DeleteFromFirebaseStorage(fileName, subFolderName);
                        byte[] textureData = tex.EncodeToPNG();
                        _DatabaseController.Instance.PostToFirebaseStorage(newName, textureData, subFolderName);
                    }
                    _DatabaseController.Instance.SaveListToRealtimeDatabase(subFolderName, fileList);
                    await Task.Delay(2000);
                    await _DatabaseController.Instance.WaitFileList();
                    pCallback?.Invoke();
                }
                else
                {
                    
                }
            }
        }

        public void CopyFileItem(string fileName, string newName, string subFolderName, Action pCallback = null, string ext = ".json")
        {
            if (mode == Mode.RESOURCES || mode == Mode.FILE_SYSTEM)
            {
                HDLogger.Log("CopyFileItem " + fileName + " " + newName, HDLogger.LogCategory.FileSystem);

                FileInfo fi = null;
                try
                {
                    fi = new FileInfo(GetBasePath(subFolderName) + fileName + ext);
                    fi.CopyTo(GetBasePath(subFolderName) + newName + ext);

                    pCallback?.Invoke();
                }
                catch (System.Exception e)
                {
                    AlertPopup.ShowAlert("error", "Error", e.Message);
                    HDLogger.LogError("Error copying " + subFolderName + "/" + fileName + " " + e.Message, HDLogger.LogCategory.FileSystem);
                }
                RefreshUnityDB();
            }
            else
            {

            }
        }


        public FileList ListFileItems(string subFolderName, Action<FileList> pCallback = null, string ext = "*.json")
        {
            FileList ll = new FileList();
            ll.list = new List<string>();
            if (mode == Mode.RESOURCES || mode == Mode.FILE_SYSTEM)
            {
                if (mode == Mode.RESOURCES)
                {
                    UnityEngine.Object[] files = Resources.LoadAll(subFolderName + "/");
                    foreach (UnityEngine.Object o in files)
                    {
                        ll.list.Add(o.name);
                    }
                }
                else
                {
                    DirectoryInfo dir = new DirectoryInfo(GetBasePath(subFolderName));
                    FileInfo[] info = dir.GetFiles(ext);
                    foreach (FileInfo f in info)
                    {
                        ll.list.Add(f.Name);
                    }
                }
                HDLogger.Log("ListFileItems " + GetBasePath(subFolderName) + ":" + ll.list.Count, HDLogger.LogCategory.FileSystem);

                pCallback?.Invoke(ll);
            }
            else
            {
                List<DatabaseFile> fileList = _DatabaseController.Instance.GetDatabaseFileList(subFolderName);
                if(fileList != null)
                {
                    foreach (DatabaseFile file in fileList)
                    {
                        ll.list.Add(file.fileName);
                    }
                }
                pCallback?.Invoke(ll);
            }
            
            return ll;
        }

    }
}
