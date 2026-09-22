# Neon Arena Rebuild 运行数据流

本文档基于当前学习工程的真实代码，补全第 10.4 节要求的四条运行链路。它回答四个问题：谁调用谁、通过什么方式调用、传递什么数据、引用从哪里来。

这是一份理解底稿，不是背诵稿。阅读一条链路后关闭文档，在纸上只写对象名和方法名重新画一遍；再对每个箭头说明通信方式和引用来源。能够不看本文档讲清楚，才算完成第 10.4 节。

## 先区分三种通信方式

| 通信方式 | 谁决定调用时机 | 当前项目示例 |
|---|---|---|
| Unity 回调 | Unity 引擎在特定时机自动调用 | `Update`、`FixedUpdate`、`OnTriggerEnter2D`、`OnTriggerStay2D` |
| 直接方法调用 | 调用方明确持有对象引用并调用方法 | `projectile.Fire(...)`、`health.TakeDamage(...)`、`pool.Get(...)` |
| C# 事件通知 | 发布者只宣布事件，已订阅的方法同步响应 | `Health.Died`、`PlayerProgress.LevelUpRequested`、`EnemyController.DiedGlobally` |

Inspector 引用、`GetComponent`、按标签查找和运行时传参解决的是“如何得到对方的引用”；直接调用或事件解决的是“得到引用以后如何通信”。不要把这两类问题混在一起。

## 流程一：按下 W 到玩家位置变化

### 一行总览

```text
按下 W
-> Input System 更新 Player/Move 动作
-> PlayerMovement.Update 读取 Vector2
-> input 字段缓存并限制长度
-> PlayerMovement.FixedUpdate 计算下一位置
-> 根据相机边界 Clamp
-> Rigidbody2D.MovePosition
-> Player 位置变化并由 Unity 渲染到画面
```

### 逐箭头拆解

| 步骤 | 从谁到谁 | 通信方式 | 数据 | 引用来源 |
|---:|---|---|---|---|
| 1 | 键盘 W -> Input System | 设备输入，由 Input System 采集 | W 的按下状态 | Unity Input System 管理键盘设备 |
| 2 | Input System -> `Player/Move` | Input Action 绑定解析 | 向上的二维输入，通常为 `(0, 1)` | `Assets/InputSystem_Actions.inputactions` 中的动作和绑定 |
| 3 | Unity -> `PlayerMovement.Update` | Unity 每渲染帧自动回调 | 无方法参数 | Player 上启用的 PlayerMovement 组件 |
| 4 | `moveAction` -> `ReadValue<Vector2>` | PlayerMovement 主动读取 Input Action | `Vector2` | `inputActions` 由 Inspector 赋值；`Awake` 用 `FindAction("Player/Move")` 得到 moveAction |
| 5 | 读取结果 -> `input` | 脚本内部赋值 | 被 `ClampMagnitude(..., 1)` 限制后的 `Vector2` | PlayerMovement 自己的字段 |
| 6 | Unity -> `PlayerMovement.FixedUpdate` | Unity 按固定物理步长自动回调 | 无方法参数 | Player 上启用的 PlayerMovement 组件 |
| 7 | `input` -> `nextPosition` | 脚本内部计算 | `body.position + input * moveSpeed * fixedDeltaTime`，结果为 `Vector2` | body 来自同对象 `GetComponent<Rigidbody2D>()` |
| 8 | Camera -> 边界 Clamp | 直接读取相机属性并计算 | 相机位置、`orthographicSize`、`aspect`、`edgePadding` | `Awake` 通过 `Camera.main` 获得主相机 |
| 9 | PlayerMovement -> Rigidbody2D | 直接调用 `MovePosition(nextPosition)` | `Vector2` 下一位置 | Rigidbody2D 在 `Awake` 通过 GetComponent 获得 |
| 10 | Rigidbody2D -> Transform/画面 | Unity 物理和渲染系统处理 | 更新后的世界坐标 | Unity 引擎内部同步 |

