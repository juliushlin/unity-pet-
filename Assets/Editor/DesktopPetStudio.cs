using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DesktopPetStudio : EditorWindow
{
    [Serializable] public class Request { public string root,prompt; }
    [Serializable] public class Job { public bool done; public int exitCode; public string activity,reply; }
    TextField prompt,transcript; Label state; Button send;
    const string Key="MoonlitPet.Job",Pid="MoonlitPet.Pid";
    string history="",status="主人，今天想一起做些什么呢～";
    public static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,".."));
    static string History=>Path.Combine(Root,"Library","CodexBridge-history.txt");
    [MenuItem("桌宠/制作面板")]
    public static void Open(){var w=GetWindow<DesktopPetStudio>(true,"月下小伙伴 · 协作",true);w.minSize=new Vector2(420,580);w.ShowUtility();}
    [InitializeOnLoadMethod] static void Welcome(){EditorApplication.delayCall+=()=>{if(!Application.isBatchMode&&!SessionState.GetBool("MoonlitPet.Welcomed",false)){SessionState.SetBool("MoonlitPet.Welcomed",true);PetCompanionDock.Open();}};}
    public void CreateGUI()
    {
        if(File.Exists(History))history=File.ReadAllText(History);
        var r=rootVisualElement;r.style.paddingLeft=18;r.style.paddingRight=18;r.style.paddingTop=18;
        var h=new Label("月下小伙伴");h.style.fontSize=25;h.style.unityFontStyleAndWeight=FontStyle.Bold;r.Add(h);
        var sub=new Label("主人，把灵感告诉我吧，我们一起把游戏做出来～");sub.style.marginTop=10;sub.style.marginBottom=12;r.Add(sub);
        state=new Label(status);state.style.whiteSpace=WhiteSpace.Normal;r.Add(state);
        var scroll=new ScrollView();scroll.style.flexGrow=1;scroll.style.marginTop=12;transcript=new TextField{multiline=true,isReadOnly=true,value=history};transcript.style.whiteSpace=WhiteSpace.Normal;scroll.Add(transcript);r.Add(scroll);
        prompt=new TextField{multiline=true,value="让桌宠被摸头时眯眼微笑，冒出小爱心。"};prompt.style.height=100;prompt.style.marginTop=12;r.Add(prompt);
        send=new Button(()=>{try{Submit(prompt.value);prompt.value="";}catch(Exception e){status=e.Message;}}){text="发送 · 自动连接 Unity"};send.style.height=38;send.style.marginTop=8;r.Add(send);
        var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;
        row.Add(new Button(()=>{try{VerifyBridge();}catch(Exception e){status=e.Message;}}){text="检查连接"});row.Add(new Button(Stop){text="停止"});row.Add(new Button(()=>{if(string.IsNullOrEmpty(SessionState.GetString(Key,""))){history="";File.WriteAllText(History,"");}}){text="新对话"});r.Add(row);
        var note=new Label("使用本机 Codex 登录。任务跨重新编译保留。额外权限不足时会说明原因；停止不会撤销已完成的修改。");note.style.fontSize=11;note.style.whiteSpace=WhiteSpace.Normal;note.style.marginTop=10;note.style.marginBottom=12;r.Add(note);
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;
    }
    void OnDisable(){EditorApplication.update-=Tick;}
    void Tick()
    {
        string folder=SessionState.GetString(Key,"");
        if(!string.IsNullOrEmpty(folder))try{
            string f=Path.Combine(folder,"status.json");if(File.Exists(f)){var j=JsonUtility.FromJson<Job>(File.ReadAllText(f));
                var replyFile=Path.Combine(folder,"reply.txt");var eventFile=Path.Combine(folder,"events.jsonl");
                if(!j.done&&File.Exists(replyFile)&&File.Exists(eventFile)&&File.ReadAllText(eventFile).Contains("\"type\":\"turn.completed\"")){j.done=true;j.reply=File.ReadAllText(replyFile);j.activity="已回复 · 可以继续输入要求";}
                status=j.activity;
                if(j.done){history=File.ReadAllText(History)+"\nCodex："+j.reply+"\n";File.WriteAllText(History,history);SessionState.EraseString(Key);AssetDatabase.Refresh();}}
        }catch(IOException){}catch(Exception e){status=e.Message;}
        if(state!=null){state.text=status;transcript.value=history;send.SetEnabled(string.IsNullOrEmpty(SessionState.GetString(Key,"")));}
    }
    public static void Submit(string request)
    {
        if(!string.IsNullOrEmpty(SessionState.GetString(Key,"")))throw new InvalidOperationException("当前任务仍在进行，请完成或停止后再发送。");
        if(string.IsNullOrWhiteSpace(request))throw new InvalidOperationException("请先输入要求。");
        var node=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Programs","NodeJS","node-v24.22.2-win-x64","node.exe");
        if(!File.Exists(node))throw new FileNotFoundException("未找到 Node.js。",node);
        string previous=File.Exists(History)?File.ReadAllText(History):"";if(previous.Length>14000)previous=previous.Substring(previous.Length-14000);
        var folder=Path.Combine(Root,"Temp","CodexBridge",DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));Directory.CreateDirectory(folder);
        var payload=new Request{root=Root,prompt="你是主人的二次元小伙伴，也是当前 Unity 项目的游戏制作助手。称呼用户为‘主人’，中文语气轻快、温柔、稍萌，可以适量用‘呀’‘喔’‘～’，不要过度撒娇或堆砌表情。保持结果、错误和权限说明准确，不假装完成。直接与主人协同制作游戏，不局限于桌宠。先使用已固定到当前项目的 Unity MCP editor_status 核对目标；MCP 工具不用再次传路径。CLI 操作则必须显式指定当前项目。终端被策略拒绝时使用 Unity 专用工具，不绕过权限。不要连接旧项目。\n当前项目："+Root+"\n之前的对话：\n"+previous+"\n本次要求：\n"+request};
        File.WriteAllText(Path.Combine(folder,"request.json"),JsonUtility.ToJson(payload),new UTF8Encoding(false));File.AppendAllText(History,"\n你："+request+"\n");
        var psi=new ProcessStartInfo(node,"\""+Path.Combine(Root,"Tools","codex-bridge.js")+"\" \""+folder+"\""){WorkingDirectory=Root,UseShellExecute=false,CreateNoWindow=true};
        var p=Process.Start(psi);SessionState.SetInt(Pid,p.Id);SessionState.SetString(Key,folder);
    }
    void Stop(){if(string.IsNullOrEmpty(SessionState.GetString(Key,"")))return;int pid=SessionState.GetInt(Pid,0);try{Process.Start(new ProcessStartInfo("taskkill.exe","/PID "+pid+" /T /F"){UseShellExecute=false,CreateNoWindow=true});}catch{}SessionState.EraseString(Key);status="已停止；已完成的修改保留。";}
    public static void CancelTask(){var w=Resources.FindObjectsOfTypeAll<DesktopPetStudio>();if(w.Length>0)w[0].Stop();}
    public static void VerifyBridge(){Open();Submit("只读连接验证：通过 Unity CLI 显式指定本项目路径，读取 editor status 和 console status；返回 Unity 版本、ready 状态和错误数。不修改文件，不制作宠物，不打开旧项目。");}
}
