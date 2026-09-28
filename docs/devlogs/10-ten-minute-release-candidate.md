# Ten-Minute Release Candidate

Date: 2026-09-28

Branch: `test/release-candidate-balance`

## Formal Configuration

- Enemy weights: Chaser 70 / Dasher 22 / Berserker 8
- Unlock times: 0 / 20 / 45 seconds
- Spawn interval: 1.5 to 0.55 seconds
- Spawn interval decrease: 0.0045 per second
- First level requirement: 5
- Upgrade requirement growth: 1.35x
- Starting weapon: Pistol

## Build Rule

- New weapon type first on weapon levels.
- Emergency healing below 50% HP.
- Damage, fire rate and range before utility upgrades.
- No debug levels, temporary Inspector values or invincibility.

## Checkpoints

| HUD Time | Level | Kills | HP | Weapons | Important upgrades | Pressure 1-5 | CPU ms | Memory MB | GC Alloc |
|---|---:|---:|---:|---|---|---:|---:|---:|---:|
| 01:00 | 4 | 37 | 100/100 | Pistol x1 / SMG x1 / Laser x0 | Global range +8%, Equip SMG, Global range +8% | 2 | 16.661 | 50.44 | 197 B/frame |
| 03:00 | 9 | 157 | 82/100 | Pistol x1 / SMG x2 / Laser x1 | Global damage +12%, Equip Laser, Global fire rate +10%, Global damage +12%, Equip SMG | 2 | 16.671 | 50.49 | 136 B/frame |
| 05:00 | 11 | 364 | 82/100 | Pistol x1 / SMG x2 / Laser x1 | Global fire rate +10%, Global fire rate +10% | 3 | 16.672 | 50.68 | 171 B/frame |
| 07:00 | 12 | 582 | 82/100 | Pistol x1 / SMG x2 / Laser x2 | Equip Laser | 3 | 16.671 | 50.77 | 189 B/frame |
| 10:00 | 14 | 913 | 82/100 | Pistol x1 / SMG x2 / Laser x2 | Global damage +12%, Global fire rate +10% | 2 | 16.671 | 50.76 | 182 B/frame |

The peak pressure after 07:00 was 3. The maximum active enemy count was 19,
and no measured gameplay frame exceeded 33 ms.

## Final Build

- Weapon copies: Pistol x1 / SMG x2 / Laser x2
- Damage multiplier: 1.36x
- Range multiplier: 1.16x
- Attack-speed multiplier: 1.40x
- Move speed: 6.00
- Pickup radius: 3.00
- Max HP: 100

## Verification

- [x] Survived to HUD time 10:00 without debug assistance.
- [x] Chaser, Dasher and Berserker all appeared.
- [x] Dasher charge and Berserker enrage remained readable.
- [x] Upgrade choices remained useful and each offered choice was accepted.
- [x] Difficulty increased without an unavoidable damage wall.
- [x] Typical CPU frame time stayed below 16.7 ms.
- [x] Repeated gameplay frames did not exceed 33 ms.
- [x] Memory after warm-up did not grow continuously.
- [x] Repeated gameplay GC Alloc stayed at or below 1 KB per frame.
- [x] Pause, resume and upgrade time stopping remained correct.
- [x] Game over and restart restored the initial run state.
- [x] Final runtime log contained zero red errors.

## Result

- Decision: PASS AFTER RETEST
- Failed criteria: the instrumented baseline rerun reached only pressure 2
  after 07:00, while the player remained at high health.
- Parameter changes: `Minimum Interval` changed from 0.65 to 0.55, and
  `Interval Decrease Per Second` changed from 0.004 to 0.0045. No other
  formal combat value changed.
- Retest result: survived 10:00 at level 14 with 913 kills, pressure 3 after
  07:00, 0.27 MB memory growth from 03:00 to 10:00, 182 B/frame final-interval
  GC allocation and zero runtime errors.

## Test History

1. The first baseline run survived 10:00 at level 14 with 794 kills and zero
   runtime errors. It recorded a global pressure peak of 3, but did not retain
   the time of that peak.
2. The instrumented baseline rerun survived 10:00 at level 13 with 784 kills,
   but measured only pressure 2 after 07:00.
3. The prescribed late-game spawn adjustment increased the maximum active
   enemies from 17 to 20. This exposed that the initial automated pressure
   calculation measured only health and enemies within four units, so it did
   not represent the documented meaning of pressure 3: controlled movement is
   required.
4. The pressure recorder was corrected to include active enemy load. The final
   full retest then passed every fixed criterion without another balance
   change.

## Automation Boundary

The final run used a Development Player with formal health, enemy weights,
unlock times, spawn timings, level requirements, weapon data and upgrade
effects. Movement and selection among the real offered upgrade cards were
automated so the same rule could be repeated. The verifier did not grant
experience, weapons, health, invincibility or temporary combat multipliers.

The CPU value is a same-machine headless Development Player regression metric,
not a claim about all desktop GPUs. This is a fixed-rule release-candidate
regression, not a large-scale player test. Pointer hitboxes had already been
manually verified during the UI pass; this run verified the upgrade controller
selection flow rather than physical mouse clicks.

## Notes

The most demanding period was the transition from five to seven minutes. The
active enemy load required continuous movement while the second Laser copy was
still pending. Pressure reached 3 without creating a sustained contact-damage
wall, and health remained at 82/100 through the final checkpoint.

The most effective progression was adding SMG early, adding Laser before the
middle of the run, and then improving global fire rate and damage. The weapon
copies increased target coverage, while the global upgrades kept later enemy
health from turning the arena into an unbounded backlog.

Pressure scale: 1 means no meaningful threat; 3 means controlled movement is
required; 5 means survival feels impossible despite correct movement.