### 为什么分成 Update 和 FixedUpdate

- `Update` 跟随渲染帧，适合及时读取 Input System 当前状态。
- `FixedUpdate` 跟随固定物理步长，适合调用 Rigidbody2D 的移动 API。
- `input` 字段是两者之间的桥梁：渲染帧先保存输入，物理帧再消费最近一次输入。
- `Time.fixedDeltaTime` 把“每秒速度”换算成“这一个物理步长移动多少”，避免物理频率改变后速度改变。

### 需要会回答的边界情况

- 同时按 W 和 D 时，原始向量长度大于 1；`ClampMagnitude` 防止斜向移动比单轴更快。
- 玩家到达边缘时，下一位置先被计算，再由 `Mathf.Clamp` 限制到相机可见范围。
- `inputActions` 未配置时，Awake 输出错误，`moveAction` 为空，Update 提前返回。
- Main Camera 缺失时仍会调用 MovePosition，但不会执行画面边界限制。

## 流程二：敌人进入射程到敌人死亡回池

### 一行总览

```text
敌人进入射程
-> PlayerShooter.Update 到达射击冷却
-> FindNearestEnemy 返回最近 Enemy GameObject
-> ProjectilePool.Get 取出并激活子弹
-> Projectile.Fire 保存方向、速度、伤害和释放时间
-> Projectile.Update 移动
-> Projectile.OnTriggerEnter2D 命中 Enemy
-> Enemy Health.TakeDamage
-> Health.Changed
-> Health.Died
-> EnemyController.HandleDied
-> ExperiencePool.Get 生成经验物
-> EnemyController.DiedGlobally 通知 GameManager 增加击杀
-> Enemy PoolMember.Release
-> EnemyPool.Release 停用并入队
-> Projectile PoolMember.Release
-> ProjectilePool.Release 停用并入队
```

### 逐箭头拆解

| 步骤 | 从谁到谁 | 通信方式 | 数据 | 引用来源 |
|---:|---|---|---|---|
| 1 | Unity -> `PlayerShooter.Update` | Unity 每帧回调 | 当前 `Time.time` | Player 上启用的 PlayerShooter |
| 2 | Update -> `FindNearestEnemy` | 同脚本直接调用 | 隐式使用 `range` 和 Player 位置 | PlayerShooter 自己的字段和 Transform |
| 3 | Enemy 标签 -> FindNearestEnemy | Unity 静态查询 API | 激活的 `GameObject[]` | `FindGameObjectsWithTag("Enemy")` |
| 4 | 候选敌人 -> 最近敌人 | 脚本内部比较 | `(enemy.position - player.position).sqrMagnitude`，类型为 `float` | 每个 Enemy 的 Transform |
| 5 | PlayerShooter -> ProjectilePool | 直接调用 `Get(position, rotation)` | Player 的 `Vector3` 位置和 `Quaternion.identity` | `projectilePool` 由 Inspector 赋值 |
| 6 | ProjectilePool -> Projectile 对象 | 队列出队或动态创建，然后激活 | 返回 `GameObject` | Pool 自己维护的 `Queue<GameObject>` 和 Prefab |
| 7 | PlayerShooter -> `Projectile.Fire` | 直接方法调用 | `Vector2 direction`、`float speed`、`float damage` | Projectile 通过刚取出的 GameObject.GetComponent 获得 |
| 8 | Unity -> `Projectile.Update` | Unity 每帧回调 | `Time.deltaTime` | 激活的 Projectile 组件 |
| 9 | Projectile -> Transform | 直接修改位置 | 标准化方向 × 速度 × deltaTime | Fire 保存的运行时字段 |
| 10 | Unity 2D Physics -> `OnTriggerEnter2D` | Unity 触发器回调 | `Collider2D other` | Projectile 与 Enemy 的 Collider2D/物理配置 |
| 11 | Projectile -> Enemy Health | `TryGetComponent` 后直接调用 `TakeDamage(damage)` | `float damage` | Health 从命中的 Enemy Collider 所在对象取得 |
| 12 | Health -> `Changed` 订阅者 | C# 事件同步通知 | 当前血量和最大血量，两个 `float` | 订阅关系；Enemy 当前没有 HUD 订阅者也不影响发布 |
| 13 | Health -> `Died` 订阅者 | C# 事件同步通知 | 无参数 | EnemyController 在 Awake 订阅自身 Health.Died |
| 14 | `Died` -> `EnemyController.HandleDied` | 事件处理方法同步执行 | 无参数，读取敌人当前位置 | 同对象事件订阅 |
| 15 | EnemyController -> ExperiencePool | 直接调用 `Get`，再调用 `Configure(1)` | 敌人死亡位置、单位经验值 `int 1` | experiencePool 由 EnemySpawner.Spawn 在运行时传入 |
| 16 | EnemyController -> GameManager | 发布静态 `DiedGlobally` 事件 | 无参数，代表一次敌人死亡 | GameManager.Start 订阅该静态事件 |
| 17 | GameManager -> HudController | 事件处理后直接调用 `SetKills` | 更新后的 `int killCount` | hud 由 Inspector 赋值 |
| 18 | EnemyController -> Enemy PoolMember | 直接调用 `Release()` | 当前 Enemy GameObject | PoolMember 来自同对象 GetComponent |
| 19 | Enemy PoolMember -> EnemyPool | 直接调用 owner.Release | 当前 `GameObject` | owner 在 Pool 创建实例时通过 SetOwner 注入 |
| 20 | Projectile -> Projectile PoolMember | OnTriggerEnter2D 末尾直接调用 | 当前 Projectile GameObject | PoolMember 来自同对象 GetComponent |
| 21 | Projectile PoolMember -> ProjectilePool | 直接调用 owner.Release | 当前 `GameObject` | owner 在 Pool 创建实例时注入 |

