# Enemy-Specific Experience Visuals

Date: 2026-09-30

Branch: `feature/free-weapon-loadout`

## Problem

All enemies dropped the same green circular pickup. Chaser and Dasher both
reward one experience point while Berserker rewards three, but the ground
objects did not communicate either enemy identity or relative value.

## Decision

| Enemy | Pickup | Color | XP | Scale |
|---|---|---|---:|---:|
| Chaser | Diamond | Neon green | 1 | 1.00x |
| Dasher | Up arrow | Gold | 1 | 1.00x |
| Berserker | Hexagon | Violet | 3 | 1.35x |

Shape and color communicate source type. Scale communicates that the
Berserker reward is worth more; equal one-point rewards keep equal size.

## Architecture

- `EnemyDefinition` stores pickup shape, color and scale with experience value.
- `EnemyController` passes the complete drop configuration on death.
- `ExperiencePickup.Configure` overwrites value, Sprite, color and scale every
  time an object leaves the shared pool.
- The three 64 by 64 antialiased sprites are generated once on first use and
  cached for the rest of the application session.
- No extra prefab or object pool is required.

## Verification

- [x] Unity script compilation succeeds with zero C# errors.
- [x] Automated configuration check finds three distinct shapes and colors.
- [x] Automated reuse check reapplies Sprite, color and scale on one pooled
  pickup instance.
- [x] Chaser visibly drops a green diamond.
- [x] Dasher visibly drops a gold arrow.
- [x] Berserker visibly drops a larger violet hexagon worth three XP.
- [x] Rapid mixed kills never retain the previous enemy's pickup visual.
- [x] Attraction, collection, leveling, pause and restart still work.
- [x] Android rendering and collection work on a physical device.

The automated Editor boundary suite completed `23/23 PASS`. Visual readability,
gameplay behavior and Android device behavior passed the manual acceptance test
on 2026-10-01.

## Interview Explanation

I kept one pooled pickup prefab and moved visual differences into enemy data.
Because pooled objects retain component state, Configure must replace both the
reward value and every visual property. The icons are generated lazily and
cached, so the feature adds readable feedback without adding per-drop texture
allocation or three duplicate pools.
