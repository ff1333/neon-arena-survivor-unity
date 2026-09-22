# Neon Arena Rebuild 脚本职责卡

这份文件基于当前学习工程的真实代码整理，共覆盖 `Assets/Scripts` 下 16 个 C# 文件。它是复习底稿，不是需要逐字背诵的标准答案。

每看完一张卡，关闭本文档并用自己的话回答三件事：这个脚本只负责什么、它从哪里拿到数据、它把结果交给谁。然后打开对应代码核对。能脱稿讲清正常流程和一个边界情况，才算真正完成这张卡。

## 1. PlayerMovement

- 挂在哪个 GameObject：`Main` 场景的 `Player`。
- 单一职责：读取玩家移动输入，通过 `Rigidbody2D` 移动 Player，并把位置限制在正交相机可视范围内。
- Inspector 输入：`moveSpeed`、`InputActionAsset inputActions`、`edgePadding`。
- 运行时输入：`Player/Move` 动作产生的 `Vector2`；主相机的正交尺寸、宽高比和位置；升级系统传入的移速增量。
- 自己保存的状态：`body`、`moveAction`、`mainCamera`、当前帧的 `input`、可被升级修改的 `moveSpeed`。
- 输出、事件或副作用：调用 `Rigidbody2D.MovePosition` 改变玩家位置；`AddMoveSpeed` 修改后续移动速度；输入配置缺失时输出错误日志。
- Unity 生命周期方法及调用时机：`Awake` 缓存组件、相机和 Input Action；`OnEnable` 启用动作；`OnDisable` 禁用动作；`Update` 每渲染帧读取输入；`FixedUpdate` 每个物理步长移动并限制边界。
- 依赖哪些其他组件：同对象的 `Rigidbody2D`；Inspector 中的 Input Actions 资产；带 `MainCamera` 标签的正交相机。
- 一个正常流程：按 W -> Input System 产生向上向量 -> `Update` 保存并限制向量长度 -> `FixedUpdate` 计算下一位置 -> 按相机边界 Clamp -> `MovePosition` 移动。
- 一个边界情况：没有配置 `inputActions` 时 `moveAction` 保持空，`Update` 提前返回，不再抛出空引用；相机缺失时仍能移动，但不会进行画面边界限制。
- 如果删除这个脚本，游戏会怎样：Player 仍存在，但键盘输入不会使其移动，移速升级也失去调用目标。
- 核心问题：输入在 `Update` 读取，是因为 Input System 状态按渲染帧更新；刚体在 `FixedUpdate` 移动，是为了与固定物理步长一致，减少碰撞和运动抖动。输入先缓存再供物理帧使用，连接了两种更新频率。
- 面试讲法：我把输入采样和物理移动分开，并用相机 `orthographicSize` 与 `aspect` 动态算边界，所以改变 Game 视图比例后边界仍能适配。

## 2. Health

- 挂在哪个 GameObject：`Player`，以及 `Enemy` Prefab。
- 单一职责：作为所有可受伤对象的生命值真相来源，统一处理扣血、治疗、最大生命、重置和死亡通知。
- Inspector 输入：`maxHealth`。
- 运行时输入：`TakeDamage`、`Heal`、`AddMaxHealth`、`ResetHealth` 的数值参数。
- 自己保存的状态：`Current` 当前血量和 `maxHealth`；`Max`、`IsDead` 是由状态计算出的只读属性。
- 输出、事件或副作用：血量变化时发布 `Changed(current, max)`；第一次有效扣到 0 时发布 `Died`。
- Unity 生命周期方法及调用时机：`Awake` 在实例初始化时把当前血量设为最大血量。
- 依赖哪些其他组件：不直接依赖其他组件；调用者通过方法和事件与它交互。
- 一个正常流程：Projectile 调用 `TakeDamage(25)` -> Health 校验伤害 -> 扣减并限制最低为 0 -> 发布 `Changed` 更新 HUD -> 到 0 时发布 `Died` -> EnemyController 或 GameManager 响应死亡。
- 一个边界情况：对象已经死亡或伤害小于等于 0 时直接返回，避免重复发布死亡；治疗不会超过最大血量；最大生命升级只接受正数，并同时增加当前血量。
- 如果删除这个脚本，游戏会怎样：玩家和敌人没有统一生命状态，子弹无法造成有效伤害，死亡、HUD 血条、敌人回池和结算链路都会断开。
- 核心问题：由 Health 判断死亡，而不是 Projectile 判断，因为 Health 才知道扣血前后的真实状态。以后近战、陷阱或持续伤害也只需调用 `TakeDamage`，不会在每种伤害源里重复死亡规则。
- 面试讲法：我让 Health 负责状态和事件，伤害源只表达“造成多少伤害”，这样降低了伤害来源与死亡处理之间的耦合。

