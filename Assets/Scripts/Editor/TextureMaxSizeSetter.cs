using UnityEditor;

public class TextureMaxSizeSetter
{
    [MenuItem("Tools/Set Android Texture Max Size")]
    static void SetMaxSize()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);

            if (importer != null)
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Android");
                settings.overridden = true;
                settings.maxTextureSize = 256;
                importer.SetPlatformTextureSettings(settings);

                importer.SaveAndReimport();
            }
        }
    }
}