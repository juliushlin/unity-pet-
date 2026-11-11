# 小月 · Unity + MCP + Codex 游戏制作伙伴

小月是 Windows 上的 Unity 协作桌宠：银白长发、长耳、红色衣装，恢复原来的 Q 版插画外观，以网格变形和程序补间带来轻微立体感、呼吸和发梢摆动。它属于 **2.5D 插画动画**，并非完整 3D 模型或逐帧骨骼动画。

## 如何使用

在 Unity 中打开项目，小月会随编辑器启动；点击脚下的铅笔条，展开小巧的暖白、绯红聊天框。再次点击收起。打开聊天时小月会低头，收起后抬头。输入制作要求并点击发送，即交给本机已登录的 Codex，通过 Coplay MCP 与当前 Unity 项目协作。

- 启动时播放一次打开动画，之后固定待机；180 秒无鼠标键盘操作后睡眠。
- 点击角色触发摸头、开心跳跃、害羞或挥手；拖动移动窗口，右键选择动作。
- Unity 菜单 **小月 → 协作窗口** 提供相同的铅笔聊天入口。
- 桌宠随当前 Unity 编辑器退出，不设置 Windows 开机启动。
- 点击铅笔只展开界面，只有主动点击发送才会提交要求。

## 教程

1. [安装与首次启动](docs/INSTALL.md)
2. [日常使用与协同制作](docs/USAGE.md)
3. [Unity MCP + Codex 连接](docs/MCP.md)
4. [常见问题](docs/TROUBLESHOOTING.md)
5. [架构和数据说明](docs/ARCHITECTURE.md)
6. [本机验证记录](docs/VALIDATION.md)

## 要求与来源

本机验证使用 Windows、Unity **6000.6.8f1**、Unity CLI **1.0.0-beta.17**、Node.js **24.22.2**、Codex CLI **0.167.0-alpha.3.2**、Coplay MCP **10.0.0**。其他版本需自行验证。

[CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp) 是第三方集成，固定 v10.0.0，嵌入包保留 MIT 上游许可证。Unity Pipeline 是备用开发通道。

角色图集为基于用户视觉参考生成的插画，没有导入游戏提取模型。原始参考图片、账号凭据、用户对话与缓存不包含在发布源码中。项目公开可见不等于授予全部美术素材无限使用许可。