### 为什么比较平方距离

只需要判断哪个敌人更近，不需要得到真实米数。`sqrMagnitude` 省去平方根计算，拿它与 `range * range` 比较即可。两边都平方后，大小关系不变。

### 对象池在链路中的不变量

- `Get` 之后：对象已经离开 available 队列、位置已设置、处于激活状态。
- `Release` 之后：对象已停用、回到池节点下、只在 available 队列中出现一次。
- PoolMember 的 `isReleased` 阻止同一个对象因多个条件在同一帧重复入队。
- Projectile 未命中时，也会因寿命结束或离开相机视口而回池。

### 需要会回答的边界情况

- 没有射程内敌人时不发射，也不更新 `nextFireTime`。
- 子弹命中非 Enemy 标签对象时直接忽略，不释放；其后靠寿命或视口边界回池。
- Enemy 已死亡时再次收到伤害，Health 因 `IsDead` 提前返回，不会再次发布 Died。
- experiencePool 为空时不生成经验，但仍发布全局死亡事件并释放敌人。

## 流程三：敌人死亡到玩家升级三选一

### 一行总览

```text
Enemy Health.Died
-> EnemyController.HandleDied
-> ExperiencePool.Get 在死亡位置生成 ExperiencePickup
-> ExperiencePickup.Configure 设置经验值
-> 玩家进入 magnetRadius 后 ExperiencePickup.Update 吸附
-> Unity 调用 ExperiencePickup.OnTriggerEnter2D
-> PlayerProgress.AddExperience
-> 经验达到阈值：扣除本级需求、Level + 1、计算新阈值
-> PlayerProgress.LevelChanged 通知 HUD
-> PlayerProgress.LevelUpRequested 通知 UpgradeController
-> UpgradeController.ShowChoices 随机抽三个不重复升级
-> 填写三个按钮文字、显示面板、Time.timeScale = 0
-> 玩家点击一个 Button
-> UpgradeController.Select
-> PlayerUpgradeApplier.Apply
-> 对应玩家组件修改属性
-> 关闭面板、Time.timeScale = 1
```