## 3. EnemyController

- 挂在哪个 GameObject：`Enemy` Prefab。
- 单一职责：控制一个被取出的敌人追踪玩家、按间隔造成接触伤害，并在死亡时生成经验和归还对象池。
- Inspector 输入：`moveSpeed`、`contactDamage`、`attackInterval`。
- 运行时输入：Spawner 通过 `Spawn` 传入玩家 `Transform` 和经验池；触发器回调传入碰撞对象；自身 Health 发布死亡事件。
- 自己保存的状态：`body`、`health`、`poolMember`、当前 `target`、`experiencePool` 和 `nextAttackTime`。
- 输出、事件或副作用：移动敌人；调用玩家 `Health.TakeDamage`；从经验池取经验物；发布静态 `DiedGlobally`；把自己 Release 回敌人池。
- Unity 生命周期方法及调用时机：`Awake` 缓存组件并订阅自身死亡；`OnEnable` 为复用实例重置攻击时间和血量；`FixedUpdate` 追踪玩家；`OnTriggerStay2D` 检查接触伤害；`OnDestroy` 退订事件。
- 依赖哪些其他组件：同对象的 `Rigidbody2D`、`Health`、`PoolMember`、触发器 Collider；运行时传入的 Player Transform 和 ExperiencePool。
- 一个正常流程：Spawner 取出 Enemy 并调用 `Spawn` -> Enemy 朝玩家移动 -> 接触玩家后按冷却扣血 -> 被子弹打到 0 -> `HandleDied` 生成经验 -> 通知击杀统计 -> 回池。
- 一个边界情况：没有目标时 `FixedUpdate` 提前返回；经验池为空时仍会发布死亡并回池，只是不生成经验；攻击冷却阻止 `OnTriggerStay2D` 每个物理帧都扣血。
- 如果删除这个脚本，游戏会怎样：敌人对象仍可能被生成，但不会追踪、攻击、生成经验或在死亡后正确回池。
- 核心问题：追踪依赖 Player Transform 和 Rigidbody2D；接触伤害依赖 Player 标签与 Health；死亡释放依赖自身 Health 事件、ExperiencePool 和 PoolMember。
- 面试讲法：敌人复用时不能依赖构造函数重置状态，因此我在 `OnEnable` 和 `Spawn` 中恢复血量、攻击计时及运行时引用。

## 4. EnemySpawner

