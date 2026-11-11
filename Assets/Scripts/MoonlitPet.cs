using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
public class MoonlitPet:MonoBehaviour
{
    public int pose;public string mood="小月陪着主人呀～";public static event Action ChatRequested;
    readonly PetMotion motion=new PetMotion();public PetMotion.Mode CurrentState=>motion.State;
    public string ProjectRoot{get;private set;}public XiaoyueSpriteAvatar Avatar=>avatar;
    XiaoyueSpriteAvatar avatar;XiaoyueChatUI chat;int ownerPid,clickIndex;float lastInteraction,nextOwnerCheck,pressedAt;Vector2 pressed;bool holding,dragged;
    void Start()
    {
        Application.runInBackground=true;Application.targetFrameRate=30;var args=Environment.GetCommandLineArgs();
        for(int i=0;i<args.Length-1;i++){if(args[i]=="--unity-owner-pid")int.TryParse(args[i+1],out ownerPid);if(args[i]=="--unity-project")ProjectRoot=args[i+1];}
        if(Application.isEditor)ProjectRoot=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
        var cam=Camera.main;cam.orthographic=true;cam.orthographicSize=4.4f;cam.rect=new Rect(0,0,1,1);cam.transform.position=new Vector3(0,0,-10);cam.transform.rotation=Quaternion.identity;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Application.isEditor?new Color(.97f,.94f,.91f,1):new Color(0,0,0,0);cam.allowHDR=false;cam.allowMSAA=false;
        foreach(Transform child in transform)if(child.name=="Xiaoyue_3D")Destroy(child.gameObject);
        avatar=new GameObject("小月 · 原画动态").AddComponent<XiaoyueSpriteAvatar>();avatar.transform.SetParent(transform,false);avatar.Initialize();
        lastInteraction=Time.unscaledTime;motion.Open(lastInteraction);chat=new GameObject("小月 · 脚下铅笔聊天").AddComponent<XiaoyueChatUI>();chat.Create(ProjectRoot);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        DesktopWindow.Setup();
#endif
    }
    public void SetPose(int value,bool voice=true){lastInteraction=Time.unscaledTime;motion.React(Mathf.Clamp(value,0,5),lastInteraction);pose=motion.Pose;}
    public void RequestChat(){ChatRequested?.Invoke();if(chat)chat.FocusInput();}
    void Update()
    {
        if(!avatar)return;float t=Time.unscaledTime;
        if(ownerPid>0&&t>=nextOwnerCheck){nextOwnerCheck=t+1;try{if(System.Diagnostics.Process.GetProcessById(ownerPid).HasExited)Application.Quit();}catch(ArgumentException){Application.Quit();}}
        motion.Tick(t,Math.Min(t-lastInteraction,DesktopWindow.IdleSeconds()));pose=motion.Pose;avatar.Animate(motion.State,pose,t-(float)motion.ChangedAt,t,chat&&chat.Expanded,Time.unscaledDeltaTime);
        var mouse=Mouse.current;if(mouse==null)return;var p=mouse.position.ReadValue();bool overUI=chat&&chat.ContainsPointer(p);
        if(mouse.leftButton.wasPressedThisFrame&&!overUI&&Hit(p)){holding=true;dragged=false;pressed=p;pressedAt=t;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            DesktopWindow.BeginDrag();
#endif
        }
        if(holding&&mouse.leftButton.isPressed){if(Vector2.Distance(p,pressed)>8)dragged=true;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            DesktopWindow.Drag(ref dragged);
#endif
        }
        if(holding&&mouse.leftButton.wasReleasedThisFrame){holding=false;lastInteraction=t;if(!dragged){int[] actions={5,2,3,1};SetPose(t-pressedAt>.65f?3:actions[clickIndex++%4]);}else motion.Idle(t);}
        if(mouse.rightButton.wasPressedThisFrame&&!overUI&&Hit(p)){
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            int c=DesktopWindow.Menu();if(c>=1&&c<=6)SetPose(c-1);if(c==99)Application.Quit();
#else
            SetPose(3);
#endif
        }
        if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame&&!Application.isEditor&&(!chat||!chat.IsTyping))Application.Quit();
    }
    bool Hit(Vector2 p){return Physics.Raycast(Camera.main.ScreenPointToRay(p),out var hit,100)&&hit.transform.IsChildOf(avatar.transform);}
}