### 逐箭头拆解

| 步骤 | 从谁到谁 | 通信方式 | 数据 | 引用来源 |
|---:|---|---|---|---|
| 1 | Enemy Health -> EnemyController | `Died` C# 事件 | 无参数 | EnemyController.Awake 订阅同对象 Health |
| 2 | EnemyController -> ExperiencePool | 直接调用 `Get` | 死亡位置 `Vector3` 和旋转 | experiencePool 由 EnemySpawner 在 `Spawn` 时传入 |
| 3 | EnemyController -> ExperiencePickup | GetComponent 后直接调用 `Configure(1)` | `int` 经验值 | 刚从经验池取得的 GameObject |
| 4 | ExperiencePickup.OnEnable -> Player | 标签查找 | Player Transform | `FindGameObjectWithTag("Player")` |
| 5 | Unity -> `ExperiencePickup.Update` | Unity 每帧回调 | Player 与经验物的位置 | 激活的 ExperiencePickup |
| 6 | ExperiencePickup -> Transform | 距离满足后直接修改位置 | `MoveTowards` 计算的新位置 | 缓存的 Player Transform |
| 7 | Unity 2D Physics -> `OnTriggerEnter2D` | Unity 触发器回调 | `Collider2D other` | Player 与 Pickup 的碰撞配置 |
| 8 | ExperiencePickup -> PlayerProgress | `TryGetComponent` 后直接调用 `AddExperience(value)` | `int value` | 从 Player Collider 所在对象取得 Progress |
| 9 | PlayerProgress -> 自身成长状态 | 脚本内部计算 | Experience、Level、ExperienceToNextLevel，均为 `int` | PlayerProgress 自身字段 |
| 10 | PlayerProgress -> HudController | `LevelChanged(int)` C# 事件 | 新等级 `int` | HUD.Initialize 时订阅 progress.LevelChanged |
| 11 | PlayerProgress -> UpgradeController | `LevelUpRequested` C# 事件 | 无参数，只表达“需要弹出升级” | UpgradeController.OnEnable 订阅 progress |
| 12 | UpgradeController -> availableUpgrades | 本地列表复制、随机选择并移除 | 三个不同的 `PlayerUpgradeData` 引用 | availableUpgrades 由 Inspector 配置 |
| 13 | UpgradeController -> TMP labels | 直接设置 `.text` | Title 和 Description 字符串 | labels 数组由 Inspector 配置 |
| 14 | UpgradeController -> UpgradePanel/时间 | 直接调用和属性赋值 | 显示面板；`Time.timeScale = 0f` | upgradePanel 由 Inspector 配置 |
| 15 | Unity UI Button -> `Select(index)` | Button.onClick 事件 | 捕获的按钮索引 `int` | Awake 为三个 Button 注册监听 |
| 16 | UpgradeController -> PlayerUpgradeApplier | 直接调用 `Apply(currentChoices[index])` | `PlayerUpgradeData` | applier 由 Inspector 配置 |
| 17 | Applier -> Movement/Shooter/Health | 根据 enum 分支后直接调用 | Data.Value，类型为 `float` | 三个组件引用由 Inspector 配置 |
| 18 | UpgradeController -> 面板/时间 | 直接调用和属性赋值 | 隐藏面板；`Time.timeScale = 1f` | 自己持有的面板引用和 Unity Time |
| 19 | PlayerProgress -> HudController | `ExperienceChanged(int, int)` C# 事件 | 剩余经验和新阈值 | HUD.Initialize 时订阅 |
| 20 | ExperiencePickup -> PoolMember | 直接调用 Release | 当前 Pickup GameObject | 同对象 GetComponent；owner 由池注入 |

### 顺序上容易忽略的细节

