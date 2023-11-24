#if UNITY_EDITOR && UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

public class PostProcessScript : MonoBehaviour {

	[PostProcessBuild]
	public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject) {

		PlayerSettings.Android.forceSDCardPermission = true; 

		if (buildTarget == BuildTarget.iOS) {

			// Get plist
			string plistPath = pathToBuiltProject + "/Info.plist";
			PlistDocument plist = new PlistDocument ();
			plist.ReadFromString (File.ReadAllText (plistPath));

			// Get root
			PlistElementDict rootDict = plist.root;
			PlistElementArray plista = rootDict.CreateArray ("LSApplicationQueriesSchemes");
			plista.AddString ("whatsapp");
			plista.AddString ("vk");
			plista.AddString ("vk-share");
			plista.AddString ("vkauthorize");

			rootDict.SetString ("NSPhotoLibraryUsageDescription","Permission to access to the Photo Library is needed to save screenshots.");

			// Write to file
			File.WriteAllText (plistPath, plist.WriteToString ());

			string projPath = PBXProject.GetPBXProjectPath (pathToBuiltProject);
			PBXProject proj = new PBXProject ();

			proj.ReadFromString (File.ReadAllText (projPath));
			string target = proj.TargetGuidByName ("Unity-iPhone");

			proj.SetBuildProperty (target, "ENABLE_BITCODE", "false");

			proj.SetBuildProperty (target, "GCC_ENABLE_OBJC_EXCEPTIONS", "true");

			string[] frameworks = new string[] { 
                "AudioToolbox.framework",
				"CFNetwork.framework",
				"CoreGraphics.framework",
				"CoreLocation.framework",
				"libz.1.1.3.dylib",
				"MobileCoreServices.framework",
				"QuartzCore.framework",
				"Security.framework",
				"StoreKit.framework",
				"SystemConfiguration.framework", 
                "SafariServices.framework"
			};
			
			for (int i = 0; i < frameworks.Length; i++) {
				proj.AddFrameworkToProject (target, frameworks [i], true);
			}
				
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories("Assets/Plugins/UniShare/IOS/res", "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(Path.Combine(pathToBuiltProject, dirPath.Replace("Assets/Plugins/UniShare/IOS/res/", "Libraries/Plugins/UniShare/IOS/Resources/")));
            }

            //Copy all the files & Replaces any files with the same name
            string[] allFiles = Directory.GetFiles("Assets/Plugins/UniShare/IOS/res", "*.*", SearchOption.AllDirectories);

            foreach (string newPath in allFiles) 
			{
                if (newPath.Contains(".meta")) continue; // leave meta files behind
				File.Copy (newPath, Path.Combine (pathToBuiltProject, newPath.Replace ("Assets/Plugins/UniShare/IOS/res/", "Libraries/Plugins/UniShare/IOS/Resources/")), true);
                proj.AddFileToBuild(target,
                        proj.AddFile(
                            newPath.Replace("Assets/Plugins/UniShare/IOS/res/", "Libraries/Plugins/UniShare/IOS/Resources/"),
                            newPath.Replace("Assets/Plugins/UniShare/IOS/res/", "Libraries/Plugins/UniShare/IOS/Resources/"),
                            PBXSourceTree.Source)
                       );

            }

			File.WriteAllText (projPath, proj.WriteToString ());
		}
	}
}
#endif