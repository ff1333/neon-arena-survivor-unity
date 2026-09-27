# Multi-Weapon Loadout

Date: 2026-09-27

Branch: `feature/multi-weapon-loadout`

Base: `bdaace7` (`main` after the contact-damage shake fix)

## Problem

The previous projectile-count upgrade split one volley into equal angles. The center projectile usually aimed correctly while side projectiles often missed, and every projectile shared one color and one stat profile. Target acquisition also used weapon range while missed projectiles were released by the camera viewport, so range did not have one consistent meaning.

## Goals

- Replace spread projectiles with visible weapon slots around the player.
- Give every equipped weapon independent cooldown, targeting and firing origin.
- Make pistol, SMG and laser readable by both color and shape.
- Use each weapon's world-space range for acquisition and projectile travel.
- Limit each weapon type to two copies and the total loadout to six weapons.
- Offer weapon choices every three levels and utility choices otherwise.
- Keep all projectile variants in the existing object pool.

## Design

| Weapon | Damage | Interval | Range | Speed | Visual |
|---|---:|---:|---:|---:|---|
| Pistol | 25 | 0.45 s | 5.5 | 14 | Yellow circle |
| SMG | 8 | 0.14 s | 6.5 | 18 | Cyan triangle |
| Laser | 75 | 1.35 s | 8 | 28 | Magenta square |

The three weapons have similar theoretical sustained damage but different firing rhythms. The first version deliberately keeps a shared projectile behavior; piercing, critical hits and incoming-damage reservation remain separate future iterations.

## Architecture

- `WeaponDefinition` stores immutable identity, visual and combat data in ScriptableObject assets.
- `PlayerShooter` owns six runtime slots and one cooldown per equipped weapon.
- `EnemyRegistry` tracks active pooled enemies without repeated `FindGameObjectsWithTag` array creation.
- `TargetSelector` owns the shared nearest-band and lowest-current-health policy.
- `Projectile` receives a WeaponDefinition for every shot and resets pooled visual state on disable.
- `UpgradeController` separates every-third-level weapon choices from normal utility choices.
- `PlayerUpgradeApplier` enforces the two-per-type and six-slot limits through PlayerShooter.

## Pooling And Lifecycle

Pistol, SMG and laser projectiles share one `ProjectilePool`. Every `Fire` call replaces sprite, color, damage, speed and remaining travel distance, preventing a reused object from retaining the previous weapon's state. Hits release immediately; misses release after consuming the firing weapon's range. The two-second lifetime remains only as a safety fallback.

The duplicate `PoolMember` component on `Projectile.prefab` was removed so each pooled projectile has one ownership handle.

## Upgrade Rules

- The run starts with one pistol.
- Levels 3, 6, 9, 12 and 15 prioritize eligible weapon choices.
- Pistol, SMG and laser are each capped at two copies.
- Full weapon types are removed from later candidates.
- Non-weapon levels retain Heal, Move Speed and Max Health.
- The old Damage, Fire Rate, Attack Range and Projectile Count assets remain in Git history but are no longer referenced by the scene upgrade list.

## Verification

Evidence available before commit:

- [x] Unity completed the latest script compilation with zero `CS` errors in the Editor log.
- [x] The latest Editor log contains zero `NullReferenceException` and `MissingReferenceException` entries.
- [x] The scene contains six assigned weapon slots and three assigned upgrade icons.
- [x] All three WeaponDefinition assets contain the planned damage, interval, range, speed and color values.
- [x] The accelerated progression run reached Level 27.
- [x] Level 15 allowed the sixth weapon slot to be filled.
- [x] A weapon already at `2/2` no longer appeared in later choices.
- [x] A full six-weapon loadout fell back to utility choices at later weapon levels.
- [x] The accelerated six-weapon regression completed with zero red Console errors.

The temporary three-XP-per-level test path was removed after verification. The normal requirement starts at five XP and resumes its `1.35x` growth curve. `Debug/Add One Level` remains available as an editor-only testing aid.

## Review Notes

- No percentage performance improvement is claimed without a controlled Profiler comparison.
- Unrelated `New Layer 1-4` entries were removed from `ProjectSettings/TagManager.asset` before submission.
- The three upgrade icon GameObjects use the consistent names `WeaponIcon0`, `WeaponIcon1` and `WeaponIcon2`.
- The timer's initial text uses an ASCII colon so the assigned TMP font does not emit a missing-glyph warning.