- `AddExperience` 中先发布 `LevelChanged`，再发布 `LevelUpRequested`，最后发布 `ExperienceChanged`。
- C# 事件在当前实现中是同步调用：`LevelUpRequested?.Invoke()` 返回前，`ShowChoices` 已执行并把 `Time.timeScale` 设为 0。
- 经验超过阈值时使用减法保留余量，而不是把 Experience 清零。
- 当前使用 `if`，一次 AddExperience 最多升一级；以后加入大额经验奖励时，应考虑用 `while` 连续处理多次升级。
- ShowChoices 把可用升级复制到临时 List，每选一个就 Remove，因此一组三选一不会重复。
- `CapturedIndex` 为每个按钮保存独立索引，避免三个 lambda 都使用循环结束后的同一个值。

### 为什么升级时暂停玩法

升级选项需要阅读和决策。如果敌人在选择期间继续移动和攻击，玩家会因操作 UI 而受到惩罚。将 `Time.timeScale` 设为 0 会暂停依赖缩放时间的移动、射击和物理流程；UI Button 仍能响应点击。

## 流程四：玩家死亡到保存最高纪录和重开

### 一行总览

```text
Enemy 与 Player 保持接触
-> Unity 调用 EnemyController.OnTriggerStay2D
-> 攻击冷却通过
-> 取得 Player Health
-> Health.TakeDamage(contactDamage)
-> Current 降到 0
-> Health.Changed 通知 HUD 更新血条
-> Health.Died 通知 GameManager.HandlePlayerDied
-> GameManager 进入 Game Over 状态
-> 禁用移动、射击、刷怪并暂停时间
-> 读取旧 BestTime/BestKills
-> 与本局 elapsedTime/killCount 取最大值
-> PlayerPrefs 写入并 Save
-> 填写结算文字并显示 GameOverPanel
-> 玩家按 R 或点击 Restart
-> GameManager.RestartRun
-> 恢复 timeScale 并重新加载 Main 场景
```

### 逐箭头拆解

| 步骤 | 从谁到谁 | 通信方式 | 数据 | 引用来源 |
|---:|---|---|---|---|
| 1 | Unity 2D Physics -> `EnemyController.OnTriggerStay2D` | Unity 物理回调 | Player 的 `Collider2D` | Enemy 与 Player 的 Trigger/Collider 配置 |
| 2 | EnemyController -> Player Health | `TryGetComponent` 后直接调用 `TakeDamage(contactDamage)` | `float contactDamage` | Health 从进入触发器的 Player 对象取得 |
| 3 | Health -> HudController | `Changed(float, float)` C# 事件 | 当前血量、最大血量 | HUD.Initialize 订阅 Player Health |
| 4 | Health -> GameManager | `Died` C# 事件 | 无参数 | GameManager.Start 订阅 Inspector 中的 playerHealth |
| 5 | GameManager -> 顶层局状态 | 事件处理方法内部赋值 | `isGameOver = true` | GameManager 自己的字段 |
| 6 | GameManager -> Movement/Shooter/Spawner | 直接设置 `enabled = false` | 三个布尔启停状态 | 三个组件引用由 Inspector 配置 |
| 7 | GameManager -> Unity Time | 直接设置 `Time.timeScale = 0f` | `float 0` | Unity 静态 Time API |
| 8 | GameManager -> PlayerPrefs | 静态 API 读取 | `BestTime: float`、`BestKills: int` | 本地持久化键值存储 |
| 9 | 本局数据 -> 新纪录 | `Mathf.Max` 直接计算 | elapsedTime 为 float；killCount 为 int | GameManager 在运行期间累计 |
| 10 | GameManager -> PlayerPrefs | SetFloat、SetInt、Save | 新最高时间和击杀数 | Unity PlayerPrefs API |
| 11 | GameManager -> finalStatsText | 直接设置 `.text` | 格式化后的字符串 | TMP_Text 由 Inspector 配置 |
| 12 | GameManager -> GameOverPanel/HUD | SetActive 和 `hud.SetState` | 显示结算；提示 R/Restart | 引用由 Inspector 配置 |
| 13A | 键盘 R -> `RestartRun` | GameManager.Update 读取 Input System 后直接调用 | R 的本帧按下状态 | `Keyboard.current` |
| 13B | Restart Button -> `RestartRun` | Unity UI Button.onClick 事件 | 无参数 | Awake 为 restartButton 注册监听 |
| 14 | RestartRun -> Time/SceneManager | 直接设置并调用场景加载 | timeScale 设为 1；当前场景 buildIndex | Unity Time 与 SceneManager API |
| 15 | SceneManager -> Main 场景重建 | Unity 场景系统 | Main 场景中保存的对象和序列化引用 | Build Profiles 中 Main 必须启用且有有效索引 |

