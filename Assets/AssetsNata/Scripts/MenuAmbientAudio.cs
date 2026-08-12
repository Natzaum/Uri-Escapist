using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class MenuAmbientAudio : MonoBehaviour
{
    private const int SampleRate = 44100;
    private const float DurationSeconds = 24f;
    private const float Tau = Mathf.PI * 2f;

    private AudioSource audioSource;
    private AudioSource effectsSource;
    private Coroutine fadeRoutine;

    public void Initialize(float volume, float feedbackVolume)
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
        float targetVolume = Mathf.Clamp01(volume);
        audioSource.volume = 0f;

        if (audioSource.clip == null)
            audioSource.clip = CreateAmbientLoop();

        if (!audioSource.isPlaying)
            audioSource.Play();

        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        effectsSource.ignoreListenerPause = true;
        effectsSource.volume = Mathf.Clamp01(feedbackVolume);
        effectsSource.clip = CreateStartFeedback();

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeIn(targetVolume));
    }

    public void PlayStartFeedback()
    {
        if (effectsSource != null && effectsSource.clip != null)
            effectsSource.Play();
    }

    public IEnumerator FadeOut(float duration)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        float initialVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(initialVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 0f;
    }

    private static AudioClip CreateAmbientLoop()
    {
        int frameCount = Mathf.RoundToInt(SampleRate * DurationSeconds);
        float[] samples = new float[frameCount * 2];

        for (int frame = 0; frame < frameCount; frame++)
        {
            float time = frame / (float)SampleRate;
            float echoes = CreateEchoes(time);

            samples[frame * 2] = echoes;
            samples[frame * 2 + 1] = echoes * 0.86f;
        }

        AudioClip clip = AudioClip.Create("Uri Menu Ambience", frameCount, 2, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateStartFeedback()
    {
        const float duration = 0.24f;
        int frameCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[frameCount];
        uint noiseState = 0x8A31C2D5u;

        for (int frame = 0; frame < frameCount; frame++)
        {
            float time = frame / (float)SampleRate;
            float attack = Mathf.Clamp01(time / 0.0015f);

            noiseState = noiseState * 1664525u + 1013904223u;
            float noise = ((noiseState >> 8) & 0xffff) / 32767.5f - 1f;

            float body = Mathf.Sin(Tau * 185f * time) * Mathf.Exp(-time * 24f) * 0.55f;
            float knock = Mathf.Sin(Tau * 510f * time) * Mathf.Exp(-time * 42f) * 0.28f;
            float metal = Mathf.Sin(Tau * 1380f * time) * Mathf.Exp(-time * 34f) * 0.1f;
            float transient = noise * Mathf.Exp(-time * 85f) * 0.24f;

            samples[frame] = Mathf.Clamp((body + knock + metal + transient) * attack, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Uri Play Tack", frameCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static float CreateEchoes(float time)
    {
        return Echo(time, 2f, 247.5f, 0.075f, 1.05f)
             + Echo(time, 6f, 330f, 0.09f, 1.05f)
             + Echo(time, 10f, 220f, 0.07f, 1.05f)
             + Echo(time, 14f, 275f, 0.08f, 1.05f)
             + Echo(time, 18f, 165f, 0.07f, 1.05f)
             + Echo(time, 22f, 330f, 0.08f, 1.05f);
    }

    private static float Echo(float time, float center, float frequency, float gain, float width)
    {
        float distance = Mathf.Abs(time - center);
        distance = Mathf.Min(distance, DurationSeconds - distance);
        float envelope = Mathf.Exp(-(distance * distance) / (2f * width * width));
        return Mathf.Sin(Tau * frequency * time) * envelope * gain;
    }

    private IEnumerator FadeIn(float targetVolume)
    {
        const float fadeDuration = 3f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        fadeRoutine = null;
    }
}