- 挂在哪个 GameObject：`Main` 场景的 `EnemySpawner`。
- 单一职责：根据存活时间决定何时、每波生成多少敌人，并在固定竞技场可视边缘选择合法出生点。
- Inspector 输入：EnemyPool、ExperiencePool；起始/最小生成间隔、每秒缩短量、增加波次数的时间、每波上限；边缘余量、与玩家最小距离、随机尝试次数。
- 运行时输入：`Time.time`；带 Player 标签的对象；主相机位置、正交尺寸和宽高比；随机数。
- 自己保存的状态：`player`、`mainCamera`、`nextSpawnTime`、`runStartTime`。
- 输出、事件或副作用：从 EnemyPool 取出敌人，设置出生位置，并通过 `EnemyController.Spawn` 注入玩家目标和经验池。
- Unity 生命周期方法及调用时机：`Start` 查找玩家和相机并校验；`OnEnable` 在一局开始时重置难度计时；`Update` 检查生成时间并执行一波生成。
- 依赖哪些其他组件：EnemyPool、ExperiencePool、EnemyController、Player 标签、正交 Main Camera。
- 一个正常流程：游戏开始启用 Spawner -> `OnEnable` 记录时间 -> 到达 `nextSpawnTime` -> 根据 elapsed 算间隔和波次数 -> 在四条边中随机选点 -> 距离玩家足够远 -> 从池中取敌人并配置。
- 一个边界情况：找不到 Player 或没有正交主相机时输出错误并禁用脚本；多次随机都离玩家太近时，使用玩家相对相机中心的对角边缘作为保底点。
- 如果删除这个脚本，游戏会怎样：敌人池仍存在，但不会有新敌人进入游戏，也不会形成随时间增长的生存压力。
- 核心问题：Prefab 的实例化/复用入口和生成位置由 Spawner 统一管理，EnemyController 只处理单个敌人的行为；这样生成规则变化不会污染敌人自身逻辑。
- 面试讲法：我发现玩家限制在固定画面、敌人却按无限世界圆环生成会造成界外击杀和不可拾取经验，因此把生成规则统一成固定竞技场边缘。

## 5. PlayerShooter

- 挂在哪个 GameObject：`Player`。
- 单一职责：按射击间隔寻找射程内最近敌人，并从子弹池取出子弹向目标方向发射。
- Inspector 输入：ProjectilePool、`fireInterval`、`projectileSpeed`、`damage`、`range`。
- 运行时输入：当前时间；所有带 Enemy 标签的激活对象位置；伤害和攻速升级数值。
- 自己保存的状态：`nextFireTime`，以及会被升级修改的 `damage`、`fireInterval`。
- 输出、事件或副作用：从 ProjectilePool 取对象；调用 `Projectile.Fire`；升级时修改伤害或射击间隔。
- Unity 生命周期方法及调用时机：`Update` 每帧检查冷却、搜索目标并尝试发射。
- 依赖哪些其他组件：ProjectilePool、Projectile；场景中的 Enemy 标签。
- 一个正常流程：冷却结束 -> `FindNearestEnemy` 遍历激活敌人 -> 找到射程内最近者 -> 设置下次射击时间 -> 从池中取子弹 -> 把方向、速度和伤害交给 Projectile。
- 一个边界情况：没有射程内敌人时不发射且不消耗冷却；攻速升级把间隔限制为最小 0.08 秒，避免零或负间隔；伤害最低为 1。
- 如果删除这个脚本，游戏会怎样：游戏只剩移动和躲避，玩家没有击杀敌人的手段。
- 核心问题：比较 `sqrMagnitude` 与 `range * range`，避免只为比较大小而进行平方根运算。自动射击是品类设计选择，让玩家把注意力放在走位、资源拾取和构筑选择上，并不是输入系统失效。
- 面试讲法：当前 `FindGameObjectsWithTag` 适合小规模学习版本，但敌人规模继续扩大时，我会维护激活敌人注册表或使用空间查询，避免频繁全场遍历和数组分配。

## 6. Projectile

