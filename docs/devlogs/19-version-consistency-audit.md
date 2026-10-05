# 2026-10-04：两款作品的版本一致性检查

用户在 3D 游戏截图中发现 v1.0.1，因此同时检查本项目。

- 本项目磁盘 PlayerSettings.bundleVersion、ReleasePipeline.Version 均为 1.2.0。
- 本项目玩法脚本未硬编码显示 v1.0.1；已有 v1.2.0 发布包保留。
- 加入 Editor/ReleaseVersionSync：脚本导入后、进入 Play 前、构建前同步 ReleasePipeline.Version，避免打开的编辑器继续保留旧配置。
- 验证副本执行 ReleasePipeline.RunBoundaryVerification，24 项通过。
- 本次没有修改本项目的游戏运行逻辑、美术、武器数值或发布包，也不重新声称完成新版真机验收。

因此两款最新版本号不同是正常的：本项目 1.2.0，Fogbound Maze 新武器版 1.2.1。
Editor 文件不打入 Player；不需要仅因这个编辑器同步修复重新生成相同的游戏附件。
