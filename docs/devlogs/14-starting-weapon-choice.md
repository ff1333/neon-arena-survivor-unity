# Starting Weapon Choice

Date: 2026-09-30

Branch: `feature/free-weapon-loadout`

## Problem

The previous run always equipped a pistol in `PlayerShooter.Awake`. That made
the first slot a hidden fixed decision and weakened the promise that the
three-choice system defines the player's build.

## Decision

- The player starts with zero equipped weapons.
- Pressing Start opens a deterministic Pistol, SMG and Laser choice.
- Movement, shooting, enemy spawning, the run timer and mobile controls remain
  disabled while this choice is open.
- Selecting a card equips that weapon in slot 0 and then starts the run.
- Later weapon levels continue to use the same three upgrade assets.
- The six-slot total limit remains unchanged.

## Implementation

- `PlayerShooter` validates and clears six slots but no longer owns a serialized
  default weapon.
- `UpgradeController.ShowStartingWeaponChoices` filters the existing upgrade
  data, requires three distinct weapon types and displays combat stats.
- `GameManager.StartRun` now enters the selection state; `BeginRun` activates
  gameplay only after the selection callback.
- The release boundary verifier reads weapon definitions from upgrade data,
  verifies that the scene starts empty and retains the six-slot checks.

## Verification

- [x] Unity script compilation succeeds with zero C# errors.
- [x] Automated configuration checks confirm zero pre-equipped weapons and
  three distinct starting weapon types.
- [x] Start opens exactly Pistol, SMG and Laser in a fixed order.
- [x] Timer, enemies, movement and mobile controls stay stopped before choice.
- [x] Pistol equips the yellow first slot and uses pistol stats.
- [x] SMG equips the cyan first slot and uses SMG stats.
- [x] Laser equips the magenta first slot and uses laser stats.
- [x] A selected weapon starts the run once and only once.
- [x] Pause, level-up choices, death, restart and Android touch still work.

The first two checks were executed in Unity 6000.3.18f1 by the Editor boundary
verification tool. Input, timing, visuals and device behavior passed the manual
acceptance test on 2026-10-01.

## Interview Explanation

I moved the initial weapon from a hidden serialized default into the same
data-driven choice path used by later upgrades. The game state stays paused
until the callback confirms a valid selection, so player agency improves
without duplicating weapon assets or allowing simulation to begin behind the
menu.