- 挂在哪个 GameObject：`Projectile` Prefab。
- 单一职责：保存一次发射参数、沿方向移动、检测命中，并在超时、出界或命中后归还对象池。
- Inspector 输入：`lifeTime`、`viewportMargin`。
- 运行时输入：`Fire` 传入的方向、速度、伤害；当前时间；主相机视口；触发器碰撞对象。
- 自己保存的状态：PoolMember、Main Camera、标准化方向、速度、伤害和释放时间。
- 输出、事件或副作用：改变自身 Transform 位置；对 Enemy Health 调用 `TakeDamage`；调用 `PoolMember.Release`。
- Unity 生命周期方法及调用时机：`Awake` 缓存 PoolMember 和相机；`Update` 移动并检查寿命/视口；`OnTriggerEnter2D` 处理命中。
- 依赖哪些其他组件：PoolMember、Collider2D；Enemy 标签和 Health；Main Camera。
- 一个正常流程：PlayerShooter 从池中取子弹并调用 `Fire` -> 子弹逐帧移动 -> 触发 Enemy Collider -> 给 Health 扣血 -> 回池等待复用。
- 一个边界情况：未命中时，到寿命或离开视口边缘就回池，避免在界外击杀；命中带 Enemy 标签但没有 Health 的对象时不扣血，但仍回池；同帧重复释放由 PoolMember 拦截。
- 如果删除这个脚本，游戏会怎样：子弹 Prefab 可以被取出，但不会获得发射数据、移动、造成伤害或自动回池。
- 核心问题：使用 `Release` 而不是 `Destroy`，因为频繁创建和销毁子弹会增加 CPU、内存分配和垃圾回收压力；池化保留实例并重置运行时状态。
- 面试讲法：我把越界判断放在 Projectile 自身，因为它最清楚当前位置和生命周期；统一回池入口则交给 PoolMember 防重复。

## 7. GameObjectPool

- 挂在哪个 GameObject：`ProjectilePool`、`EnemyPool`、`ExperiencePool` 各挂一个实例。
- 单一职责：预创建并保存可复用对象，按需提供激活实例，并接收归还实例。
- Inspector 输入：对应的 `prefab` 和 `initialSize`。
- 运行时输入：`Get(position, rotation)` 的生成位置和旋转；`Release(instance)` 的归还对象。
- 自己保存的状态：FIFO 队列 `available`，其中只应包含当前未激活、可再次取用的实例。
- 输出、事件或副作用：启动时 Instantiate 预热对象；Get 时出队、定位并激活；Release 时停用、归到池节点下并入队；池为空时扩容。
- Unity 生命周期方法及调用时机：`Awake` 调用 `Warm(initialSize)` 完成预热。
- 依赖哪些其他组件：Prefab；每个池成员上的 PoolMember，没有时会在创建实例时自动添加。
- 一个正常流程：Warm 创建 24 个 Enemy 并全部入队 -> Spawner 调用 Get -> 一个对象出队并激活 -> Enemy 死亡后 Release -> 停用并重新入队。
- 一个边界情况：Prefab 未配置时输出错误且不预热；队列耗尽时 `CreateInstance` 动态扩容；重复 Release 会破坏队列不变量，因此由 PoolMember 在入口处阻止。
- 如果删除这个脚本，游戏会怎样：Spawner 和 Shooter 无法获得敌人、经验和子弹；若改成 Instantiate/Destroy，功能可能恢复但高频运行会产生更多分配和 GC。
- 核心问题：Warm 保证初始队列包含指定数量的停用对象；Get 保证返回对象不再位于 available 且处于激活状态；Release 保证对象停用、归属池节点并只入队一次。
- 面试讲法：这个池是按 Prefab 分开的通用池。它解决高频生命周期成本，但仍需 PoolMember 维护“一个实例只能归还一次”的不变量。

## 8. PoolMember

- 挂在哪个 GameObject：`Projectile`、`Enemy`、`ExperiencePickup` Prefab；GameObjectPool 创建对象时缺失则自动添加。
- 单一职责：记录实例属于哪个池，并为实例提供幂等的 `Release` 入口。
- Inspector 输入：无。
- 运行时输入：池创建实例时调用 `SetOwner`；业务脚本调用 `Release`。
- 自己保存的状态：`owner` 和当前激活周期是否已经释放的 `isReleased`。
- 输出、事件或副作用：首次 Release 时调用所属池的 `Release(gameObject)`；缺少 owner 时输出错误。
- Unity 生命周期方法及调用时机：`OnEnable` 在每次从池中重新激活时把 `isReleased` 重置为 false。
- 依赖哪些其他组件：所属 GameObjectPool。
- 一个正常流程：池创建实例并设置 owner -> Get 激活对象 -> OnEnable 清除释放标记 -> 命中或死亡调用 Release -> 标记后归还 owner。
- 一个边界情况：Projectile 可能在同一帧同时触发出界和碰撞，两次调用 Release；第二次因为 `isReleased` 已为 true 而直接返回，避免同一对象两次入队。
- 如果删除这个脚本，游戏会怎样：业务对象不知道应该归还哪个池，也无法统一阻止重复归还。
- 核心问题：重复 Release 会让同一个 GameObject 在队列里出现多次，后续可能被同时 Get 给两个逻辑使用；布尔标记保护了池队列的一致性。
- 面试讲法：PoolMember 相当于实例侧的所有权句柄，使 Projectile、EnemyController 等业务脚本不必各自保存具体池实现。

