# v1.2.0 验证索引

最终核对：2026-10-04。执行者为自动化工具与 Codex 画面审查，不冒充作者手动试玩。

| 检查 | 结果 | 证据 |
| --- | --- | --- |
| 编辑器发布边界 | 24 项通过 | ../../devlogs/18-v1.2.0-build-verification.md |
| Windows 构建 | 成功，0 错误、1 告警 | Windows-build-summary.txt |
| WebGL 构建 | 成功，0 错误、1 告警 | WebGL-build-summary.txt |
| Android 构建 | 成功，0 错误、26 告警 | Android-build-summary.txt |
| 独立 Windows Player | 开始、武器、战斗、暂停与小窗口画面已检查 | v1.2.0-*.log，../../images/ |
| Edge 浏览器 | 开始→武器→移动→暂停；未捕获 JS/Unity 异常 | neon-web-result.json，../../images/neon-web-*.png |
| 附件校验 | 三个包 SHA256 与清单一致 | SHA256SUMS.txt |
| Android 真机 | 待作者安装新版验收 | 不沿用旧版本结论 |

Windows 画面检查 1280×720，武器页另查 854×480；浏览器 viewport 1280×800，游戏画布 960×540。
敌人外观检查由脚本摆放三种敌人，不代表正式刷怪时序。没有把烟测写成长时间性能或平衡验收。

## 告警解释

Windows/WebGL 各 1 条：`MobileJoystickTouchArea.mouseActive` 的 CS0414。
鼠标模拟读取逻辑只在 UNITY_EDITOR 编译，正式包中字段仍被赋值但没有读取；这是编译告警，不是构建失败。
本次保留最终已验证包与源代码一致，不在打包后偷偷改变代码。

Android 的 26 条经 Unity 保存的 BuildReport 逐条核对：

- 6 条 `Failed to download any source lists!`：SDK 清单联网检查失败。
- 18 条 `Still waiting for package manifests to be fetched remotely.`：SDK 等待远端清单。
- 1 条 Diagnostics Data / Debug Symbols 提示：未配置用于崩溃解析的完整调试符号，影响崩溃报告符号解析。
- 1 条上述 CS0414。

原始日志还包含 URP 包内 Terrain ShaderGraph 模板的 AddPass/BaseMapGen 依赖导入提示，
它们不是这个 2D 游戏中已验证画面发生粉色材质错误的证据，也不能计成额外的 BuildReport 告警。
Windows 与浏览器画面已实际审查；手机渲染效果仍以新包真机验收为准。

`Size` 是 Unity BuildReport 的统计值，不等于附件压缩大小；以 Packages/v1.2.0 下实际文件为准。

## 复现浏览器检查

需要 Node.js、Playwright 与本机 Edge：

```text
node tools/verify-web.cjs neon Builds/WebGL/v1.2.0 Builds/WebVerification
```

脚本启动临时本地 HTTP 服务并在结束时关闭；截图还需要人工查看。
