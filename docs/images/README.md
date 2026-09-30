# 游戏截图与证据清单

这里保存 Neon Arena Rebuild 学习工程的真实运行截图，不使用参考工程或 AI 生成图片冒充运行结果。

## 当前文件

| 文件 | 内容 | 来源 |
|---|---|---|
| `01-start-screen.png` | 开始界面 | `v1.0.0` WebGL 正式构建 |
| `02-gameplay.png` | 自动射击与敌人 | `v1.0.0` WebGL 正式构建 |
| `03-level-up.png` | 升级三选一 | `v1.0.0` WebGL 正式构建 |
| `04-combat-feedback.png` | 多敌人与命中反馈 | `v1.0.0` WebGL 正式构建 |
| `05-profiler-before.png` | 对象池前 Profiler 记录 | 原始文件 `profiler-before-pooling.png` |
| `06-profiler-after.png` | 对象池后 Profiler 记录 | 原始文件 `profiler-after-pooling.png` |
| `07-game-over.png` | 结算与最高纪录 | `v1.0.0` WebGL 正式构建 |

`04-combat-feedback.png` 只证明实战中存在多敌人与命中反馈，不把白色受击闪烁误写成独立敌人类型。敌人三类型的实现与自动回归证据见 `docs/devlogs/09-enemy-audio-feedback.md`。

Profiler 两张图来自不同玩法规模的提交，不能用单帧数值计算固定性能提升比例。解释边界见 `docs/optimization/01-object-pooling-profiler.md`。

## 图片保存在哪里

统一保存到下面这个文件夹，不放进 `Assets`：

```text
D:\software\documents\unity_games\projects\learning\NeonArenaRebuild\docs\images
```

Windows 截图并保存的完整操作：

1. 在 Unity 中把需要的游戏画面显示出来。
2. 按 `Win + Shift + S`，鼠标拖动框选 Game 窗口。
3. 点击屏幕右下角出现的截图通知，打开“截图工具”。
4. 在截图工具中按 `Ctrl + S`。
5. 在保存窗口的地址栏粘贴上面的完整文件夹路径，然后按 Enter。
6. 在“文件名”中填写清单要求的名字，例如 `01-start-screen.png`。
7. 文件类型选择 PNG，然后点击“保存”。

如果右下角通知没有出现，可以打开 Windows“截图工具”，点击“新建”后截图，再按第 4 至第 7 步保存。

## 拍摄前统一设置

1. Unity 打开 `Assets/Scenes/Main.unity`。
2. Game 视图选择 `16:9`，建议使用 `1280 x 720`。
3. 关闭 Scene、Inspector 等会遮挡 Game 画面的浮动窗口。
4. 每次截图前确认画面没有红色报错提示。
5. 使用 Windows `Win + Shift + S` 截取 Game 区域，不要截整个桌面和个人信息。
6. 将图片保存到本目录，必须使用下面的固定文件名。

## 需要重新采集时

### 01-start-screen.png

刚点击 Play、尚未开始游戏。必须看到标题、Start 按钮和最高纪录；计时应为 00:00。

### 02-gameplay.png

运行约 60 秒。画面应同时看到玩家、多个敌人、子弹、经验物和 HUD，玩家不能越界。

### 03-level-up.png

升级三选一面板打开时拍摄。三个按钮必须集中在中央、文字完整、无重叠。

### 04-combat-feedback.png

运行约 120 秒。用于证明难度曲线已经增加每波数量，画面应比开局有明显更高压力，但 UI 仍清晰。

### 07-game-over.png

玩家死亡后的结算画面。必须看到本局时间、击杀数、最高纪录和 Restart 按钮。

### 可选：08-console-clean.png

停止 Play 后打开 Console，确认红色 Error 数量为 0。只截 Console 区域和清楚可见的计数。

## 作品集图片验收

- [x] 游戏截图均为本人学习工程正式构建的真实画面。
- [x] 没有使用参考工程或 AI 生成图片冒充运行结果。
- [x] README 引用的文件名与真实文件完全一致。
- [x] 图片中没有手机号、邮箱、Token、用户名目录等敏感信息。
- [x] 已逐张打开检查，没有全黑、错误裁切或窗口边框。