## 9. ExperiencePickup

- 挂在哪个 GameObject：`ExperiencePickup` Prefab。
- 单一职责：在玩家进入磁吸范围后靠近玩家，并在首次接触时增加经验后回池。
- Inspector 输入：`value`、`magnetRadius`、`moveSpeed`。
- 运行时输入：`Configure` 传入本次经验值；Player 标签对象的位置；触发器碰撞对象。
- 自己保存的状态：PoolMember、玩家 Transform、经验值和 `collected` 标记。
- 输出、事件或副作用：移动自身；调用 `PlayerProgress.AddExperience`；领取后回池。
- Unity 生命周期方法及调用时机：`Awake` 缓存 PoolMember；`OnEnable` 为每次复用重置 collected 并重新寻找 Player；`Update` 处理磁吸移动；`OnTriggerEnter2D` 处理领取。
- 依赖哪些其他组件：PoolMember、Collider2D；Player 标签和 PlayerProgress。
- 一个正常流程：敌人死亡 -> 经验池 Get -> `Configure(1)` -> 玩家进入半径后经验物靠近 -> 触发 Player -> 增加经验 -> 回池。
- 一个边界情况：找不到 Player 时不移动；非 Player 碰撞直接忽略；`collected` 在加经验前置为 true，防止同一激活周期多次触发导致重复加经验。
- 如果删除这个脚本，游戏会怎样：经验物可能显示在场景中，但不会吸附、增加经验或正确回池。
- 核心问题：吸附只负责移动，真正领取由触发器确认；`collected` 与 PoolMember 的防重复分别保护“经验只加一次”和“对象只回池一次”。
- 面试讲法：池化对象的状态必须在 `OnEnable` 重置，这里最关键的是 collected，否则第二次取出后可能永远无法领取。

## 10. PlayerProgress

- 挂在哪个 GameObject：`Player`。
- 单一职责：维护玩家等级、当前经验和下一级需求，并在数值变化或升级时发布事件。
- Inspector 输入：`firstLevelRequirement`。
- 运行时输入：经验物调用 `AddExperience(amount)`。
- 自己保存的状态：`Level`、`Experience`、`ExperienceToNextLevel`。
- 输出、事件或副作用：发布 `ExperienceChanged`、`LevelChanged`、`LevelUpRequested`；升级时更新等级和下一等级需求。
- Unity 生命周期方法及调用时机：`Awake` 初始化首级需求；`Start` 在订阅者已完成启用后广播初始 UI 数值。
- 依赖哪些其他组件：不直接持有其他组件；ExperiencePickup 调用它，HUD 与 UpgradeController 订阅它。
- 一个正常流程：拾取经验 -> `AddExperience(1)` -> 累加到阈值 -> 扣除本级需求并升级 -> 下一级需求乘 1.35 向上取整 -> 通知等级和升级选择 -> 通知经验条。
- 一个边界情况：非正经验直接忽略；超过阈值的多余经验通过减法保留，而不是清零。当前代码一次调用最多升一级，因为使用 `if` 而不是 `while`；当前每个经验物值为 1，所以正常玩法中影响有限，大额奖励加入前需要扩展。
- 如果删除这个脚本，游戏会怎样：经验拾取没有接收者，等级、经验条和三选一升级都不会工作。
- 核心问题：保留多余经验避免玩家因刚好超过阈值而损失资源，也使数值增长连续、公平。
- 面试讲法：Progress 只维护成长状态并发事件，不直接操作 UI 或弹窗，因此以后替换 HUD 或升级界面不需要改经验计算。

