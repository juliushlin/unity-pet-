using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class XiaoyueAuthoring
{
    [MenuItem("小月/恢复原画与铅笔聊天场景")]
    public static void Setup()
    {
        Directory.CreateDirectory("Assets/Resources");
        if(!File.Exists("Assets/Resources/XiaoyuePortrait.png"))File.Copy("Assets/Art/Companion.png","Assets/Resources/XiaoyuePortrait.png");
        AssetDatabase.Refresh();var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/XiaoyuePortrait.png");importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.SaveAndReimport();
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/PetSprite.mat");if(!material){material=new Material(Shader.Find("Moonlit/PetSprite"));AssetDatabase.CreateAsset(material,"Assets/Resources/PetSprite.mat");}material.SetFloat("_Cutoff",.35f);EditorUtility.SetDirty(material);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);camera.GetComponent<Camera>().orthographic=true;camera.GetComponent<Camera>().orthographicSize=4.4f;
        new GameObject("小月 · 原画互动伙伴",typeof(MoonlitPet));Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,"Assets/Scenes/Xiaoyue.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Xiaoyue.unity",true)};
        PlayerSettings.productName="小月";PlayerSettings.companyName="Xiaoyue Studio";PlayerSettings.defaultScreenWidth=520;PlayerSettings.defaultScreenHeight=820;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.useFlipModelSwapchain=false;PlayerSettings.runInBackground=true;AssetDatabase.SaveAssets();
    }
}
