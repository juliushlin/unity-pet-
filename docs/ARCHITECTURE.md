# 架构和数据说明

主人 → 小月铅笔聊天界面 → 共享任务服务 → 本机 Codex CLI → Coplay MCP Python 服务 → Unity MCP 插件 → 当前项目。

`XiaoyueSpriteAvatar` 使用原插画图集和可变形网格提供呼吸、发梢摆动、轻微摇摆与低头动作；`PetMotion` 管理打开、固定待机、反应与睡眠。没有保留已被替换的几何 3D 角色。

`XiaoyueChatUI` 是桌面界面；`XiaoyueStudio` 是编辑器界面。它们共用 `XiaoyueChatService` 与 `Tools/codex-bridge.js`，将提示通过标准输入交给 Codex，使用项目路径固定目标。`PetEditorLink` 管理跟随编辑器的桌宠进程。

任务写入 `Temp/CodexBridge`，对话写入 `Library/Xiaoyue-chat.txt`。这些都是本地文件，不应提交。需要删除本地聊天记录时请先停止任务、关闭 Unity，再删除对应文件。

输入及必要项目上下文会发往当前 Codex 模型服务；本地界面不代表模型离线运行。保留 Codex 自带权限限制。仓库排除账号文件、个人路径配置、用户对话、日志、构建与原始参考图。