## 11. PlayerUpgradeData

- 挂在哪个 GameObject：不挂载；它是 ScriptableObject 类型，当前对应 `Assets/Data` 下五个升级 `.asset`。
- 单一职责：保存一项升级的显示文本、效果类型和数值，作为可复用、可在 Inspector 编辑的配置资产。
- Inspector 输入：每个资产的 `title`、`description`、`effectType`、`value`。
- 运行时输入：无；UpgradeController 读取资产，PlayerUpgradeApplier 消费资产。
- 自己保存的状态：序列化配置数据；通过只读属性对外暴露。
- 输出、事件或副作用：本身不执行玩法副作用，只提供数据；`CreateAssetMenu` 在 Unity 菜单中提供创建入口。
- Unity 生命周期方法及调用时机：无自定义生命周期方法。
- 依赖哪些其他组件：`UpgradeEffectType` enum；不依赖场景对象。
- 一个正常流程：设计者在 Project 中创建升级资产 -> 配置 Damage 和 10 -> UpgradeController 随机选到它并显示文字 -> Applier 根据类型给 Shooter 增加伤害。
- 一个边界情况：资产配置了负数或不合理值时，具体组件的 Add/Reduce 方法会做部分下限保护，但数据资产本身不校验所有设计规则；发布前需要检查配置。
- 如果删除这个脚本，游戏会怎样：五个升级资产失去类型，UpgradeController 不能以数据驱动方式读取升级内容。
- 核心问题：使用数据资产而不是把内容写死在按钮中，可以新增/调整升级而不修改 UI 流程代码，并让同一份配置被不同界面或系统复用。
- 面试讲法：ScriptableObject 在这里承担静态配置，而玩家本局已经获得的加成仍保存在运行时组件中，避免修改共享资产污染后续游戏。

## 12. PlayerUpgradeApplier

- 挂在哪个 GameObject：`Player`。
- 单一职责：把 `PlayerUpgradeData` 描述的效果路由到真正拥有该数值的玩家组件。
- Inspector 输入：PlayerMovement、PlayerShooter、Health 引用。
- 运行时输入：`Apply(PlayerUpgradeData data)`。
- 自己保存的状态：三个组件引用；不保存升级数值副本。
- 输出、事件或副作用：根据 effectType 调用加伤害、减射击间隔、治疗、加移速或加最大生命的方法。
- Unity 生命周期方法及调用时机：无自定义生命周期方法；只在玩家选择升级时被 UpgradeController 调用。
- 依赖哪些其他组件：PlayerMovement、PlayerShooter、Health、PlayerUpgradeData、UpgradeEffectType。
- 一个正常流程：玩家点击 Damage 选项 -> UpgradeController 传入对应 Data -> Applier 匹配 Damage -> 调用 `PlayerShooter.AddDamage(value)`。
- 一个边界情况：新增 enum 项但忘记在 switch 中添加 case 时，升级看似可选却不会生效；data 或组件引用为空时当前实现会抛空引用，因此 Inspector 配置必须完整。
- 如果删除这个脚本，游戏会怎样：三选一仍可显示和点击，但选择无法真正改变玩家属性。
- 核心问题：它隔离了 UI 流程与玩家具体组件。UpgradeController 不需要知道伤害存在 Shooter、生命存在 Health。
- 面试讲法：这是一个简单的应用层路由器；效果继续增多时可以进一步改为策略对象，但当前五种效果下 switch 更直接、可读。

## 13. UpgradeController

