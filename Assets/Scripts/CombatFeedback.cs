using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(ParticleSystem))]
public class CombatFeedback : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.65f;

    private const int SampleRate = 44100;

    private AudioSource audioSource;
    private ParticleSystem feedbackParticles;
    private AudioClip pistolShotClip;
    private AudioClip smgShotClip;
    private AudioClip laserShotClip;
    private AudioClip hitClip;
    private AudioClip deathClip;
    private AudioClip playerHitClip;
    private AudioClip chargeClip;
    private AudioClip enrageClip;
    private AudioClip levelUpClip;
    private float nextShotSoundTime;
    private float nextHitSoundTime;

    public static CombatFeedback Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        feedbackParticles = GetComponent<ParticleSystem>();

        ConfigureAudioSource();
        ConfigureParticles();
        CreateClips();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PlayShot(WeaponType weaponType)
    {
        if (Time.unscaledTime < nextShotSoundTime)
        {
            return;
        }

        AudioClip clip = weaponType switch
        {
            WeaponType.SubmachineGun => smgShotClip,
            WeaponType.Laser => laserShotClip,
            _ => pistolShotClip
        };

        nextShotSoundTime = Time.unscaledTime + 0.035f;
        audioSource.PlayOneShot(clip);
    }

    public void PlayEnemyHit(Vector3 position, Color color)
    {
        EmitBurst(position, color, 4, 0.1f);

        if (Time.unscaledTime >= nextHitSoundTime)
        {
            nextHitSoundTime = Time.unscaledTime + 0.025f;
            audioSource.PlayOneShot(hitClip);
        }
    }

    public void PlayEnemyDeath(Vector3 position, Color color)
    {
        EmitBurst(position, color, 12, 0.16f);
        audioSource.PlayOneShot(deathClip);
    }

    public void PlayPlayerHit(Vector3 position)
    {
        EmitBurst(position, new Color(1f, 0.2f, 0.35f), 14, 0.18f);
        audioSource.PlayOneShot(playerHitClip);
    }

    public void PlayEnemyCharge(Vector3 position, Color color)
    {
        EmitBurst(position, color, 6, 0.12f);
        audioSource.PlayOneShot(chargeClip);
    }

    public void PlayEnemyEnrage(Vector3 position, Color color)
    {
        EmitBurst(position, color, 10, 0.15f);
        audioSource.PlayOneShot(enrageClip);
    }

    public void PlayLevelUp(Vector3 position)
    {
        EmitBurst(position, new Color(0.27f, 0.9f, 0.78f), 18, 0.16f);
        audioSource.PlayOneShot(levelUpClip);
    }

    private void ConfigureAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = masterVolume;
    }

    private void ConfigureParticles()
    {
        ParticleSystem.MainModule main = feedbackParticles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = 0.22f;
        main.startSpeed = 0f;
        main.startSize = 0.12f;
        main.gravityModifier = 0f;
        main.maxParticles = 256;

        ParticleSystem.EmissionModule emission = feedbackParticles.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = feedbackParticles.shape;
        shape.enabled = false;

        ParticleSystemRenderer particleRenderer =
            feedbackParticles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sortingOrder = 20;
        feedbackParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void EmitBurst(
        Vector3 position,
        Color color,
        int count,
        float size)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(1.5f, 3.5f);
            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
            {
                position = position,
                velocity = new Vector3(
                    Mathf.Cos(angle) * speed,
                    Mathf.Sin(angle) * speed,
                    0f),
                startColor = color,
                startSize = size,
                startLifetime = 0.22f
            };

            feedbackParticles.Emit(emit, 1);
        }
    }

    private void CreateClips()
    {
        pistolShotClip = CreateSweep("Pistol", 520f, 390f, 0.055f, 0.13f);
        smgShotClip = CreateSweep("SMG", 720f, 520f, 0.035f, 0.09f);
        laserShotClip = CreateSweep("Laser", 920f, 1320f, 0.12f, 0.12f);
        hitClip = CreateSweep("Hit", 260f, 170f, 0.045f, 0.1f);
        deathClip = CreateSweep("Death", 190f, 70f, 0.14f, 0.16f);
        playerHitClip = CreateSweep("PlayerHit", 130f, 55f, 0.18f, 0.2f);
        chargeClip = CreateSweep("Charge", 280f, 620f, 0.16f, 0.1f);
        enrageClip = CreateSweep("Enrage", 150f, 330f, 0.2f, 0.14f);
        levelUpClip = CreateLevelUpClip();
    }

    private static AudioClip CreateSweep(
        string clipName,
        float startFrequency,
        float endFrequency,
        float duration,
        float amplitude)
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float progress = i / (float)sampleCount;
            float frequency = Mathf.Lerp(
                startFrequency,
                endFrequency,
                progress);
            phase += Mathf.PI * 2f * frequency / SampleRate;
            float envelope = 1f - progress;
            samples[i] = Mathf.Sin(phase) * envelope * amplitude;
        }

        AudioClip clip = AudioClip.Create(
            clipName,
            sampleCount,
            1,
            SampleRate,
            false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateLevelUpClip()
    {
        float[] frequencies = { 440f, 660f, 880f };
        float noteDuration = 0.08f;
        int samplesPerNote = Mathf.CeilToInt(SampleRate * noteDuration);
        float[] samples = new float[samplesPerNote * frequencies.Length];

        for (int note = 0; note < frequencies.Length; note++)
        {
            float phase = 0f;
            for (int i = 0; i < samplesPerNote; i++)
            {
                float progress = i / (float)samplesPerNote;
                phase += Mathf.PI * 2f * frequencies[note] / SampleRate;
                samples[note * samplesPerNote + i] =
                    Mathf.Sin(phase) * (1f - progress) * 0.14f;
            }
        }

        AudioClip clip = AudioClip.Create(
            "LevelUp",
            samples.Length,
            1,
            SampleRate,
            false);
        clip.SetData(samples, 0);
        return clip;
    }
}