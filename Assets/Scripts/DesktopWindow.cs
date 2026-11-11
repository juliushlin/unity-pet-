using System;
using System.Runtime.InteropServices;
using UnityEngine;
public static class DesktopWindow
{
    [StructLayout(LayoutKind.Sequential)] struct LastInput{public uint size,time;}
    [DllImport("user32.dll")] static extern bool GetLastInputInfo(ref LastInput input);
    public static double IdleSeconds(){var i=new LastInput{size=(uint)Marshal.SizeOf(typeof(LastInput))};return GetLastInputInfo(ref i)?unchecked((uint)Environment.TickCount-i.time)/1000.0:0;}
    [StructLayout(LayoutKind.Sequential)] struct Point{public int x,y;}
    [StructLayout(LayoutKind.Sequential)] struct Rect{public int left,top,right,bottom;}
    [StructLayout(LayoutKind.Sequential)] struct Margins{public int left,right,top,bottom;}
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    delegate bool EnumProc(IntPtr window,IntPtr arg);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc callback,IntPtr arg);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint pid);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h,int index);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h,int index,int value);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int z,uint flags);
    [DllImport("user32.dll")] static extern bool SetLayeredWindowAttributes(IntPtr h,uint key,byte alpha,uint flags);
    [DllImport("user32.dll")] static extern bool GetCursorPos(out Point p);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h,out Rect r);
    [DllImport("dwmapi.dll")] static extern int DwmExtendFrameIntoClientArea(IntPtr h,ref Margins m);
    [DllImport("user32.dll")] static extern IntPtr CreatePopupMenu();
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern bool AppendMenu(IntPtr menu,uint flags,UIntPtr id,string text);
    [DllImport("user32.dll")] static extern int TrackPopupMenu(IntPtr menu,uint flags,int x,int y,int reserved,IntPtr window,IntPtr rect);
    [DllImport("user32.dll")] static extern bool DestroyMenu(IntPtr menu);
    static IntPtr hwnd; static Point anchor; static Rect origin;
    public static void Setup(){uint id=(uint)System.Diagnostics.Process.GetCurrentProcess().Id;EnumWindows((w,a)=>{GetWindowThreadProcessId(w,out var p);if(p==id&&IsWindowVisible(w)){hwnd=w;return false;}return true;},IntPtr.Zero);if(hwnd==IntPtr.Zero){Debug.LogError("Pet window not found");return;} SetWindowLong(hwnd,-16,unchecked((int)0x80000000));int ext=GetWindowLong(hwnd,-20)|0x80000;bool inspect=Array.IndexOf(Environment.GetCommandLineArgs(),"--show-in-taskbar")>=0;ext=inspect?(ext&~0x80)|0x40000:ext|0x80;SetWindowLong(hwnd,-20,ext); var m=new Margins{left=-1,right=-1,top=-1,bottom=-1};int dwm=DwmExtendFrameIntoClientArea(hwnd,ref m);bool layered=SetLayeredWindowAttributes(hwnd,0,255,1); SetWindowPos(hwnd,new IntPtr(-1),Math.Max(0,Screen.currentResolution.width-550),Math.Max(0,Screen.currentResolution.height-880),520,820,0x40|0x20);Debug.Log("Pet transparent window: hwnd="+hwnd+", DWM="+dwm+", layered="+layered);}
    public static void BeginDrag(){GetCursorPos(out anchor);GetWindowRect(hwnd,out origin);}
    public static void Drag(ref bool moved){GetCursorPos(out var p); int dx=p.x-anchor.x,dy=p.y-anchor.y;if(Math.Abs(dx)+Math.Abs(dy)>8)moved=true;if(moved)SetWindowPos(hwnd,new IntPtr(-1),origin.left+dx,origin.top+dy,0,0,0x11);}
    public static int Menu(){var menu=CreatePopupMenu(); string[] labels={"陪伴","挥挥手","开心跳跳","害羞","打个小盹","摸摸头"};for(int i=0;i<6;i++)AppendMenu(menu,0,(UIntPtr)(i+1),labels[i]);AppendMenu(menu,0x800,UIntPtr.Zero,null);AppendMenu(menu,0,(UIntPtr)99,"退出桌宠");GetCursorPos(out var p);int r=TrackPopupMenu(menu,0x100|2,p.x,p.y,0,hwnd,IntPtr.Zero);DestroyMenu(menu);return r;}
}