- 挂在哪个 GameObject：当前挂在 `Player`。
- 单一职责：响应升级请求，从配置池中随机生成三个不重复选项，控制升级面板，并把玩家选择交给 Applier。
- Inspector 输入：PlayerProgress、PlayerUpgradeApplier、UpgradePanel、三个 Button、三个 TMP Label、可用 PlayerUpgradeData 数组。
- 运行时输入：`LevelUpRequested` 事件；按钮点击索引；随机数。
- 自己保存的状态：三个 `currentChoices`；面板激活状态可通过 `IsOpen` 查询。
- 输出、事件或副作用：修改三个标签文字；显示/隐藏并置顶升级面板；暂停/恢复 `Time.timeScale`；调用 Applier。
- Unity 生命周期方法及调用时机：`Awake` 初始隐藏面板并注册按钮监听；`OnEnable` 订阅升级请求；`OnDisable` 退订。
- 依赖哪些其他组件：PlayerProgress、PlayerUpgradeApplier、Button、TMP_Text、升级数据资产。
- 一个正常流程：Progress 发布 LevelUpRequested -> 复制 availableUpgrades -> 随机抽取后从临时列表移除，保证三项不重复 -> 写入按钮标签 -> 面板置顶并暂停 -> 玩家点击 -> Apply -> 关闭面板并恢复时间。
- 一个边界情况：升级资产少于 3、按钮或标签不是 3 个时输出错误并停止打开；面板未打开或索引越界时 Select 直接返回；循环中用局部 `CapturedIndex`，避免所有 lambda 最终引用同一个循环变量。
- 如果删除这个脚本，游戏会怎样：等级仍能增加，但不会出现三选一，也无法把升级数据应用给玩家。
- 核心问题：打开选择时暂停玩法，避免玩家阅读和选择期间仍被敌人攻击；面板 `SetAsLastSibling` 保证绘制和点击优先级高于 HUD。
- 面试讲法：我用“复制列表、抽一个删一个”的方式保证本次三个候选不重复，而不修改原始配置数组。

## 14. HudController

- 挂在哪个 GameObject：`GameCanvas > HUD`。
- 单一职责：把玩家生命、经验、等级、计时、击杀和状态转换成 UI 控件显示。
- Inspector 输入：Health Slider、Experience Slider、Timer/Level/Kill/State 四个 TMP_Text。
- 运行时输入：GameManager 调用 `Initialize`、`SetTimer`、`SetKills`、`SetState`；Health 和 PlayerProgress 事件。
- 自己保存的状态：初始化后保存 Player Health 和 PlayerProgress 引用，以便接收事件和退订。
- 输出、事件或副作用：修改 Slider 的 maxValue/value 和 TMP 文本。
- Unity 生命周期方法及调用时机：没有在 Awake 自动查找业务对象；由 GameManager 在 Start 调用 Initialize；`OnDestroy` 退订 Health 和 Progress 事件。
- 依赖哪些其他组件：Unity UI Slider、TMP_Text、Health、PlayerProgress。
- 一个正常流程：GameManager 初始化 HUD -> HUD 订阅数值事件并主动刷新初始值 -> 玩家受伤 -> Health 发布 Changed -> HUD 只更新血条。
- 一个边界情况：Initialize 如果被重复调用会重复订阅，当前 GameManager 只调用一次；销毁时先判空再退订，避免初始化未完成时出现空引用。
- 如果删除这个脚本，游戏会怎样：玩法逻辑仍可能运行，但玩家看不到血量、经验、等级、时间、击杀和状态反馈。
- 核心问题：事件驱动避免 HUD 每帧轮询很少变化的血量和经验，也让 Health/Progress 不需要反向依赖具体 UI。
- 面试讲法：高频计时由 GameManager 主动设置，低频状态变化用事件更新；我根据数据变化频率选择通信方式，而不是所有内容都塞进 Update。

## 15. GameManager

