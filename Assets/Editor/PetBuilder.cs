using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class PetBuilder
{
    public static void Setup()
    {
        var path="Assets/Art/Companion.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(path); importer.isReadable=true; importer.alphaIsTransparency=true; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.maxTextureSize=2048; importer.SaveAndReimport();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Camera").AddComponent<Camera>(); camera.tag="MainCamera"; camera.orthographic=true;camera.orthographicSize=3.1f;camera.transform.position=new Vector3(0,0,-10);camera.backgroundColor=new Color(.075f,.065f,.10f,1);camera.clearFlags=CameraClearFlags.SolidColor;
        camera.gameObject.AddComponent<AudioListener>();
        var pet=new GameObject("Moonlit Companion").AddComponent<MoonlitPet>();
        Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,"Assets/Scenes/MoonlitPet.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MoonlitPet.unity",true)};
        PlayerSettings.companyName="Moonlit Studio";PlayerSettings.productName="月下小伙伴";PlayerSettings.defaultScreenWidth=360;PlayerSettings.defaultScreenHeight=450;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=false;PlayerSettings.runInBackground=true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);PlayerSettings.SetGraphicsAPIs(UnityEditor.BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
        PlayerSettings.useFlipModelSwapchain=false;
        AssetDatabase.SaveAssets();DesktopPetStudio.Open();Debug.Log("MoonlitPet setup complete");
    }
    [MenuItem("桌宠/导出 Windows 桌宠")]
    public static void Build()
    {
        SessionState.SetBool(PetEditorLink.Building,true);PetEditorLink.StopPet();
        try {
        PlayerSettings.useFlipModelSwapchain=false;PlayerSettings.runInBackground=true;
        Directory.CreateDirectory("Assets/Resources");
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/PetSprite.mat");
        if(!material){material=new Material(Shader.Find("Moonlit/PetSprite"));AssetDatabase.CreateAsset(material,"Assets/Resources/PetSprite.mat");}
        material.SetFloat("_Cutoff",.35f);EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();
        PlayerSettings.productName="小月";PlayerSettings.companyName="Xiaoyue Studio";PlayerSettings.defaultScreenWidth=520;PlayerSettings.defaultScreenHeight=820;
        var output=Path.GetFullPath(Path.Combine(Application.dataPath,"../Builds/Xiaoyue/Xiaoyue.exe"));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Xiaoyue.unity"},output,BuildTarget.StandaloneWindows64,BuildOptions.None);
        Debug.Log("Desktop pet build: "+report.summary.result+" at "+output);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("桌宠构建未完成："+report.summary.result);
        } finally {SessionState.SetBool(PetEditorLink.Building,false);}
        PetEditorLink.StartPet();
    }
}
