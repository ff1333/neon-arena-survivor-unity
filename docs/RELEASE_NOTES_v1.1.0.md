# Neon Arena Rebuild v1.1.0

Release date: 2026-10-02

## Highlights

- Start each run by choosing Pistol, SMG or Laser.
- Build any six-slot weapon composition, including six copies of one weapon.
- Read enemy drops at a glance through distinct pickup shapes and colors.
- Keep the existing Windows, WebGL and Android control schemes.

## Gameplay Changes

- Removed the hidden default pistol from `PlayerShooter`.
- Added a paused starting-weapon selection before the timer and enemy spawner begin.
- Removed the two-copies-per-weapon restriction while retaining the six-slot total cap.
- Updated weapon cards to show the type count and total occupied slots.
- Added a green diamond for Chaser XP, a gold arrow for Dasher XP and a larger
  violet hexagon for Berserker XP.

## Engineering Changes

- Reused the data-driven upgrade path for starting weapon selection.
- Reconfigured every pooled experience pickup on spawn to prevent stale visuals.
- Added automated checks for empty starting loadouts, all three starting choices,
  six copies of one weapon and pooled pickup appearance reuse.

## Verification

- Unity automated release-boundary suite: `24/24 PASS`.
- Windows standalone build and startup smoke test: PASS.
- WebGL build and browser gameplay smoke test: PASS.
- Android APK build: PASS.
- Android v1.1.0 physical-device smoke test: PASS.

## Known Limits

- Single-player, single-scene portfolio scope.
- Android coverage is limited to the available physical device, not a device lab.
- Store signing and commercial store submission are outside this GitHub release.
