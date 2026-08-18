using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class ExteriorAmbientAudio : MonoBehaviour
{
    private const int SampleRate = 22050;
    private const float Tau = Mathf.PI * 2f;

    private readonly System.Random random = new System.Random(7319);
    private AudioSource windSource;
    private AudioSource[] eventSources;
    private AudioClip[] eventClips;
    private float[] eventVolumes;
    private Transform listenerTransform;
    private int nextSourceIndex;

    private IEnumerator Start()
    {
        AudioListener listener = FindFirstObjectByType<AudioListener>();
        listenerTransform = listener != null ? listener.transform : Camera.main?.transform;

        CreateSources();
        CreateClips();

        windSource.Play();
        StartCoroutine(FadeInWind());

        yield return new WaitForSecondsRealtime(3f);
        StartCoroutine(PlayRandomEvents());
    }

    private void CreateSources()
    {
        windSource = gameObject.AddComponent<AudioSource>();
        windSource.playOnAwake = false;
        windSource.loop = true;
        windSource.spatialBlend = 0f;
        windSource.ignoreListenerPause = true;
        windSource.volume = 0f;

        eventSources = new AudioSource[3];
        for (int index = 0; index < eventSources.Length; index++)
        {
            GameObject sourceObject = new GameObject($"Exterior Event Source {index + 1}");
            sourceObject.transform.SetParent(transform);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 4f;
            source.maxDistance = 60f;
            source.ignoreListenerPause = true;
            eventSources[index] = source;
        }
    }

    private void CreateClips()
    {
        windSource.clip = CreateWindLoop();
        eventClips = new[]
        {
            CreateLeafRustle(),
            CreateDistantImpact(),
            CreateMetalCreak(),
            CreateNightChirps(),
            CreateDryBranchSnap()
        };
        eventVolumes = new[] { 0.2f, 0.28f, 0.16f, 0.11f, 0.19f };
    }

    private IEnumerator FadeInWind()
    {
        const float targetVolume = 0.11f;
        const float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            windSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        windSource.volume = targetVolume;
    }

    private IEnumerator PlayRandomEvents()
    {
        while (true)
        {
            float delay = Mathf.Lerp(6f, 13f, (float)random.NextDouble());
            yield return new WaitForSecondsRealtime(delay);

            if (listenerTransform == null)
                continue;

            int clipIndex = random.Next(eventClips.Length);
            AudioSource source = eventSources[nextSourceIndex];
            nextSourceIndex = (nextSourceIndex + 1) % eventSources.Length;

            float angle = (float)random.NextDouble() * Tau;
            float distance = Mathf.Lerp(16f, 38f, (float)random.NextDouble());
            float height = Mathf.Lerp(-1f, 5f, (float)random.NextDouble());
            source.transform.position = listenerTransform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                height,
                Mathf.Sin(angle) * distance);

            float volumeVariation = Mathf.Lerp(0.85f, 1.1f, (float)random.NextDouble());
            source.pitch = Mathf.Lerp(0.92f, 1.08f, (float)random.NextDouble());
            source.PlayOneShot(eventClips[clipIndex], eventVolumes[clipIndex] * volumeVariation);
        }
    }

    private static AudioClip CreateWindLoop()
    {
        const float duration = 18f;
        int frameCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[frameCount * 2];

        for (int frame = 0; frame < frameCount; frame++)
        {
            float left = CreateCyclicNoise(frame, frameCount, 41, 17) * 0.2f
                       + CreateCyclicNoise(frame, frameCount, 337, 53) * 0.11f
                       + CreateCyclicNoise(frame, frameCount, 1879, 91) * 0.035f;
            float right = CreateCyclicNoise(frame, frameCount, 43, 29) * 0.2f
                        + CreateCyclicNoise(frame, frameCount, 331, 67) * 0.11f
                        + CreateCyclicNoise(frame, frameCount, 1889, 107) * 0.035f;
            float time = frame / (float)SampleRate;
            float gust = 0.62f + 0.24f * Mathf.Sin(Tau * time / duration)
                                  + 0.14f * Mathf.Sin(Tau * 3f * time / duration + 1.3f);

            samples[frame * 2] = left * gust;
            samples[frame * 2 + 1] = right * gust;
        }

        return CreateClip("Exterior Soft Wind", samples, frameCount, 2);
    }

    private static AudioClip CreateLeafRustle()
    {
        const float duration = 1.8f;
        int frameCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[frameCount];
        uint state = 0xA13F72C9u;
        float filtered = 0f;

        for (int frame = 0; frame < frameCount; frame++)
        {
            float time = frame / (float)SampleRate;
            float normalized = time / duration;
            float envelope = Mathf.Sin(Mathf.PI * normalized);
            envelope *= envelope * (0.65f + 0.35f * Mathf.Sin(Tau * 5f * time));
            float noise = NextNoise(ref state);
            filtered = Mathf.Lerp(filtered, noise, 0.18f);
            samples[frame] = filtered * envelope * 0.42f;
        }

        return CreateClip("Exterior Leaves", samples, frameCount, 1);
    }

    private static AudioClip CreateDistantImpact()
    {
        const float duration = 1.15f;
        int frameCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[frameCount];
        uint state = 0xF52B130Du;

        for (int frame = 0; frame < frameCount; frame++)
        {
            float time = frame / (float)SampleRate;
            float body = Mathf.Sin(Tau * 68f * time) * Mathf.Exp(-time * 5.8f) * 0.55f;
            float transient = NextNoise(ref state) * Mathf.Exp(-time * 38f) * 0.34f;
            samples[frame] = body + transient;
        }

        return CreateClip("Exterior Distant Impact", samples, frameCount, 1);
    }

    private static AudioClip CreateMetalCreak()
    {
        const float duration = 2.4f;
        int frameCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[frameCount];

        for (int frame = 0; frame < frameCount; frame++)
        {
            float time = frame / (float)SampleRate;
            float normalized = time / duration;
            float envelope = Mathf.Sin(Mathf.PI * normalized);
            envelope *= envelope;
            float frequency = Mathf.Lerp(510f, 285f, normalized) + Mathf.Sin(Tau * 3f * time) * 24f;
            float tone = Mathf.Sin(Tau * frequency * time) + Mathf.Sin(Tau * frequency * 1.49f * time) * 0.22f;
            samples[frame] = tone * envelope * 0.3f;
        }

        return CreateClip("Exterior Metal Creak", samples, frameCount, 1);
    }

    private static AudioClip CreateNightChirps()
    {
        const float duration = 2.2f;
        int frameCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[frameCount];
        float[] starts = { 0.08f, 0.31f, 1.16f, 1.39f };

        for (int frame = 0; frame < frameCount; frame++)
        {
            float time = frame / (float)SampleRate;
            float sample = 0f;

            for (int chirp = 0; chirp < starts.Length; chirp++)
            {
                float localTime = time - starts[chirp];
                if (localTime < 0f || localTime > 0.12f)
                    continue;

                float envelope = Mathf.Sin(Mathf.PI * localTime / 0.12f);
                float frequency = 2050f + chirp * 95f + localTime * 900f;
                sample += Mathf.Sin(Tau * frequency * localTime) * envelope * 0.24f;
            }

            samples[frame] = sample;
        }

        return CreateClip("Exterior Night Chirps", samples, frameCount, 1);
    }

    private static AudioClip CreateDryBranchSnap()
    {
        const float duration = 0.38f;
        int frameCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[frameCount];
        uint state = 0x319CA74Bu;

        for (int frame = 0; frame < frameCount; frame++)
        {
            float time = frame / (float)SampleRate;
            float first = NextNoise(ref state) * Mathf.Exp(-time * 70f);
            float secondTime = Mathf.Max(0f, time - 0.075f);
            float second = time >= 0.075f ? NextNoise(ref state) * Mathf.Exp(-secondTime * 85f) * 0.65f : 0f;
            samples[frame] = (first + second) * 0.6f;
        }

        return CreateClip("Exterior Dry Snap", samples, frameCount, 1);
    }

    private static float CreateCyclicNoise(int frame, int frameCount, int anchorCount, int seed)
    {
        float position = frame * anchorCount / (float)frameCount;
        int firstIndex = Mathf.FloorToInt(position);
        int secondIndex = (firstIndex + 1) % anchorCount;
        float blend = position - firstIndex;
        blend = blend * blend * (3f - 2f * blend);
        return Mathf.Lerp(HashNoise(firstIndex, seed), HashNoise(secondIndex, seed), blend);
    }

    private static float HashNoise(int index, int seed)
    {
        unchecked
        {
            uint value = (uint)(index + seed * 374761393);
            value = (value ^ (value >> 13)) * 1274126177u;
            value ^= value >> 16;
            return (value & 0xffff) / 32767.5f - 1f;
        }
    }

    private static float NextNoise(ref uint state)
    {
        state = state * 1664525u + 1013904223u;
        return ((state >> 8) & 0xffff) / 32767.5f - 1f;
    }

    private static AudioClip CreateClip(string name, float[] samples, int frameCount, int channels)
    {
        AudioClip clip = AudioClip.Create(name, frameCount, channels, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

public static class ExteriorAmbientBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        InstallForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstallForScene(scene);
    }

    private static void InstallForScene(Scene scene)
    {
        if (scene.name != "MainScene" || UnityEngine.Object.FindFirstObjectByType<ExteriorAmbientAudio>() != null)
            return;

        GameObject ambience = new GameObject("Exterior Ambience");
        SceneManager.MoveGameObjectToScene(ambience, scene);
        ambience.AddComponent<ExteriorAmbientAudio>();
    }
}
