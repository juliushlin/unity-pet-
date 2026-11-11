using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
public class XiaoyueStudio:EditorWindow
{
    TextField input,history;Label status,toggleText;Button send;VisualElement chatPanel;Image portrait;PreviewRenderUtility preview;XiaoyueSpriteAvatar avatar;
    readonly PetMotion motion=new PetMotion();double next,started;bool expanded;float expansion;
    public static string Root=>System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,".."));
    [MenuItem("小月/协作窗口")]public static void Open(){var w=GetWindow<XiaoyueStudio>(true,"小月 · 主人的小伙伴",true);w.minSize=new Vector2(420,630);w.ShowUtility();}
    public void CreateGUI()
    {
        var r=rootVisualElement;r.Clear();r.style.flexDirection=FlexDirection.Column;r.style.backgroundColor=new Color(.98f,.955f,.93f);r.style.paddingLeft=18;r.style.paddingRight=18;r.style.paddingTop=8;r.style.paddingBottom=12;
        var title=new Label("小月");title.style.fontSize=22;title.style.unityTextAlign=TextAnchor.MiddleCenter;title.style.color=new Color(.67f,.1f,.21f);r.Add(title);
        portrait=new Image{scaleMode=ScaleMode.ScaleToFit};portrait.style.flexGrow=1;portrait.style.minHeight=250;r.Add(portrait);
        var pencil=new Button(Toggle);pencil.style.height=46;pencil.style.flexShrink=0;pencil.style.marginLeft=38;pencil.style.marginRight=38;pencil.style.backgroundColor=new Color(1,.98f,.945f);pencil.style.flexDirection=FlexDirection.Row;pencil.style.borderTopLeftRadius=pencil.style.borderTopRightRadius=pencil.style.borderBottomLeftRadius=pencil.style.borderBottomRightRadius=15;
        var icon=new PencilToggleIcon();icon.style.width=26;icon.style.height=26;icon.style.marginLeft=15;icon.style.marginTop=8;pencil.Add(icon);toggleText=new Label("主人，写下一个灵感…");toggleText.style.color=new Color(.48f,.17f,.25f);toggleText.style.marginLeft=12;toggleText.style.marginTop=12;pencil.Add(toggleText);r.Add(pencil);
        chatPanel=new VisualElement();chatPanel.style.marginTop=10;chatPanel.style.backgroundColor=new Color(1,.98f,.945f);chatPanel.style.paddingLeft=12;chatPanel.style.paddingRight=12;chatPanel.style.paddingTop=10;chatPanel.style.paddingBottom=8;chatPanel.style.borderTopLeftRadius=chatPanel.style.borderTopRightRadius=chatPanel.style.borderBottomLeftRadius=chatPanel.style.borderBottomRightRadius=15;chatPanel.style.display=DisplayStyle.None;r.Add(chatPanel);
        status=new Label("主人，小月在听喔～");status.style.fontSize=11;status.style.color=new Color(.58f,.32f,.39f);status.style.whiteSpace=WhiteSpace.Normal;chatPanel.Add(status);
        var scroll=new ScrollView();scroll.style.flexGrow=1;scroll.style.minHeight=70;history=new TextField{isReadOnly=true,multiline=true};StyleField(history,new Color(1,.99f,.97f));scroll.Add(history);chatPanel.Add(scroll);
        input=new TextField{multiline=true,tooltip="告诉小月，主人想做什么～"};input.style.height=46;input.style.marginTop=6;StyleField(input,new Color(.955f,.90f,.87f));chatPanel.Add(input);
        var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;chatPanel.Add(row);
        row.Add(new Button(()=>{try{XiaoyueChatService.Stop(Root);}catch(Exception e){status.text=e.Message;}}){text="停止"});
        send=new Button(()=>{try{XiaoyueChatService.Begin(Root,input.value);input.value="";}catch(Exception e){status.text=e.Message;}}){text="发送要求 →"};send.style.flexGrow=1;send.style.height=31;send.style.backgroundColor=new Color(.67f,.1f,.21f);send.style.color=Color.white;row.Add(send);
        preview?.Cleanup();preview=new PreviewRenderUtility();var obj=new GameObject("Original Xiaoyue Preview");avatar=obj.AddComponent<XiaoyueSpriteAvatar>();avatar.Initialize();preview.AddSingleGO(obj);preview.camera.orthographic=true;preview.camera.orthographicSize=3.1f;preview.camera.transform.position=new Vector3(0,1.35f,-10);preview.camera.transform.rotation=Quaternion.identity;preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.98f,.955f,.93f);
        started=EditorApplication.timeSinceStartup;motion.Open(started);portrait.RegisterCallback<PointerDownEvent>(e=>motion.React(5,EditorApplication.timeSinceStartup));EditorApplication.update-=Tick;EditorApplication.update+=Tick;
    }
    public void Toggle(){expanded=!expanded;toggleText.text=expanded?"收起聊天 · 小月在听喔":"主人，写下一个灵感…";if(expanded){chatPanel.style.display=DisplayStyle.Flex;input.Focus();}}
    static void StyleField(TextField f,Color bg){f.style.color=new Color(.36f,.13f,.22f);f.style.whiteSpace=WhiteSpace.Normal;var box=f.Q<VisualElement>("unity-text-input");if(box!=null){box.style.backgroundColor=bg;box.style.color=new Color(.36f,.13f,.22f);box.style.borderBottomWidth=box.style.borderTopWidth=box.style.borderLeftWidth=box.style.borderRightWidth=0;}}
    void Tick()
    {
        double t=EditorApplication.timeSinceStartup;if(t<next)return;float dt=(float)(t-next+.05);next=t+.05;expansion=Mathf.MoveTowards(expansion,expanded?1:0,Mathf.Clamp(dt,.01f,.1f)*5);chatPanel.style.height=220*expansion;chatPanel.style.opacity=expansion;if(!expanded&&expansion<=0)chatPanel.style.display=DisplayStyle.None;
        try{var j=XiaoyueChatService.Poll(Root);if(j!=null){string text=j.activity??"";status.text=text.Length>65?text.Substring(0,65)+"…":text;send.SetEnabled(j.done);}history.value=XiaoyueChatService.ReadHistory(Root);
            if(avatar&&portrait.resolvedStyle.width>1&&portrait.resolvedStyle.height>1){motion.Tick(t,Math.Min(t-started,DesktopWindow.IdleSeconds()));avatar.Animate(motion.State,motion.Pose,(float)(t-motion.ChangedAt),(float)t,expanded,.05f);preview.BeginPreview(new Rect(0,0,portrait.resolvedStyle.width,portrait.resolvedStyle.height),GUIStyle.none);preview.camera.aspect=portrait.resolvedStyle.width/portrait.resolvedStyle.height;preview.Render(true);portrait.image=preview.EndPreview();}}catch(Exception e){status.text=e.Message;}
    }
    void OnDisable(){EditorApplication.update-=Tick;preview?.Cleanup();preview=null;}
}
public class PencilToggleIcon:VisualElement
{
    public PencilToggleIcon(){pickingMode=PickingMode.Ignore;generateVisualContent+=ctx=>{var p=ctx.painter2D;p.strokeColor=new Color(.67f,.1f,.21f);p.lineWidth=2;p.BeginPath();p.MoveTo(new Vector2(9,4));p.LineTo(new Vector2(5,4));p.LineTo(new Vector2(3,7));p.LineTo(new Vector2(3,22));p.LineTo(new Vector2(21,22));p.LineTo(new Vector2(23,19));p.LineTo(new Vector2(23,15));p.Stroke();p.BeginPath();p.MoveTo(new Vector2(8,17));p.LineTo(new Vector2(10,12));p.LineTo(new Vector2(21,2));p.LineTo(new Vector2(25,6));p.LineTo(new Vector2(14,16));p.ClosePath();p.Stroke();};}
}
