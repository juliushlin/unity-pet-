using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using MCPForUnity.Editor.Services;

[InitializeOnLoad]
public static class XiaoyueCoplayBootstrap
{
    public static string LastStatus="等待 Unity 编译完成";
    static XiaoyueCoplayBootstrap(){EditorApplication.delayCall+=Start;}
    [MenuItem("小月/MCP/启动 Coplay 桥接")]
    public static async void Start()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=Start;return;}
        try{EditorPrefs.SetBool("MCPForUnity.UseHttpTransport",false);EditorPrefs.SetBool("MCPForUnity.TelemetryDisabled",true);bool ok=await MCPServiceLocator.TransportManager.StartAsync(MCPForUnity.Editor.Services.Transport.TransportMode.Stdio);LastStatus=ok?"Coplay MCP bridge ready":"Coplay MCP bridge could not start";Debug.Log(LastStatus+"; port="+MCPServiceLocator.Bridge.CurrentPort);File.WriteAllText(Path.Combine(XiaoyueStudio.Root,"Library","XiaoyueMcp-state.txt"),LastStatus+"; port="+MCPServiceLocator.Bridge.CurrentPort);}
        catch(Exception e){LastStatus=e.Message;Debug.LogError("Coplay MCP: "+e.Message);}
    }
}
