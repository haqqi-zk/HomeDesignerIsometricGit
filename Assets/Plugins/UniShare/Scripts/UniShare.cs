using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if NETFX_CORE
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Foundation;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime;
using Windows;
using System;
#endif

public delegate void didScreenShotReady(Texture2D screenshotTexture2D);

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class UniShare: MonoBehaviour  {

	private Texture2D createdTexture;
	private bool instantShare = false; 
	private string screenshotPath;

	public string VK_ID="";
	public Image SharePopupImage;
	public GameObject SharePopupWindow;
	public GameObject SharePopupWatermark;
	public string ScreenshotName="screenshot.png";
	public string ShareText="UniShare just works! #unishare";

    public List<GameObject> hideGameObjectsWhileScreenshot;

    public event didScreenShotReady OnScreenShotReady;

	[Header("Captured Area Bounds")]
	[Space(10)]
	public bool simulateCapturedArea = false;
	[Range(0,1)]
	public float BorderTop=0.1f;
	[Range(0,1)]
	public float BorderBottom=0.1f;
	[Range(0,1)]
	public float BorderLeft=0.1f;
	[Range(0,1)]
	public float BorderRight=0.1f;

	#if UNITY_IPHONE
	[DllImport ("__Internal")]	
	private static extern void presentActivitySheetWithImageAndString(string message,byte[] imgData,int _length);

	[DllImport ("__Internal")]	
	private static extern void pluginInit(string vkid);
	#endif

    void Start()
	{
        DontDestroyOnLoad(this);
		if(SharePopupWatermark)SharePopupWatermark.SetActive(false);

		#if UNITY_IPHONE
		if(!Application.isEditor)
		{
			pluginInit(VK_ID);
		}
		#endif

		#if UNITY_ANDROID
		AndroidJNIHelper.debug = true;
		#endif
	}

	public void TakeScreenshot()
	{
		instantShare = false;
		StartCoroutine(getScreenshot());
	}

	public void TakeScreenshotAndShare()
	{
		instantShare = true;
		StartCoroutine(getScreenshot());
	}

	public void ShareScreenshot()
	{
		ShareNativeImage ( ShareText );
	}
		
	void OnGUI()
	{
		if (simulateCapturedArea) 
		{
			GUI.Box (new Rect (
				Mathf.RoundToInt (Screen.width*BorderLeft),
				Mathf.RoundToInt (Screen.height * BorderTop),
				Mathf.RoundToInt (Screen.width - (Screen.width * BorderLeft) - (Screen.width*BorderRight)), 
				Mathf.RoundToInt (Screen.height - (Screen.height * BorderTop) - (Screen.height*BorderBottom))),"");
		}
	}

	IEnumerator getScreenshot()
	{
		
		if(SharePopupWatermark)SharePopupWatermark.SetActive(true);

        if(hideGameObjectsWhileScreenshot.Count>0) {
            foreach(GameObject go in hideGameObjectsWhileScreenshot) {
                go.SetActive(false);
            }
        }

		yield return new WaitForEndOfFrame();

		int resX = Mathf.RoundToInt (Screen.width - (Screen.width * BorderLeft) - (Screen.width*BorderRight));
		int resY = Mathf.RoundToInt (Screen.height - (Screen.height * BorderTop) - (Screen.height*BorderBottom));
		int startPosX = Mathf.RoundToInt (Screen.width * BorderLeft);
		int startPosY = Mathf.RoundToInt (Screen.height * BorderBottom);

		bool resOk = true;

		if (resX <= 0 || resY <= 0) 
		{
			Debug.LogError ("Invalid bounds!");
			resOk = false;
		}

		if (resOk) {
			createdTexture = new Texture2D (resX, resY, TextureFormat.ARGB32, false);
			createdTexture.ReadPixels (new Rect (startPosX, startPosY, resX, resY), 0, 0, false);
			createdTexture.Apply ();

			if (SharePopupWindow)
				SharePopupWindow.SetActive (true);

			if (instantShare) {
				ShareScreenshot ();
			}
				
			if (SharePopupImage) {
				Sprite image = Sprite.Create (createdTexture, new Rect (0, 0, resX, resY), new Vector2 (0.5f, 0.5f), 100f);
				SharePopupImage.sprite = image;
			}
            if (OnScreenShotReady!=null)
               OnScreenShotReady(createdTexture);
		}

		if (SharePopupWatermark)
			SharePopupWatermark.SetActive (false);

        if (hideGameObjectsWhileScreenshot.Count > 0)
        {
            foreach (GameObject go in hideGameObjectsWhileScreenshot)
            {
                go.SetActive(true);
            }
        }


		yield return null;
	}



	void ShareNativeImage(string shareText) {

		if(Application.isEditor)
		{
			#if UNITY_EDITOR
			UnityEditor.EditorUtility.DisplayDialog("Ooops!","Social sharing is not working in the editor!","Got it!");
			#endif
			Debug.LogError("UniShare: Social sharing is not working in the editor!");
			return;
		}

		#if NETFX_CORE

		screenshotPath = Path.Combine (Application.persistentDataPath, ScreenshotName).Replace("/","\\");

		byte[] imgData = createdTexture.EncodeToPNG();

		//write out all the bytes into a png
		File.WriteAllBytes (screenshotPath, imgData);

		_Call();

		#endif

		#if UNITY_IPHONE
		byte[] imgData = createdTexture.EncodeToPNG();
		presentActivitySheetWithImageAndString(shareText,imgData,imgData.Length);
		#endif

		#if UNITY_ANDROID
		string screenShotPath = Application.persistentDataPath + "/" + ScreenshotName;
		System.IO.File.WriteAllBytes(screenShotPath, createdTexture.EncodeToPNG());
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

		intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + screenShotPath);
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
		intentObject.Call<AndroidJavaObject>("setType", "image/png");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText);

		AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

		AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share");
		currentActivity.Call("startActivityForResult", jChooser,0);

		#endif
	}

	void _Call()
	{
		#if NETFX_CORE

		//ui thread of course..
		UnityEngine.WSA.Application.InvokeOnUIThread(() =>
		{

		DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
		dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.DataRequested);

		Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();

		}, false);

		#endif
	}

	#if NETFX_CORE
	private async void DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
	{
		try
		{
			DataRequest request = e.Request;
			request.Data.Properties.Title = "Share";
			request.Data.Properties.Description = ShareText;

			Windows.Storage.StorageFile sampleFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(screenshotPath);

			request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(sampleFile));
		}
		catch (System.Exception ex)
		{
			Debug.Log(ex.Message);
		}
	}
	#endif

	#if UNITY_EDITOR
	static class UniShareGameObjectCreator {
		[MenuItem("GameObject/UniShare", false,1)]
		static void Create() {

			PlayerSettings.Android.forceSDCardPermission = true;

			GameObject go = new GameObject("UniShare", typeof(UniShare));

			Canvas canvas = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Canvas>() : null;

			Selection.activeGameObject = go;
		}
	}
	#endif
}

