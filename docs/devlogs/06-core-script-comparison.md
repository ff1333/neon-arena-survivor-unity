# Core Script Design Comparison

Date: 2026-09-27

Branch: `docs/core-script-comparison`

Learning project baseline: `9c82b21`

## Purpose

This document compares seven core responsibility groups between the reference prototype and my rebuilt project. The goal is to identify concrete architectural tradeoffs rather than claim that one structure is universally better.

## 1. GameObjectPool

### Reference design

- `GameObjectPool.Initialize` receives a prefab and pool size at runtime.
- `GameManager` creates the projectile, enemy and pickup pool GameObjects.
- `Warm` pre-creates inactive objects after initialization.
- `Get` creates another instance when the queue is empty, so exhaustion does not stop gameplay.
- Pool ownership is assigned separately by callers through `PoolMember.SetPool`.

### Rebuilt design

- Each pool is a scene object configured through serialized fields.
- `Awake` calls `Warm`, so the pool is ready before gameplay begins.
- `CreateInstance` adds or finds `PoolMember` and assigns its owner immediately.
- `Release` disables the instance, reparents it under the pool and enqueues it.
- Like the reference, an empty queue expands through `CreateInstance`.

### Tradeoff

Runtime pool creation is flexible and keeps scene setup small. Scene-configured pools make capacities and ownership easier to inspect, but add Inspector dependencies. The rebuilt ownership path is more centralized, while the reference caller has more control over runtime configuration.

### Interview wording

I used a queue-based pool that prewarms scene-configured objects and expands only when exhausted. Every created instance receives one pool owner, and released objects return under the pool hierarchy. This avoids repeated high-frequency creation during steady gameplay, while accepting extra resident memory and reset complexity.

## 2. PoolMember

### Reference design

- Stores a public read-only `Pool` reference.
- `Release` returns to the pool when assigned.
- Missing ownership falls back to disabling the GameObject.
- It does not track whether the same active lifecycle has already been released.

### Rebuilt design

- Stores a private owner and an `isReleased` guard.
- `OnEnable` resets the guard for the next pooled lifecycle.
- Repeated `Release` calls during one lifecycle return immediately.
- A missing owner produces a specific Console error instead of silently hiding the configuration problem.

### Tradeoff

The reference implementation is smaller and tolerant of an unassigned pool. The rebuilt version is safer against duplicate queue entries and easier to diagnose, but relies on correct ownership setup before release.

### Interview wording

`PoolMember` lets a pooled object return itself without knowing which gameplay system spawned it. I added a per-lifecycle release guard because hit, timeout and disable paths can converge in the same frame; without the guard, one instance could be enqueued twice.

## 3. Player responsibilities

### Reference design

- `PlayerController` combines input, movement, aiming, firing, experience and upgrade application.
- It uses the legacy Input Manager and moves by changing `Transform.position`.
- World limits are numeric values inside the controller.
- Targeting calls `FindGameObjectsWithTag` and scans the returned array.
- Weapon values are mutable fields on the player.

### Rebuilt design

- `PlayerMovement` owns Input System sampling and fixed-step Rigidbody2D movement.
- `ArenaBounds` owns reusable world-boundary data.
- `PlayerShooter` owns six runtime weapon slots and one cooldown per equipped weapon.
- `PlayerProgress` owns experience and level-up events.
- `PlayerUpgradeApplier` validates and applies upgrade effects.
- `WeaponDefinition` ScriptableObjects hold pistol, SMG and laser configuration.
- `EnemyRegistry` and `TargetSelector` replace per-scan tag array creation and share one targeting policy.

### Tradeoff

The combined controller is quick to assemble and easy to follow in a small prototype. Splitting responsibilities makes movement, weapons and progression easier to change independently, but creates more scripts and serialized references that must be configured correctly.

### Interview wording

The project outgrew a single player controller when I added six independent weapon slots. I separated movement, shooting, progression and upgrade application so each system owns one kind of state. I use the new Input System in `Update`, cache the input vector, and apply Rigidbody movement in `FixedUpdate` to keep input sampling and physics timing separate.

## 4. EnemyController

### Reference design

- Supports grunt and elite ranks with different health, damage, scale, color and experience.
- Moves by changing the Transform in `Update`.
- `OnTriggerStay2D` applies repeated contact damage through an attack cooldown.
- Death is reported to the singleton `GameManager`, which records the kill and creates the pickup.

### Rebuilt design

- Uses `Rigidbody2D.MovePosition` in `FixedUpdate`.
- A contact hit deals fixed damage once and immediately returns the enemy to its pool.
- `OnEnable` restores health, clears hit state and registers the instance in `EnemyRegistry`.
- `OnDisable` unregisters the instance so targeting only sees active enemies.
- Projectile death obtains an experience pickup from the injected pool and raises `DiedGlobally` for the kill counter.
- A nonlethal player hit requests bounded camera shake before release.

### Tradeoff

The rebuilt enemy has clearer pooled-state reset and supports registry-based targeting. Its one-hit contact behavior matches the revised design, but it currently has less enemy variety than the reference. The static registry and global death event reduce direct scene references but still represent global coupling that should be monitored as the project grows.

### Interview wording

Pooled enemies cannot rely on construction for every spawn, so I reset transient state in `OnEnable` and runtime references in `Spawn`. I also separate projectile kills from contact expiration: only a real kill drops experience and increments the kill counter, so taking damage is not rewarded.

## 5. EnemySpawner

