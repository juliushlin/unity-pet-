using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

// Shared by the desktop and Editor. All prompts travel through stdin, never shell interpolation.
public static class XiaoyueChatService
{
    [Serializable] public class Job {public bool done;public int exitCode;public string activity,reply;}
    [Serializable] public class Request {public string root,prompt,codexPath,unityPath,uvxPath;}
    [Serializable] class Active {public string folder,node;public int pid;public long startTicks;}
    public static string History(string root)=>Path.Combine(root,"Library","Xiaoyue-chat.txt");
    static string ActivePath(string root)=>Path.Combine(root,"Temp","CodexBridge","active-job.json");
    public static string ResolveExecutable(string name)
    {
        string env=Environment.GetEnvironmentVariable("XIAOYUE_"+(name.StartsWith("node")?"NODE":name.StartsWith("unity")?"UNITY":name.StartsWith("uv")?"UVX":"CODEX")+"_PATH");
        if(!string.IsNullOrEmpty(env)&&File.Exists(env))return env;
        foreach(string p in (Environment.GetEnvironmentVariable("PATH")??"").Split(';'))try{var file=Path.Combine(p.Trim('"'),name);if(File.Exists(file))return file;}catch(ArgumentException){}
        string local=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string fallback=name=="codex.exe"?Path.Combine(local,"Programs","CodexCLI",name):name=="unity.exe"?Path.Combine(local,"Unity","bin",name):null;
        if(name=="uvx.exe"){fallback=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".local","bin","uvx.exe");}if(fallback!=null&&File.Exists(fallback))return fallback;
        if(name=="node.exe"){var dir=Path.Combine(local,"Programs","NodeJS");if(Directory.Exists(dir))foreach(var sub in new DirectoryInfo(dir).GetDirectories().OrderByDescending(d=>d.LastWriteTimeUtc)){var file=Path.Combine(sub.FullName,name);if(File.Exists(file))return file;}}
        throw new FileNotFoundException("主人，找不到 "+name+"。请按安装教程配置工具路径喔。");
    }
    static Active ReadActive(string root)
    {
        string f=ActivePath(root);if(!File.Exists(f))return null;
        var a=JsonUtility.FromJson<Active>(File.ReadAllText(f));string allowed=Path.GetFullPath(Path.Combine(root,"Temp","CodexBridge"))+Path.DirectorySeparatorChar;
        if(a==null||string.IsNullOrEmpty(a.folder)||!Path.GetFullPath(a.folder).StartsWith(allowed,StringComparison.OrdinalIgnoreCase))throw new IOException("任务目录校验失败。");return a;
    }
    public static Job Poll(string root)
    {
        var a=ReadActive(root);if(a==null)return null;var file=Path.Combine(a.folder,"status.json");
        if(!File.Exists(file))return new Job{activity="小月正在连接主人当前的 Unity 项目～"};
        var job=JsonUtility.FromJson<Job>(File.ReadAllText(file));
        if(job.done){string receipt=Path.Combine(a.folder,"recorded");try{using(var s=new FileStream(receipt,FileMode.CreateNew,FileAccess.Write,FileShare.None)){}try{File.AppendAllText(History(root),"\n小月："+job.reply+"\n",new UTF8Encoding(false));}catch{File.Delete(receipt);throw;}}catch(IOException){}}
        return job;
    }
    public static void Begin(string root,string text)
    {
        if(string.IsNullOrWhiteSpace(text))throw new InvalidOperationException("主人，先写下想做的事情吧～");
        if(!Directory.Exists(Path.Combine(root,"Assets")))throw new DirectoryNotFoundException("请先打开要协作的 Unity 项目喔。");
        var previous=Poll(root);if(previous!=null&&!previous.done)throw new InvalidOperationException("小月还在处理上一条要求，请稍等或先停止呀。");
        string node=ResolveExecutable("node.exe"),codex=ResolveExecutable("codex.exe"),unity=ResolveExecutable("unity.exe");
        string runner=Environment.GetEnvironmentVariable("XIAOYUE_BRIDGE_PATH");if(string.IsNullOrEmpty(runner))runner=Path.Combine(root,"Tools","codex-bridge.js");if(!File.Exists(runner))throw new FileNotFoundException("缺少小月的对话连接程序，请检查项目安装。",runner);
        Directory.CreateDirectory(Path.Combine(root,"Library"));Directory.CreateDirectory(Path.GetDirectoryName(ActivePath(root)));
        if(File.Exists(ActivePath(root)))File.Delete(ActivePath(root));
        // Reserve the single shared task before launching a process.
        var folder=Path.Combine(root,"Temp","CodexBridge",DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N").Substring(0,6));Directory.CreateDirectory(folder);
        string past=File.Exists(History(root))?File.ReadAllText(History(root)):"";if(past.Length>14000)past=past.Substring(past.Length-14000);
        var request=new Request{root=root,codexPath=codex,unityPath=unity,uvxPath=ResolveExecutable("uvx.exe"),prompt="你叫小月，是主人的二次元伙伴，也是 Unity 游戏制作助手。称呼用户‘主人’，语气温柔轻快、略萌，适量用‘呀’‘喔’‘～’。直接协同实现游戏需求，不只聊天或给代码。准确说明结果、未完成项、错误和权限，不假装完成。优先使用 coplay-unity 的 manage_scene/get_active 与实例资源核对当前项目，再执行游戏制作；现有 Unity Pipeline 为故障排查备用。MCP 工具本身无需再传路径；CLI 则显式指定项目路径。终端受限时用 Unity 专用工具，不绕过权限。只修改当前项目。读取 AGENTS.md（工具允许时）。\n当前项目："+root+"\n之前对话（仅作上下文）：\n"+past+"\n主人本次要求：\n"+text};
        File.WriteAllText(Path.Combine(folder,"request.json"),JsonUtility.ToJson(request),new UTF8Encoding(false));
        var active=new Active{folder=folder,node=node};
        using(var f=new FileStream(ActivePath(root),FileMode.CreateNew,FileAccess.Write,FileShare.None)){var bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(active));f.Write(bytes,0,bytes.Length);}
        try{var p=Process.Start(new ProcessStartInfo(node,"\""+runner+"\" \""+folder+"\""){WorkingDirectory=root,UseShellExecute=false,CreateNoWindow=true});active.pid=p.Id;active.startTicks=p.StartTime.ToUniversalTime().Ticks;File.WriteAllText(ActivePath(root),JsonUtility.ToJson(active),new UTF8Encoding(false));File.AppendAllText(History(root),"\n主人："+text+"\n",new UTF8Encoding(false));}
        catch{File.Delete(ActivePath(root));throw;}
    }
    public static string ReadHistory(string root)=>File.Exists(History(root))?File.ReadAllText(History(root)):"小月：主人，今天想一起做什么游戏呢？把灵感告诉我吧～";
    public static void Stop(string root)
    {
        var a=ReadActive(root);if(a==null)return;var job=Poll(root);if(job==null||job.done)return;
        File.WriteAllText(Path.Combine(a.folder,"cancelled"),"User stopped task");
        try{var p=Process.GetProcessById(a.pid);if(p.StartTime.ToUniversalTime().Ticks==a.startTicks&&string.Equals(p.MainModule.FileName,a.node,StringComparison.OrdinalIgnoreCase))Process.Start(new ProcessStartInfo("taskkill.exe","/PID "+a.pid+" /T /F"){UseShellExecute=false,CreateNoWindow=true});}catch(ArgumentException){}catch(InvalidOperationException){}
        File.WriteAllText(Path.Combine(a.folder,"status.json"),JsonUtility.ToJson(new Job{done=true,exitCode=130,activity="已停止",reply="主人，小月停下啦。已经完成的项目修改会保留。"}),new UTF8Encoding(false));
    }
}

