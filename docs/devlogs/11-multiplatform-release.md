# Multiplatform Release Verification

Date: 2026-09-29

Branch: `release/v1.0.0-multiplatform`

## Automated Boundary Verification

Result: PASS

- [x] Main scene is enabled at build index 0
- [x] Windows build support is installed
- [x] WebGL build support is installed
- [x] Android build support is installed
- [x] Main scene contains no missing scripts
- [x] Main scene contains four object pools
- [x] Every object pool has a prefab and positive prewarm size
- [x] Upgrade UI has three cards and ten release upgrades
- [x] Pool expands when its prewarmed queue is empty
- [x] Duplicate Release does not enqueue one instance twice
- [x] Target selection returns null with no enemies
- [x] Company name is release-ready
- [x] Product name is release-ready
- [x] Version is 1.0.0
- [x] Android identifier is release-ready
- [x] Portrait autorotation is disabled
- [x] Both landscape orientations are enabled

This verification was executed by an Editor automation tool. It is not represented as a manual user test.

## Platform Builds

| Platform | Build | Independent runtime | Errors | Notes |
|---|---|---|---:|---|
| Windows | PASS | PASS | 0 | Standalone build started outside Unity; start/gameplay evidence and Player logs were reviewed. |
| WebGL | PASS | PASS | 0 | Local HTTP run covered start, combat, upgrade selection and game over; browser Console had no errors. |
| Android | PASS | PASS | 0 | User completed a real-device playtest of the rebuilt touch-control APK and reported no material issue. |

## Manual And Runtime Evidence

- Windows build summary: `StandaloneWindows64`, 0 errors, 0 warnings.
- Windows standalone evidence: `player-release.log`, `player-release-2.log` and
  runtime screenshots under `Builds/Windows/v1.0.0`.
- The first Windows Player log contained a Unity TLS certificate warning and a
  shutdown-time `ComputeBuffer` disposal warning. The D3D11 rerun started and
  exited cleanly, and neither log contained a gameplay exception or C# stack
  trace. These are recorded as non-blocking engine/service warnings rather than
  hidden as gameplay failures.
- WebGL build summary: 0 errors, 0 warnings.
- WebGL local server: `http://127.0.0.1:8080`; loader, WASM, framework and data
  files returned HTTP 200/304.
- WebGL runtime: start, combat, level-up selection and game-over UI displayed.
- WebGL browser Console: 0 errors; one non-blocking Unity persistent-data
  synchronization deprecation warning.
- Android build result: succeeded; APK was installed and played on a physical
  Android device after the floating-joystick fix.
- Android runtime result was reported by the user; no ADB device was connected
  during this documentation pass, so no `logcat` capture was claimed.
- Package SHA-256 values were recomputed and matched `SHA256SUMS.txt` for all
  three deliverables.

The build artifacts and screenshots are ignored local evidence and are not
committed to Git. Section 9.5 still requires a clean-main rebuild and final
retest before the immutable `v1.0.0` tag is created.

## Release Decision

PASS - Ready for release commits and pull request. Final clean-main rebuild and
retest remain mandatory before tagging `v1.0.0`.
