using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public sealed class PresentationSmokeCapture : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(),"-neonSmoke") >= 0)
            new GameObject("Presentation Smoke").AddComponent<PresentationSmokeCapture>();
    }

    private IEnumerator Start()
    {
        var args = Environment.GetCommandLineArgs();
        var captureIndex = Array.IndexOf(args,"-screenshot");
        if (captureIndex < 0 || captureIndex + 1 >= args.Length) { Application.Quit(2); yield break; }
        yield return new WaitForSecondsRealtime(1f);
        if (Array.IndexOf(args,"-neonLoadout") >= 0)
        {
            FindFirstObjectByType<GameManager>().StartRun();
            yield return null;
        }
        if (Array.IndexOf(args,"-neonGameplay") >= 0)
        {
            FindFirstObjectByType<GameManager>().StartRun();
            yield return null;
            GameObject.Find("UpgradeButton1").GetComponent<Button>().onClick.Invoke();
            yield return null;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player.GetComponent<PlayerShooter>().EquippedCount != 1 || Time.timeScale == 0)
                throw new InvalidOperationException("Starting weapon flow failed");
            var spawner = FindFirstObjectByType<EnemySpawner>();
            spawner.enabled = false;
            var entries = (EnemySpawnEntry[])typeof(EnemySpawner).GetField("enemyTypes",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(spawner);
            var pool = GameObject.Find("EnemyPool").GetComponent<GameObjectPool>();
            var xp = GameObject.Find("ExperiencePool").GetComponent<GameObjectPool>();
            for (var i = 0; i < entries.Length; i++)
            {
                var enemy = pool.Get(new Vector3(-3f + i * 3f,3.5f,0),Quaternion.identity).GetComponent<EnemyController>();
                enemy.Spawn(player.transform,xp,entries[i].Definition);
                if (enemy.GetComponent<SpriteRenderer>().sprite.name != entries[i].Definition.BehaviorType.ToString())
                    throw new InvalidOperationException("Enemy appearance mismatch");
            }
            yield return new WaitForSeconds(1f);
            player.GetComponent<Health>().TakeDamage(15f);
            CombatFeedback.Instance.ShowDamage(new Vector3(0,2,0),34,false);
            // Verify pooled trails cannot connect a reused projectile to its previous location.
            var bullets = FindObjectsByType<Projectile>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach (var bullet in bullets)
            {
                if (!bullet.gameObject.activeSelf && bullet.TryGetComponent<TrailRenderer>(out var trail) && trail.positionCount != 0)
                    throw new InvalidOperationException("Inactive projectile retained a trail");
            }
            Debug.Log("NEON_POLISH_FLOW_PASS weapons=1 enemyStyles=3 pooledTrails=clear");
            if (Array.IndexOf(args,"-neonPause") >= 0)
                GameObject.Find("PauseButton").GetComponent<Button>().onClick.Invoke();
        }
        var output = args[captureIndex + 1];
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        Time.timeScale = 0f;
        ScreenCapture.CaptureScreenshot(output);
        yield return new WaitForSecondsRealtime(1f);
        Debug.Log("NEON_PRESENTATION_SMOKE_PASS " + output);
        Application.Quit(0);
    }
}
