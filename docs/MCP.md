# Unity + Coplay MCP + Codex

## 三者各自做什么

Unity 负责运行项目与编辑资源；Coplay MCP 通过标准协议提供场景、对象、脚本等操作工具；Codex 接收用户需求、选择工具、执行修改并核对结果。小月负责把当前项目和对话交给 Codex。

## 本机当前配置

- Unity 插件：`Packages/com.coplaydev.unity-mcp` 中嵌入的上游 10.0.0 版本。
- Python 后端：`mcpforunityserver==10.0.0`，通过 uvx 的隔离环境运行。
- Codex 连接名：`coplay-unity`。
- 本地模式：stdio。Python 服务通过插件的本地桥接访问 Unity，不向公网暴露 Unity 编辑器端口。
- `XiaoyueCoplayBootstrap` 在项目编译完成后启动 Unity 端桥接，状态写入 `Library/XiaoyueMcp-state.txt`。

在其他电脑可参考以下命令创建 Codex 连接，其中 `项目目录名` 应替换为实际的 Unity 项目名称：

```powershell
codex mcp add coplay-unity --env UNITY_MCP_TELEMETRY_ENABLED=false -- uvx --from mcpforunityserver==10.0.0 mcp-for-unity --transport stdio --default-instance "项目目录名"
```

## 正确验收顺序

1. Unity 无编译错误，Coplay 桥接处于运行状态。
2. MCP 客户端完成 `initialize` 握手，并能发现工具。
3. 通过 `manage_scene` 的 `get_hierarchy` 读取实际场景。
4. 核对实例确实是当前项目；多开 Unity 时不得仅凭“能连接”就修改。
5. 让 Codex 做一项可恢复的场景改动，再读取对象确认结果，撤销测试对象。

配置文件存在、进程运行、协议连通、实际修改成功，是不同的验证层级。尚未完成的层级应明确标记。

## 权限

不要使用关闭保护或跳过审批的参数来解决终端执行失败。若终端被环境策略阻止，使用已授权的 Unity MCP 专用工具；若这些工具也不可用，应报告实际错误并停止依赖它的修改。
