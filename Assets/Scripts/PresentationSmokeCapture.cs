using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed class PresentationSmokeCapture : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(),"-neonSmoke") >= 0)
        {
            Application.runInBackground=true;
            new GameObject("Presentation Smoke").AddComponent<PresentationSmokeCapture>();
        }
    }

    private IEnumerator Start()
    {
        var args = Environment.GetCommandLineArgs();
        var captureIndex = Array.IndexOf(args,"-screenshot");
        if (captureIndex < 0 || captureIndex + 1 >= args.Length) { Application.Quit(2); yield break; }
        yield return new WaitForSecondsRealtime(1f);
        Application.runInBackground=true;
        var previousLanguage=PortfolioSettings.Chinese;
        var hadBestTime=PlayerPrefs.HasKey("BestTime");var previousBestTime=PlayerPrefs.GetFloat("BestTime");
        var hadBestKills=PlayerPrefs.HasKey("BestKills");var previousBestKills=PlayerPrefs.GetInt("BestKills");
        PortfolioSettings.SetLanguage(Array.IndexOf(args,"-english")<0);
        yield return null;
        if(Array.IndexOf(args,"-settings")>=0) { PortfolioSettingsMenu.Instance.Open();yield return null; }
        var bossCase=Array.Exists(args,arg=>arg is "-neonBoss" or "-neonRewards" or "-neonFinal" or "-neonVictory");
        if(bossCase)
        {
            var manager=FindFirstObjectByType<GameManager>();manager.StartRun();yield return null;yield return null;
            GameObject.Find("UpgradeButton1").GetComponent<Button>().onClick.Invoke();yield return null;
            var weapons=FindFirstObjectByType<PlayerShooter>();weapons.enabled=false;FindFirstObjectByType<EnemySpawner>().enabled=false;
            var definition=Resources.FindObjectsOfTypeAll<WeaponDefinition>().First();
            while(weapons.EquippedCount<6)weapons.EquipWeapon(definition);
            yield return new WaitForSeconds(2.3f);
            var encounter=manager.GetComponent<BossEncounter>();
            if(encounter.Stage!=BossStage.MiniFight)throw new InvalidOperationException("Boss trigger failed");
            if(Array.IndexOf(args,"-neonBoss")<0)
            {
                encounter.ActiveBoss.Health.TakeDamage(1000000);yield return null;
                if(Array.IndexOf(args,"-neonRewards")<0)
                {
                    encounter.ChooseReward(0);encounter.ChooseReward(2);yield return new WaitForSeconds(2.3f);
                    if(Array.IndexOf(args,"-neonVictory")>=0){encounter.ActiveBoss.Health.TakeDamage(1000000);yield return null;}
                }
            }
            if(encounter.ActiveBoss!=null)
            {
                encounter.ActiveBoss.transform.position=weapons.transform.position+Vector3.up*3.2f;
                yield return new WaitForSeconds(.9f);
            }
            Debug.Log("NEON_CAMPAIGN_SMOKE_PASS stage="+encounter.Stage);
        }
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
        yield return null;
        var camera=Camera.main;
        foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        { canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.sortingOrder+=1000; }
        foreach(var label in FindObjectsByType<LocalizedLabel>(FindObjectsSortMode.None)) label.Refresh();
        Canvas.ForceUpdateCanvases();
        var target=new RenderTexture(Screen.width,Screen.height,24);camera.targetTexture=target;camera.Render();
        var previous=RenderTexture.active;RenderTexture.active=target;
        var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();
        File.WriteAllBytes(output,texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;Destroy(texture);Destroy(target);
        PortfolioSettings.SetLanguage(previousLanguage);
        if(hadBestTime)PlayerPrefs.SetFloat("BestTime",previousBestTime);else PlayerPrefs.DeleteKey("BestTime");
        if(hadBestKills)PlayerPrefs.SetInt("BestKills",previousBestKills);else PlayerPrefs.DeleteKey("BestKills");
        PlayerPrefs.Save();
        Debug.Log("NEON_PRESENTATION_SMOKE_PASS " + output);
        Application.Quit(0);
    }
}
