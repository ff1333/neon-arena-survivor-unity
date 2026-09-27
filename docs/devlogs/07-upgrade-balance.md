# Upgrade Balance And Eligibility

Date: 2026-09-27

Branch: `feature/upgrade-balance`

## Implemented

- Added capped global damage, range and attack-speed multipliers.
- Added player-owned experience pickup radius with a fixed cap.
- Added movement-speed cap and full-health heal filtering.
- Allowed the upgrade panel to display one to three centered choices.
- Preserved weapon choices every three levels and six-slot limits.

## Verification

- [x] Damage stops at 2.0x.
- [x] Range stops at 1.5x for both targeting and projectile travel.
- [x] Attack speed stops at 2.0x for all equipped weapons.
- [x] Pickup radius stops at 6.
- [x] Movement speed stops at 9.
- [x] Heal is absent at full health and eligible after damage.
- [x] One-choice and two-choice layouts remain centered and clickable.
- [x] Temporary Play-mode test values restore after stopping.
- [x] Final Console contains zero red script or runtime errors.

## Evidence

The normal Play-mode pass covered enemy spawning, automatic fire,
experience pickup, ordinary three-choice upgrades and weapon levels.

An isolated Unity 6.3 batch-mode verification then executed 32 assertions
against the same scene and scripts. It verified the serialized production
defaults, every numeric clamp, conditional eligibility at full and reduced
health, the `1.88 + 0.12 = 2.0` damage boundary, shared projectile range,
one-choice and two-choice button coordinates, button clicks and panel closing.
The verifier exited successfully without compiler errors or runtime
exceptions.

## Notes

The multiplier assets store additive increments such as 0.12 and 0.08.
Runtime code clamps accumulated values to explicit caps. WeaponDefinition
assets remain immutable base configurations; PlayerShooter combines them with
run-specific global multipliers.
