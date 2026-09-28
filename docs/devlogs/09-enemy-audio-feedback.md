# Enemy Variants, Audio And Combat Feedback

Date: 2026-09-28

Branch: `feature/enemy-audio-feedback`

## Implemented

- Added ScriptableObject-based enemy definitions.
- Added weighted unlock times for three enemy types.
- Added a telegraphed directional dash behavior.
- Added a health-threshold berserk behavior.
- Reused one enemy prefab and pool across all variants.
- Added runtime-generated weapon and combat sounds.
- Added particle feedback for hits, deaths, player damage and state changes.
- Added distinct spawn-warning colors for enemy variants.
- Corrected four low-contrast game-state button labels to `#F4F7FA`.

## Verification

- [x] Chaser behavior passes.
- [x] Dasher telegraph, direction lock and dash pass.
- [x] Berserker health threshold and speed transition pass.
- [x] Weighted unlock times pass with formal values.
- [x] Enemy pool resets color, scale and behavior state.
- [x] Weapon, hit, death, player-hit and level-up audio pass.
- [x] Combat particles remain behind the UI and visible above gameplay.
- [x] Three-minute automated gameplay regression passes.
- [x] Final Console and standalone runtime contain zero game errors.

## Automated Evidence

- Unity edit-mode verifier: 109 assertions passed, 0 failed.
- Simulated gameplay duration: 180.0 seconds.
- Enemy types observed: Chaser, Dasher and Berserker.
- Kills: 113.
- Final level: 7.
- Maximum active enemies: 8.
- Runtime game errors: 0.

The automated gameplay run used formal enemy definitions, spawn weights,
unlock times, intervals, weapons, upgrades and object pools. Player movement
and upgrade selection were automated, and player health was raised only to
keep the unattended stress run alive. It is a system regression rather than
a balance verdict; the formal ten-minute balance pass remains a separate
release-candidate task.

## Notes

Enemy identity is stored in ScriptableObject assets while runtime state remains
in EnemyController. Pooled enemies are fully reconfigured on every spawn.
Audio clips are generated from short waveforms at runtime, so this version has
no external audio licensing dependency.

The enemy/audio implementation did not change existing TMP colors. Four dark
button labels were inherited from the previous main branch and were corrected
during this verification round after the contrast issue was reported.
