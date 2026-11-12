using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
public class XiaoyueChatUI:MonoBehaviour
{
    public const int WindowWidth=520,WindowHeight=820;
    public static XiaoyueChatUI Instance{get;private set;}
    public bool Expanded{get;private set;}
    public float Expansion=>expansion;
    string project,lastHistory="";float nextPoll,expansion;Font font;
    UnityEngine.UI.InputField input;UnityEngine.UI.Text conversation,status,toggleLabel;UnityEngine.UI.Button send;
    UnityEngine.UI.ScrollRect scroll;RectTransform panel,pencil;CanvasGroup group;
    static readonly Color Cream=new Color(.985f,.963f,.927f,.99f),Wine=new Color(.36f,.13f,.22f),Scarlet=new Color(.67f,.10f,.21f),Muted=new Color(.59f,.39f,.43f),Gold=new Color(.80f,.62f,.36f);
    public void Create(string root)
    {
        Instance=this;project=root;font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","SimHei"},18);
        if(EventSystem.current==null){var e=new GameObject("XiaoyueEventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));e.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
        var canvas=new GameObject("XiaoyueConversation",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));canvas.transform.SetParent(transform,false);canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().sortingOrder=50;
        var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(WindowWidth,WindowHeight);scaler.matchWidthOrHeight=.5f;
        pencil=Rect("PencilToggle",canvas.transform,130,535,260,50);var bg=pencil.gameObject.AddComponent<UnityEngine.UI.Image>();bg.sprite=Round();bg.type=UnityEngine.UI.Image.Type.Sliced;bg.color=Cream;
        var button=pencil.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=bg;button.onClick.AddListener(Toggle);
        var icon=Rect("PencilIcon",pencil,18,12,25,25).gameObject.AddComponent<PencilIconGraphic>();icon.color=Scarlet;icon.raycastTarget=false;
        toggleLabel=Text("ToggleLabel",pencil,"主人，写下一个灵感…",56,14,190,28,14,Wine);
        panel=Rect("CompactConversation",canvas.transform,35,598,450,207);panel.pivot=new Vector2(.5f,1);panel.anchoredPosition=new Vector2(260,-598);
        var image=panel.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=Round();image.type=UnityEngine.UI.Image.Type.Sliced;image.color=Cream;
        group=panel.gameObject.AddComponent<CanvasGroup>();group.alpha=0;group.interactable=group.blocksRaycasts=false;
        Text("Heading",panel,"小月 · 陪主人一起做游戏",14,11,350,24,15,Scarlet,true);
        Button("Stop",panel,"停止",390,8,45,25,new Color(.95f,.89f,.86f),Wine,()=>{try{XiaoyueChatService.Stop(project);}catch(Exception e){status.text=e.Message;}});
        var area=Rect("ConversationScroll",panel,12,40,426,93);scroll=area.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.horizontal=false;scroll.inertia=false;
        var viewport=Rect("Viewport",area,0,0,426,93);var maskImage=viewport.gameObject.AddComponent<UnityEngine.UI.Image>();maskImage.color=Color.white;var mask=viewport.gameObject.AddComponent<UnityEngine.UI.Mask>();mask.showMaskGraphic=false;
        var content=Rect("Content",viewport,0,0,426,93);content.anchorMin=new Vector2(0,1);content.anchorMax=new Vector2(1,1);content.sizeDelta=Vector2.zero;
        var layout=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.padding=new RectOffset(4,4,4,4);layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandHeight=false;
        content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        conversation=Text("ConversationText",content,"主人，小月在这里呀～",0,0,418,85,13,Wine);conversation.supportRichText=false;conversation.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleWidth=1;scroll.content=content;scroll.viewport=viewport;
        var box=Rect("Prompt",panel,12,143,344,46);var inputBg=box.gameObject.AddComponent<UnityEngine.UI.Image>();inputBg.sprite=Round();inputBg.type=UnityEngine.UI.Image.Type.Sliced;inputBg.color=new Color(.95f,.90f,.87f);
        input=box.gameObject.AddComponent<UnityEngine.UI.InputField>();input.lineType=UnityEngine.UI.InputField.LineType.MultiLineNewline;input.textComponent=Text("InputText",box,"",10,7,324,34,14,Wine);input.textComponent.supportRichText=false;input.placeholder=Text("Placeholder",box,"告诉小月，你想做什么？",10,7,324,34,14,Muted);
        send=Button("Send",panel,"发送",365,143,73,46,Scarlet,Color.white,Send);
        status=Text("Status",panel,"主人，小月准备好啦～",14,190,420,15,10,Muted);status.verticalOverflow=VerticalWrapMode.Truncate;
    }
    public void Toggle(){SetExpanded(!Expanded);}
    public void SetExpanded(bool open){Expanded=open;toggleLabel.text=open?"收起聊天 · 小月在听喔":"主人，写下一个灵感…";if(open){input.Select();input.ActivateInputField();}else{input.DeactivateInputField();if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);}}
    public void FocusInput(){if(!Expanded)SetExpanded(true);else input.ActivateInputField();}
    public bool ContainsPointer(Vector2 p)=>(pencil&&RectTransformUtility.RectangleContainsScreenPoint(pencil,p))||(Expanded&&panel&&RectTransformUtility.RectangleContainsScreenPoint(panel,p));
    public bool IsTyping=>input&&input.isFocused;
    void Send(){try{XiaoyueChatService.Begin(project,input.text);input.text="";status.text="小月正在认真想办法，稍等喔～";}catch(Exception e){status.text=e.Message;}}
    void Update()
    {
        if(group){expansion=Mathf.MoveTowards(expansion,Expanded?1:0,Time.unscaledDeltaTime*5);float ease=1-Mathf.Pow(1-expansion,3);group.alpha=ease;group.interactable=group.blocksRaycasts=Expanded;panel.localScale=new Vector3(Mathf.Lerp(.94f,1,ease),Mathf.Lerp(.78f,1,ease),1);}
        if(Time.unscaledTime<nextPoll||string.IsNullOrEmpty(project))return;nextPoll=Time.unscaledTime+.3f;
        try{var j=XiaoyueChatService.Poll(project);if(j!=null){string message=j.activity??"";status.text=message.Length>48?message.Substring(0,48)+"…":message;send.interactable=j.done;}string h=XiaoyueChatService.ReadHistory(project);if(h!=lastHistory){lastHistory=h;conversation.text=h;Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=0;}}catch(Exception e){status.text=e.Message;}
    }
    void OnDestroy(){if(Instance==this)Instance=null;}

