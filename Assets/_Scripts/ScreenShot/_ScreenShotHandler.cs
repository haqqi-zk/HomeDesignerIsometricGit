using Evereal.VideoCapture;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Proyecto26;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

[Serializable, ExecuteInEditMode]
public class _ScreenShotHandler : CaptureBase
{
    public RawImage image2D;
    public Button ssButton;
    public Button saveButton;
    public Button browserButton;
    public Button closeImagePreviewBtn;
    public GameObject screenshotResult;

    public int height = 1080;
    public int width = 1920;
    public int screenHeight;
    public int screenWidth;

    [SerializeField]
    public ImageFormat imageFormat = ImageFormat.PNG;
    public string imageSavePath { get; protected set; }

    private byte[] imageData;
    public bool isSaved = false;
    public bool uploaded = false;
    public string uploadedName;
    private void Start()
    {
        ssButton.onClick.AddListener(StartCaptureImage);
        saveButton.onClick.AddListener(EncodeToPNG);
        browserButton.onClick.AddListener(OpenSaveFolder);

    }
    public void StartCaptureImage()
    {
        GetScreenSize();
        CreateRenderTextures();
        StartCoroutine(CaptureImage());

    }
    public void TakeScreenShot()
    {
        RenderTexture prevTexture = RenderTexture.active;

        //RenderTexture.active = null;
        RenderTexture.active = outputTexture;

        Texture2D txt = Utils.CreateTexture(screenWidth, screenHeight, null);
        // Read screen contents into the texture
        txt.ReadPixels(new Rect(0, 0, screenWidth, screenHeight), 0, 0);
        txt.Apply();
        imageData = txt.EncodeToPNG();

        RenderTexture.active = prevTexture;

        image2D.texture = txt;

        screenshotResult.SetActive(true);
        this.gameObject.SetActive(false);

        regularCamera.targetTexture = null;
    }
    private IEnumerator CaptureImage()
    {
        yield return new WaitForEndOfFrame();
        TakeScreenShot();

    }
    public void GetScreenSize()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        frameHeight = Screen.height;
        frameWidth = Screen.width;
    }
    public void EncodeToPNG()
    {
        if (!isSaved)
        {
            saveFolderFullPath = Utils.CreateFolder(saveFolder);
            string ext = imageFormat == ImageFormat.PNG ? "png" : "jpg";
            imageSavePath = string.Format("{0}image_{1}.{2}",
              saveFolderFullPath,
              Utils.GetTimeString(),
              ext);

            File.WriteAllBytes(imageSavePath, imageData);
            isSaved = true;
        }
    }
    public void ImageSaved(bool saved)
    {
        isSaved = saved;
        uploaded = saved;
        uploadedName = null;
    }
    public void OpenSaveFolder()
    {
        Utils.BrowseFolder(saveFolder);
    }
    public void ShareToSocialMedia(string mediaName)
    {
        if(mediaName == "facebook")
        {
            string url = "https://www.facebook.com/sharer.php?u=";
            PostToFirebaseStorage(url);
        }
        else if(mediaName == "twitter")
        {
            string url = "https://twitter.com/intent/tweet/?url=";
            PostToFirebaseStorage(url);
        }
        else if(mediaName == "linkedin")
        {
            string url = "https://linkedin.com/sharing/share-offsite/?url=";
            PostToFirebaseStorage(url);
        }
        else if(mediaName == "pinterest")
        {
            string url = "https://id.pinterest.com/pin/create/button/?url=";
            PostToFirebaseStorage(url);
        }
        else if(mediaName == "mail")
        {
            string url = "mailto:?subject=Let%27s%20Join%20FXMetaverse&body=Check%20out%20this:%20";
            PostToFirebaseStorage(url);
        }
        else if(mediaName == "whatsapp")
        {
            string url = "https://api.whatsapp.com/send/?text=";
            PostToFirebaseStorage(url);
        }
    }
    public void PostToFirebaseStorage(string mediaLink)
    {
        string fullName = null;
        if(uploadedName == null || uploadedName == string.Empty)
        {
            string ext = imageFormat == ImageFormat.PNG ? "png" : "jpg";
            fullName = string.Format("image_{0}.{1}",
              Utils.GetTimeString(),
              ext);
            uploadedName = fullName;
        }
        else
        {
            fullName = uploadedName;
        }
        

        var url = _DatabaseController.Instance.StorageUrl + fullName;
        if (!uploaded)
        {
            if (imageData != null)
            {
                RestClient.Post(new RequestHelper
                {
                    EnableDebug = true,
                    Uri = url,
                    ContentType = "png",
                    Method = "POST",
                    UploadHandler = new UploadHandlerRaw(imageData)
                }).Then(response =>
                {
                    if (response.StatusCode >= 200 || response.StatusCode <= 299)
                    {
                        //action after fileUpload
                        Application.OpenURL(mediaLink + url + "?alt=media");
                        uploaded = true;
                    }
                });
            }
        }
        else
        {
            Application.OpenURL(mediaLink + url + "?alt=media");
        }
        
    }
}
