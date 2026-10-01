# Free Weapon Loadout

Date: 2026-09-30

Branch: `feature/free-weapon-loadout`

## Problem

The original loadout limited Pistol, SMG and Laser to two copies each. That
rule kept the build visually balanced, but it also pushed every successful run
toward a similar `2/2/2` composition. It conflicted with the upgrade promise
that the player chooses a growth path.

## New Rule

- The loadout still has exactly six physical weapon slots.
- A run begins with an explicit Pistol, SMG or Laser choice instead of a
  pre-equipped pistol.
- A weapon type has no separate copy limit.
- Any mix is valid while a total slot remains, including six copies of one
  weapon or a mixed build.
- Every equipped weapon retains its own slot, cooldown, targeting pass and
  projectile origin.
- Once all six slots are occupied, weapon upgrades become ineligible and a
  weapon level falls back to available utility upgrades.

## UI

A weapon card now displays both values without implying a per-type cap:

```text
Equip SMG  x3  4/6 slots
```

`x3` is the number of equipped SMGs. `4/6 slots` is total loadout usage.

## Modified Files

- `Assets/Scripts/PlayerShooter.cs`
- `Assets/Scripts/GameManager.cs`
- `Assets/Scripts/PlayerUpgradeApplier.cs`
- `Assets/Scripts/UpgradeController.cs`
- `Assets/Data/Upgrade_EquipPistol.asset`
- `Assets/Data/Upgrade_EquipSMG.asset`
- `Assets/Data/Upgrade_EquipLaser.asset`
- `Assets/Scenes/Main.unity`
- `Assets/Editor/ReleasePipeline.cs`

## Verification

- [x] Unity script compilation succeeds with zero C# errors.
- [x] One weapon type can occupy all six slots.
- [x] A seventh weapon is rejected by the six-slot total limit.
- [x] The run remains stopped until a starting weapon is selected.
- [x] Each of the three starting choices equips the correct first weapon.
- [x] A full loadout falls back to utility choices on later weapon levels.
- [x] Upgrade cards show type count and total slot usage.
- [x] Existing mixed loadouts remain valid.
- [x] Gameplay, pause, restart and Android touch input remain unaffected.

The first three checks were executed in Unity 6000.3.18f1 by the Editor
boundary-verification tool. The remaining checks passed the manual acceptance
test on 2026-10-01, including Android touch input.

## Interview Explanation

I originally capped every weapon type at two copies. Testing the progression
showed that this made the apparent three-choice system converge on similar
balanced loadouts. I moved the invariant to the real resource, the six physical
slots. The selection system now allows specialization while `CanEquip` still
prevents overflow, so player agency increases without adding runtime or UI
complexity.
