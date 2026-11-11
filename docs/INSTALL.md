# 安装与首次启动

## 1. 安装工具

1. 用 [Unity Hub](https://unity.com/download) 安装 Unity **6000.6.8f1**，包含 Windows Build Support。用此版本打开本项目。
2. 从 [Node.js 官网](https://nodejs.org/)安装 Node.js 24 LTS。关闭旧终端后重新打开，运行 `node -v`、`npm -v` 验证。
3. 按 [Codex CLI 官方说明](https://developers.openai.com/codex/cli/)安装 Codex，运行 `codex login`，由本人完成登录；用 `codex --version` 验证。
4. 安装 [Git](https://git-scm.com/downloads/win) 与 [uv](https://docs.astral.sh/uv/getting-started/installation/)。Coplay 后端需要 Python 3.10+，uv 可管理隔离依赖。
5. 按 [Unity CLI 官方说明](https://unity.com/products/unity-cli)安装 Unity CLI，用 `unity --version` 验证。注意：Unity CLI 的 unity.exe 与编辑器的 Unity.exe 是两个程序。

## 2. 获取项目

```powershell
git clone https://github.com/juliushlin/unity-pet-.git
```

Unity Hub → 添加已有项目 → 选择克隆目录。等待导入和编译完成。登录或条款弹窗由本人处理。

## 3. 工具路径

小月优先读取以下环境变量，其次搜索 PATH 和常见安装位置：

| 变量 | 内容 |
|---|---|
| XIAOYUE_NODE_PATH | node.exe 完整路径 |
| XIAOYUE_CODEX_PATH | Codex CLI codex.exe 完整路径 |
| XIAOYUE_UNITY_PATH | Unity CLI unity.exe 完整路径 |
| XIAOYUE_UVX_PATH | uvx.exe 完整路径 |

可以在 Windows“编辑账户的环境变量”中设置。修改后重启 Unity Hub 和 Unity。不要把个人路径或账号令牌提交到源码。Codex 必须是完整安装，不能只拷贝主程序而漏掉辅助程序。

## 4. MCP

项目已嵌入 `Packages/com.coplaydev.unity-mcp`。不要重复安装同一个包。首次后端启动可能需要联网下载：

```powershell
uvx --from mcpforunityserver==10.0.0 mcp-for-unity --help
```

脚本编译后 `XiaoyueCoplayBootstrap` 自动启动本地 stdio 桥接。详细说明见 [MCP.md](MCP.md)。

## 5. 启动小月

1. 打开 `Assets/Scenes/Xiaoyue.unity`，运行可预览。
2. Unity 菜单 **桌宠 → 导出 Windows 桌宠**，在本机生成 `Builds/Xiaoyue/Xiaoyue.exe`，并自动唤醒桌宠。这是本机运行所需构建，仓库不分发安装包。
3. 后续打开这个 Unity 项目会自动启动桌宠；关闭编辑器会停止桌宠。
4. 点击脚下铅笔展开聊天，输入要求并发送。
5. 若场景意外损坏，可用 **小月 → 恢复原画与铅笔聊天场景**。此操作重建小月自己的场景，请先保存其他工作。

在编辑器内部使用 **小月 → 协作窗口**，无需先构建桌面程序。第一次发送前确保 Codex 已登录、工具路径可用，且 MCP 桥接已启动。