### elapsedTime 和最高纪录从哪里来

- 游戏开始后，GameManager.Update 只在“已开始、未死亡、未手动暂停、升级面板未打开”时累加 `Time.unscaledDeltaTime`。
- 使用 unscaledDeltaTime 让计时逻辑不直接受 `timeScale` 数值影响，但外层条件仍明确排除暂停和升级选择。
- 每次敌人发布 `DiedGlobally`，GameManager 的 `HandleEnemyDied` 把 `killCount` 加一并更新 HUD。
- 死亡时分别用 `Mathf.Max(旧纪录, 本局成绩)`，所以较差的一局不会覆盖更好的历史纪录。
- `PlayerPrefs.Save()` 明确把更新写入本地存储；重载场景后，GameManager.Start 再读出并显示。

### 需要会回答的边界情况

- `HandlePlayerDied` 首先检查 `isGameOver`，防止同一死亡被重复结算和重复保存。
- Enemy 的 `nextAttackTime` 防止 `OnTriggerStay2D` 每个物理帧都造成伤害。
- Game Over 时 movement、shooter、spawner 均禁用，避免暂停界面背后继续产生逻辑。
- Restart 前先把 timeScale 恢复为 1；否则新场景加载后可能仍处于暂停状态。
- `OnDestroy` 退订 Player Health 和静态 Enemy 死亡事件，避免重载场景后旧 GameManager 残留订阅。

## 四条链路的引用来源总表

| 引用获取方式 | 当前实例 |
|---|---|
| Inspector 序列化引用 | GameManager 的 gameplay/UI 引用、PlayerShooter.projectilePool、UpgradeController 的按钮和升级资产 |
| 同对象 `GetComponent` | PlayerMovement -> Rigidbody2D、Projectile -> PoolMember、EnemyController -> Health/PoolMember |
| 碰撞对象 `TryGetComponent` | Projectile -> Enemy Health、EnemyController -> Player Health、Pickup -> PlayerProgress |
| 标签查找 | EnemySpawner 和 ExperiencePickup 查找 Player；PlayerShooter 查询所有激活 Enemy |
| `Camera.main` | PlayerMovement、EnemySpawner、Projectile 获取 Main Camera |
| 运行时方法参数注入 | EnemySpawner 调用 EnemyController.Spawn 传入 target 和 experiencePool |
| 对象池 owner 注入 | GameObjectPool.CreateInstance 调用 PoolMember.SetOwner(this) |
| C# 事件订阅 | Health.Died、Health.Changed、LevelChanged、LevelUpRequested、DiedGlobally |

## 脱稿自测

按以下规则完成，不能只阅读本文档：

1. 在纸上分别画出四条“一行总览”，只允许写类名、方法名和箭头。
2. 随机挑每条链路中的三个箭头，说明它是 Unity 回调、直接调用还是 C# 事件。
3. 再说明调用方如何得到被调用方引用：Inspector、GetComponent、标签查找、碰撞参数、运行时参数或事件订阅。
4. 给每条链路讲一个提前 return 或边界情况。
5. 不看本文档录一段 3 至 5 分钟口述；在哪个箭头卡住，就回到对应脚本核对，而不是背这一段文字。

能完成以上五项后，再在第 10 关最终验收中勾选“四条运行数据流能在白纸上画出”。
