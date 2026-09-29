# Android 浮动摇杆适配

日期：2026-09-29

分支：`release/v1.0.0-multiplatform`

## 真机发现

第一版 Android 测试 APK 构建成功并可安装启动，但玩家无法移动。

这次结果应记录为：

- APK 构建：通过
- APK 安装和启动：通过
- Android 触摸移动：失败
- Android 完整真机验收：未通过

## 根因

`PlayerMovement` 读取 Input System 的 `Player/Move` 动作。该动作已有键盘、
手柄和摇杆绑定，但触摸屏没有天然的二维移动向量。触摸屏上的一次按压只
提供位置和按下状态，不能直接代替键盘的 WASD，因此仅将项目构建为 APK
不会自动得到手机移动操作。

## 修复设计

新增运行时生成的 Android 浮动摇杆：

1. 游戏未开始、暂停、升级选择和结算时隐藏并停止输入。
2. 正常游戏时，在没有按钮覆盖的屏幕区域任意位置按下即可建立摇杆原点。
3. 半透明底盘和摇杆柄只在按压期间显示。
4. 拖动距离被限制为固定半径，并归一化为 `Vector2`。
5. 方向值通过 Input System 的虚拟 `<Gamepad>/leftStick` 发送给既有
   `Player/Move` 动作，因此复用原移动逻辑、边界限制和移速升级。
6. 松手、暂停或打开升级界面时立即发送零向量，防止角色继续移动。
7. 移动触摸层位于普通 UI 后方，开始、暂停、升级和重开按钮优先响应。
8. Windows 正式构建不会生成触摸层，原有键盘操作保持不变。

## 修改文件

- `Assets/Scripts/MobileControlsOverlay.cs`
- `Assets/Scripts/MobileJoystickTouchArea.cs`
- `Assets/Scripts/GameManager.cs`
- `Assets/Scripts/UpgradeController.cs`

## 当前验证

- [x] 独立 C# 编译通过
- [x] 编译结果为 0 warnings、0 errors
- [ ] Unity Editor 中按下并拖动后玩家移动
- [ ] 松手后玩家立即停止
- [x] 开始、暂停、升级和重开按钮仍可点击
- [x] 新 APK 在 Android 真机上可移动
- [ ] 横屏两个方向都能正常操作
- [ ] 连续运行至少 5 分钟无闪退或明显掉帧

## 真机复测结论

2026-09-29，用户将加入浮动摇杆后的新版 APK 安装到 Android 真机并完成
实际试玩，确认移动和主要游戏流程没有明显问题。因此 Android 状态从
“仅构建通过”更新为“真机试玩通过”。

Unity Editor 鼠标模拟、左右两个横屏方向逐项测试以及明确计时的 5 分钟
压力测试没有单独证据，继续保留为未勾选，不能把未执行项目写成已执行。
第 14 关 9.5 会从最终 `main` 重新构建，并完成发布标签前的最后复测。