### Reference design

- Spawns on a fixed-radius ring around the player.
- Reduces its interval over time.
- Increases normal-enemy batch size and scales health, speed and experience.
- Periodically creates elite enemies.
- Delegates actual pool access to `GameManager.Instance`.

### Rebuilt design

- Samples a random position inside `ArenaBounds`.
- Rejects candidates that are too close to the player.
- Uses the farthest arena corner when all random attempts fail.
- Spawns a pooled warning marker first; the warning creates the enemy after 1.5 seconds.
- Reduces spawn interval over time but clamps it to a configured minimum.

### Tradeoff

The rebuilt placement is map-aware, telegraphed and less likely to create unavoidable contact damage. The reference progression is currently richer because it scales batches and stats and includes elites. A later iteration could add data-driven enemy waves without removing the rebuilt placement rules.

### Interview wording

I changed spawning from a player-centered ring to bounded arena sampling with a minimum safety distance. Random selection has a fixed attempt count and a deterministic farthest-corner fallback, so it cannot loop forever. A warning object separates position selection from actual enemy activation and gives the player reaction time.

## 6. GameManager

### Reference design

- Uses `GameManager.Instance` as a singleton service.
- Creates pools and routes projectile, enemy and pickup spawning.
- Owns run state, pause, upgrade choices, combo logic, hit feedback, persistence and game-over flow.
- Other systems call it directly for shared operations.

### Rebuilt design

- Does not expose a GameManager singleton.
- Owns start, pause, death, restart, elapsed time, kill count and best-run persistence.
- Scene systems hold explicit references to the pools and collaborators they need.
- `UpgradeController` owns candidate filtering, every-third-level weapon choices, button binding and time-scale handling during selection.
- `GameManager` checks `UpgradeController.IsOpen` to prevent pause and timing conflicts.

### Tradeoff

A singleton makes a compact prototype convenient because every system has one access point. As responsibilities accumulate, that manager becomes a broad dependency and changes for unrelated features converge in one class. The rebuilt version narrows GameManager, but explicit references increase Inspector setup and the project still uses a few static events and registries.

### Interview wording

I kept run-state orchestration in GameManager but moved upgrade selection and pool ownership to their respective systems. This makes the dependencies more explicit and lets weapon rules change without editing the run-state controller. I would still consider a singleton acceptable for a small prototype; the choice depends on scope and coupling, not a blanket rule.

## 7. HudController

### Reference design

- Uses legacy `UnityEngine.UI.Text` components.
- Owns start, pause, upgrade and game-over panels.
- Binds upgrade and run-control buttons.
- Displays messages, combo state and damage flash.
- Polls singleton state in `Update` for timer, kills and combo.
- Uses events for health, experience and level.

### Rebuilt design

- Uses TextMeshPro text components.
- Subscribes to health, experience and level events.
- Receives timer, kill and state text through explicit setter methods.
- Leaves run panels and buttons to `GameManager`.
- Leaves upgrade buttons and candidate presentation to `UpgradeController`.

### Tradeoff

The reference HUD has more presentation features in one place. The rebuilt HUD is a narrower display adapter and is easier to replace or restyle, but coordination is spread across GameManager and UpgradeController. Event subscriptions must be paired with unsubscriptions to avoid stale listeners.

### Interview wording

I keep combat and upgrade rules out of the HUD. Health, experience and level are event-driven because they change discretely, while elapsed time is pushed by GameManager during an active run. The HUD formats values and updates widgets but does not decide gameplay outcomes.

## Interview Summary

The reference project favors a compact prototype architecture: a singleton manager and larger controllers make initial assembly fast and provide richer enemy and feedback features in relatively few files. My rebuilt project favors explicit responsibilities because later requirements introduced bounded-map movement, pooled lifecycle resets, shared target selection, three weapon definitions, six independent cooldowns and filtered upgrade choices.

The rebuilt structure reduces the number of unrelated reasons each core class has to change, but it is not free. It adds scripts, events, static registries and Inspector dependencies. My design goal is therefore not maximum separation; it is to keep ownership clear enough that one gameplay rule can be changed and tested without editing an unrelated UI or run-state class.

## Follow-up Findings

- The reference has elite enemies, batch growth, combo feedback and richer HUD messages that are not yet implemented in the rebuilt project.
- The rebuilt project has stronger spawn telegraphing, duplicate-release protection, map-aware boundaries and independent weapon state.
- `GameObjectPool.cs` in the rebuilt project contains unused namespace imports; they can be removed in a later scoped cleanup rather than mixed into this documentation PR.
- Static `EnemyRegistry` and `EnemyController.DiedGlobally` are pragmatic global access points. If the number of scenes or game modes grows, their lifetime and reset behavior should receive dedicated tests.

## Oral Review Prompts

These prompts are prepared for later interview rehearsal:

1. Why does an object pool still need `PoolMember`?
2. When is a singleton GameManager useful, and when does it become a coupling problem?
3. Why did the rebuilt targeting path replace repeated `FindGameObjectsWithTag` calls?
4. Why is input sampled in `Update` while Rigidbody movement runs in `FixedUpdate`?
5. Which reference-project features are worth reintroducing, and how would they fit the rebuilt boundaries?

## Verification

- [x] Compared all seven responsibility groups against the current source files.
- [ ] Explained every comparison aloud without reading the answer word for word. This rehearsal is intentionally deferred and is not claimed as completed.
- [x] Confirmed that this branch changes only this development log.
