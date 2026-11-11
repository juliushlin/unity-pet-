# 常见问题

## Unity 还在导入或被弹窗挡住

等待资源导入、编译完成；登录、语言、条款等窗口需本人确认。不能把主线程尚未就绪误判成 MCP 插件没有安装。

## Coplay 显示 HTTP 连接失败

本项目采用本地 stdio 桥接。旧配置缓存可能仍选择 HTTP；检查 `XiaoyueCoplayBootstrap` 的实际启动模式和 `Library/XiaoyueMcp-state.txt`，不要在未启动 HTTP 后端时反复连接 8080。

## GitHub 直连下载失败

检查本机网络。可以从同一官方仓库下载固定版本归档，将 `MCPForUnity` 放到 Unity 的 `Packages/com.coplaydev.unity-mcp` 作为嵌入包，保留上游 LICENSE；不要去不明镜像下载替代程序。

## Codex 请求很慢

本机曾出现 WebSocket 超时后才切换 HTTPS 的情况，可能额外等待约 1–2 分钟。检查官方诊断结果，不要反复点发送产生多个任务。

## `.sandbox-bin` 或命令被策略拦截

这是执行环境初始化/权限问题，不等同于 Unity 编译错误。不得关闭保护或强行绕过。保留错误记录，改用已授权的 MCP 工具；无法执行的操作须明确告知。

## 桌面黑背景

Windows 透明窗口需 D3D11、禁用 Flip Model Swapchain、透明清屏，以及正确的本窗口 HWND。更换渲染配置后必须重新构建；不能只检查编辑器中的 Game 视图。