    static RectTransform Rect(string name,Transform parent,float x,float y,float width,float height){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(width,height);return r;}
    UnityEngine.UI.Text Text(string name,Transform parent,string value,float x,float y,float w,float h,int size,Color color,bool bold=false){var r=Rect(name,parent,x,y,w,h);var t=r.gameObject.AddComponent<UnityEngine.UI.Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;t.alignment=TextAnchor.UpperLeft;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;t.raycastTarget=false;return t;}
    void Image(string name,Transform parent,float x,float y,float w,float h,Color color){var r=Rect(name,parent,x,y,w,h);r.gameObject.AddComponent<UnityEngine.UI.Image>().color=color;}
    UnityEngine.UI.Button Button(string name,Transform parent,string value,float x,float y,float w,float h,Color bg,Color fg,UnityEngine.Events.UnityAction action){var r=Rect(name,parent,x,y,w,h);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=Round();image.type=UnityEngine.UI.Image.Type.Sliced;image.color=bg;var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;b.onClick.AddListener(action);Text("Label",r,value,0,0,w,h,16,fg,true).alignment=TextAnchor.MiddleCenter;return b;}
    static Sprite rounded;
    static Sprite Round(){if(rounded)return rounded;int n=40;var t=new Texture2D(n,n,TextureFormat.RGBA32,false);t.name="XiaoyueRoundedPanel";for(int y=0;y<n;y++)for(int x=0;x<n;x++){float dx=Mathf.Max(12-x,x-(n-13)),dy=Mathf.Max(12-y,y-(n-13));float d=Mathf.Sqrt(Mathf.Max(0,dx)*Mathf.Max(0,dx)+Mathf.Max(0,dy)*Mathf.Max(0,dy));t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(12.5f-d)));}t.Apply();rounded=Sprite.Create(t,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(14,14,14,14));return rounded;}
}


