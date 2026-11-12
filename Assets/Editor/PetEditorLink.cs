using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class PetEditorLink
{
    const string Launched="MoonlitPet.AutoLaunched",Pid="MoonlitPet.DesktopPid";
    public const string Building="MoonlitPet.Building";
    static double next;
    public static string Exe=>Path.Combine(DesktopPetStudio.Root,"Builds","Xiaoyue","Xiaoyue.exe");
    static PetEditorLink(){EditorApplication.update+=Update;EditorApplication.quitting+=Quit;MoonlitPet.ChatRequested-=OpenChat;MoonlitPet.ChatRequested+=OpenChat;}
    static void OpenChat(){XiaoyueStudio.Open();}
    static void Update()
    {
        if(EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.35;
        if(!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Xiaoyue","global-enabled"))&&!Application.isBatchMode&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&!SessionState.GetBool(Building,false)&&!SessionState.GetBool(Launched,false)&&File.Exists(Exe))StartPet();
        string request=Path.Combine(DesktopPetStudio.Root,"Temp","PetChat.request");
        if(File.Exists(request))try{File.Delete(request);DesktopPetStudio.Open();}catch(IOException){}
    }
    [MenuItem("桌宠/重新唤醒桌面伙伴")]
    public static void StartPet(){Launch(false);}
    public static void StartInspection(){Launch(true);}
    static void Launch(bool inspect)
    {
        StopPet();if(!File.Exists(Exe)){UnityEngine.Debug.LogWarning("主人，先从桌宠菜单导出一次 Windows 桌宠喔。");return;}
        var psi=new ProcessStartInfo(Exe,"--unity-owner-pid "+Process.GetCurrentProcess().Id+" --unity-project \""+DesktopPetStudio.Root+"\""+(inspect?" --show-in-taskbar":"")){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(Exe)};
        var p=Process.Start(psi);SessionState.SetInt(Pid,p.Id);SessionState.SetBool(Launched,true);
    }
    public static void StopPet()
    {
        int pid=SessionState.GetInt(Pid,0);if(pid<=0)return;
        try{var p=Process.GetProcessById(pid);if(string.Equals(p.MainModule.FileName,Exe,StringComparison.OrdinalIgnoreCase)){p.CloseMainWindow();if(!p.WaitForExit(1200))p.Kill();}}catch(ArgumentException){}catch(InvalidOperationException){}
        SessionState.SetInt(Pid,0);
    }
    static void Quit(){StopPet();try{XiaoyueChatService.Stop(XiaoyueStudio.Root);}catch(Exception e){UnityEngine.Debug.LogWarning(e.Message);}}
}

public class PetCompanionDock:EditorWindow {public static void Open(){XiaoyueStudio.Open();}}

