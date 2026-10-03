using UnityEngine;

[DisallowMultipleComponent]
public sealed class VHSFilterSettings : MonoBehaviour
{
    [SerializeField] private bool effectEnabled = true;
    [SerializeField, Range(0f, 1f)] private float intensity = 0.35f;
    [SerializeField, Range(0f, 1f)] private float scanlineStrength = 0.12f;
    [SerializeField, Range(0f, 0.2f)] private float noiseStrength = 0.025f;
    [SerializeField, Range(0f, 0.05f)] private float jitterStrength = 0.008f;
    [SerializeField, Range(0f, 0.01f)] private float chromaticAberration = 0.0015f;
    [SerializeField, Range(0f, 0.25f)] private float trackingBandStrength = 0.06f;
    [SerializeField, Range(0.1f, 5f)] private float trackingBandSpeed = 0.8f;

    // Modificador em runtime; os valores originais do perfil permanecem intactos.
    public float ExhaustionIntensity { get; set; }

    public bool IsEffectActive => isActiveAndEnabled && effectEnabled && intensity > 0f;
    public float Intensity => Mathf.Clamp01(intensity + Mathf.Clamp01(ExhaustionIntensity) * .3f);
    public float ScanlineStrength => scanlineStrength;
    public float NoiseStrength => noiseStrength;
    public float JitterStrength => jitterStrength;
    public float ChromaticAberration => chromaticAberration + Mathf.Clamp01(ExhaustionIntensity) * .004f;
    public float TrackingBandStrength => trackingBandStrength;
    public float TrackingBandSpeed => trackingBandSpeed;
}
