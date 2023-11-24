using Exoa.Designer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Exoa.Designer.Utils
{
    public class ThumbnailGeneratorUtils
    {
        public static bool Exists(string filaName)
        {
            HDLogger.Log("Thumb Exists:" + filaName, HDLogger.LogCategory.Screenshot);
            if (_DatabaseController.connectionState)
            {
                if (SaveSystem.Create(SaveSystem.Mode.ONLINE).Exists(filaName, HDSettings.EXT_THUMBNAIL_FOLDER, ".png"))
                    return true;
            }
            else
            {
                if (SaveSystem.Create(SaveSystem.Mode.FILE_SYSTEM).Exists(filaName, HDSettings.EXT_THUMBNAIL_FOLDER, ".png"))
                    return true;
            }
            if (SaveSystem.Create(SaveSystem.Mode.RESOURCES).Exists(filaName, HDSettings.EMBEDDED_THUMBNAIL_FOLDER, ".png"))
                return true;
            return false;
        }

        public async static Task<Texture2D> Load(string fileName)
        {
            HDLogger.Log("Thumb Load:" + fileName, HDLogger.LogCategory.Screenshot);
            if (_DatabaseController.connectionState)
            {
                Texture2D extTex = await SaveSystem.Create(SaveSystem.Mode.ONLINE).LoadTextureItem(fileName, HDSettings.EXT_THUMBNAIL_FOLDER, null, ".png");
                if (extTex == null)
                    extTex = await SaveSystem.Create(SaveSystem.Mode.RESOURCES).LoadTextureItem(fileName, HDSettings.EMBEDDED_THUMBNAIL_FOLDER, null, ".png");
                return extTex;
            }
            else
            {
                Texture2D extTex = await SaveSystem.Create(SaveSystem.Mode.FILE_SYSTEM).LoadTextureItem(fileName, HDSettings.EXT_THUMBNAIL_FOLDER, null, ".png");
                if (extTex == null)
                    extTex = await SaveSystem.Create(SaveSystem.Mode.RESOURCES).LoadTextureItem(fileName, HDSettings.EMBEDDED_THUMBNAIL_FOLDER, null, ".png");
                return extTex;
            }
        }
        public static void TakeAndSaveScreenshot(Transform target, string filaName, bool orthographic, Vector3 direction)
        {
            RuntimePreviewGenerator.BackgroundColor = HDSettings.THUMBNAIL_BACKGROUND;
            RuntimePreviewGenerator.MarkTextureNonReadable = false;
            RuntimePreviewGenerator.OrthographicMode = orthographic;
            RuntimePreviewGenerator.PreviewDirection = direction;

            Texture2D tex = RuntimePreviewGenerator.GenerateModelPreview(target, 256, 256);

            try
            {
                byte[] _bytes = tex.EncodeToPNG();

                //Debug.Log("Saving Thumbnail path:" + filaName);
                if (_DatabaseController.connectionState)
                {
                    SaveSystem.Create(SaveSystem.Mode.ONLINE).SaveFileItem(filaName, HDSettings.EXT_THUMBNAIL_FOLDER, _bytes);
                }
                else
                {
                    SaveSystem.Create(SaveSystem.Mode.FILE_SYSTEM).SaveFileItem(filaName, HDSettings.EXT_THUMBNAIL_FOLDER, _bytes);
                }
            }
            catch (Exception e) { Debug.LogError(e.Message); }

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

        }

        public static void Duplicate(string v1, string v2)
        {
            try
            {
                if (_DatabaseController.connectionState)
                {
                    SaveSystem.Create(SaveSystem.Mode.ONLINE).CopyFileItem(v1, v2, HDSettings.EXT_THUMBNAIL_FOLDER, null, ".png");
                }
                else
                {
                    SaveSystem.Create(SaveSystem.Mode.FILE_SYSTEM).CopyFileItem(v1, v2, HDSettings.EXT_THUMBNAIL_FOLDER, null, ".png");
                }
            }
            catch (Exception e) { Debug.LogError(e.Message); }

        }
    }
}