- 挂在哪个 GameObject：`Main` 场景的 `GameManager`。
- 单一职责：编排一局游戏的顶层状态，包括开始、暂停、升级暂停协作、死亡结算、重开、计时、击杀统计和最高纪录。
- Inspector 输入：PlayerMovement、PlayerShooter、EnemySpawner、Player Health、PlayerProgress、UpgradeController、HudController；开始/暂停/结算面板和文本；三个按钮。
- 运行时输入：Enter、Escape、R 键；按钮点击；Player Health.Died；EnemyController.DiedGlobally；时间。
- 自己保存的状态：`hasStarted`、`isPaused`、`isGameOver`、`elapsedTime`、`killCount`。
- 输出、事件或副作用：启停玩法组件；设置 `Time.timeScale`；切换 UI 面板；更新 HUD；通过 PlayerPrefs 读写最高时间和击杀；重载当前场景。
- Unity 生命周期方法及调用时机：`Awake` 建立初始暂停状态和按钮监听；`Start` 初始化 HUD、订阅事件并读取纪录；`Update` 处理快捷键和计时；`OnDestroy` 退订死亡事件。
- 依赖哪些其他组件：几乎所有顶层玩法控制器、UI 和 SceneManager，但不直接实现它们的内部算法。
- 一个正常流程：场景加载后暂停并显示 Start -> Enter 调用 StartRun -> 启用移动/射击/刷怪 -> 统计时间和击杀 -> 玩家死亡 -> 停用玩法、保存纪录、显示结算 -> R 重载 Main。
- 一个边界情况：重复 StartRun、重复死亡和无效 Resume 都通过状态条件提前返回；升级面板打开时 Escape 不切换暂停，避免两个暂停来源互相覆盖；销毁时必须退订静态敌人死亡事件。
- 如果删除这个脚本，游戏会怎样：各子系统仍存在，但没有统一的一局状态：开局面板、暂停、结算、纪录、重开和组件启停都会失去协调。
- 核心问题：它应该管理跨系统的“局状态”和编排，不应该管理玩家移动公式、子弹碰撞、敌人寻路或经验阈值等局部细节。
- 面试讲法：GameManager 是有限状态的协调者。当前用布尔值适合状态较少的版本；流程继续扩大时可改成显式 enum 状态机，减少非法组合。

## 16. UpgradeEffectType

- 挂在哪个 GameObject：不挂载；它是一个 C# `enum` 类型。
- 单一职责：定义当前系统允许的升级效果有限集合：Damage、FireRate、Heal、MoveSpeed、MaxHealth。
- Inspector 输入：PlayerUpgradeData 资产通过下拉框选择其中一个枚举值。
- 运行时输入：PlayerUpgradeApplier 的 switch 读取枚举值。
- 自己保存的状态：无实例状态；每个枚举成员对应一个整数标识，但业务代码使用可读名称。
- 输出、事件或副作用：本身没有副作用，只为配置和分支提供类型安全的值。
- Unity 生命周期方法及调用时机：无。
- 依赖哪些其他组件：无；PlayerUpgradeData 和 PlayerUpgradeApplier 依赖它。
- 一个正常流程：升级资产选择 `MoveSpeed` -> 运行时 Data 暴露该值 -> Applier 的对应 case 调用 Movement.AddMoveSpeed。
- 一个边界情况：在 enum 中新增成员后，旧资产序列化值通常仍保留，但必须同步检查 Applier switch 和所有配置；随意调整成员顺序可能让按整数序列化的数据含义变化。
- 如果删除这个脚本，游戏会怎样：PlayerUpgradeData 和 PlayerUpgradeApplier 无法编译，升级类型也失去受限集合。
- 核心问题：enum 比字符串或魔法数字更易读，并在拼写错误和未支持值上提供编译期帮助。
- 面试讲法：枚举适合当前固定且很小的效果集合；若效果需要独立复杂行为，可以把每个效果升级为多态策略或 ScriptableObject 行为。

## 完成标准

不要用“我已经有这份文档”作为完成依据。按下面顺序自测：

1. 随机抽五个脚本，每个在 60 秒内脱稿讲清职责、输入、输出和依赖。
2. 对五个核心问题脱稿回答：Update/FixedUpdate、Health 死亡归属、平方距离、对象池不变量、事件驱动 HUD。
3. 在 Unity Hierarchy 或 Project 中亲自指出每个 MonoBehaviour 挂载位置、三个 Pool 的 Prefab，以及五个升级资产。
4. 任选一条正常流程，在代码中逐个找到对应方法，不靠猜测补箭头。
5. 用自己的表达修改每张卡的“面试讲法”；能被追问两层仍讲清楚，才在第 10 关清单中勾选。
