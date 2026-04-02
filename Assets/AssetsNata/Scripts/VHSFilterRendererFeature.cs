using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

[DisallowMultipleRendererFeature("VHS Filter")]
public sealed class VHSFilterRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Material material;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

    private VHSFilterPass pass;
    private Material runtimeMaterial;

    public override void Create()
    {
        pass ??= new VHSFilterPass();
        pass.renderPassEvent = renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!TryGetActiveSettings(ref renderingData, out VHSFilterSettings settings))
        {
            return;
        }

        if (!EnsureRuntimeMaterial())
        {
            return;
        }

        runtimeMaterial.CopyPropertiesFromMaterial(material);
        pass.Setup(runtimeMaterial, settings);
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
        pass = null;

        if (runtimeMaterial != null)
        {
            CoreUtils.Destroy(runtimeMaterial);
            runtimeMaterial = null;
        }
    }

    private bool EnsureRuntimeMaterial()
    {
        if (material == null || material.shader == null)
        {
            if (runtimeMaterial != null)
            {
                CoreUtils.Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }

            return false;
        }

        if (runtimeMaterial != null && runtimeMaterial.shader == material.shader)
        {
            return true;
        }

        if (runtimeMaterial != null)
        {
            CoreUtils.Destroy(runtimeMaterial);
        }

        runtimeMaterial = CoreUtils.CreateEngineMaterial(material.shader);
        return runtimeMaterial != null;
    }

    private static bool TryGetActiveSettings(ref RenderingData renderingData, out VHSFilterSettings settings)
    {
        settings = null;

        ref readonly CameraData cameraData = ref renderingData.cameraData;
        Camera camera = cameraData.camera;

        if (camera == null || cameraData.cameraType != CameraType.Game || cameraData.isSceneViewCamera || cameraData.isPreviewCamera)
        {
            return false;
        }

        if (!camera.TryGetComponent(out settings))
        {
            return false;
        }

        return settings != null && settings.IsEffectActive;
    }

    private sealed class VHSFilterPass : ScriptableRenderPass
    {
        private const string PassName = "VHS Filter";

        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int ScanlineStrengthId = Shader.PropertyToID("_ScanlineStrength");
        private static readonly int NoiseStrengthId = Shader.PropertyToID("_NoiseStrength");
        private static readonly int JitterStrengthId = Shader.PropertyToID("_JitterStrength");
        private static readonly int ChromaticAberrationId = Shader.PropertyToID("_ChromaticAberration");
        private static readonly int TrackingBandStrengthId = Shader.PropertyToID("_TrackingBandStrength");
        private static readonly int TrackingBandSpeedId = Shader.PropertyToID("_TrackingBandSpeed");

        private readonly ProfilingSampler profilingSampler = new(PassName);
        private RTHandle temporaryColor;
        private Material activeMaterial;
        private VHSFilterSettings activeSettings;

        public void Setup(Material materialToUse, VHSFilterSettings settingsToUse)
        {
            activeMaterial = materialToUse;
            activeSettings = settingsToUse;
            requiresIntermediateTexture = true;
        }

#pragma warning disable 618, 672
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!CanRun(ref renderingData))
            {
                return;
            }

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(
                ref temporaryColor,
                descriptor,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: "_VHSFilterTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!CanRun(ref renderingData))
            {
                return;
            }

            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            UpdateMaterialProperties(activeMaterial, activeSettings);

            CommandBuffer cmd = CommandBufferPool.Get(PassName);
            using (new ProfilingScope(cmd, profilingSampler))
            {
                Blitter.BlitCameraTexture(cmd, source, temporaryColor, activeMaterial, 0);
                Blitter.BlitCameraTexture(cmd, temporaryColor, source);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
#pragma warning restore 618, 672

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (activeMaterial == null || activeSettings == null || !activeSettings.IsEffectActive)
            {
                return;
            }

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (!CanRun(cameraData))
            {
                return;
            }

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            TextureHandle source = resourceData.activeColorTexture;
            if (!source.IsValid())
            {
                return;
            }

            UpdateMaterialProperties(activeMaterial, activeSettings);

            TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "_VHSFilterTemp";
            destinationDesc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);
            if (!destination.IsValid())
            {
                return;
            }

            RenderGraphUtils.BlitMaterialParameters parameters = new(source, destination, activeMaterial, 0);
            renderGraph.AddBlitPass(parameters, PassName);
            resourceData.cameraColor = destination;
        }

        public void Dispose()
        {
            temporaryColor?.Release();
            temporaryColor = null;
        }

        private bool CanRun(ref RenderingData renderingData)
        {
            if (activeMaterial == null || activeSettings == null || !activeSettings.IsEffectActive)
            {
                return false;
            }

            ref readonly CameraData cameraData = ref renderingData.cameraData;
            return cameraData.camera != null
                && cameraData.cameraType == CameraType.Game
                && !cameraData.isSceneViewCamera
                && !cameraData.isPreviewCamera;
        }

        private static bool CanRun(UniversalCameraData cameraData)
        {
            return cameraData.camera != null
                && cameraData.isGameCamera
                && !cameraData.isSceneViewCamera
                && !cameraData.isPreviewCamera;
        }

        private static void UpdateMaterialProperties(Material materialToUpdate, VHSFilterSettings settings)
        {
            materialToUpdate.SetFloat(IntensityId, settings.Intensity);
            materialToUpdate.SetFloat(ScanlineStrengthId, settings.ScanlineStrength);
            materialToUpdate.SetFloat(NoiseStrengthId, settings.NoiseStrength);
            materialToUpdate.SetFloat(JitterStrengthId, settings.JitterStrength);
            materialToUpdate.SetFloat(ChromaticAberrationId, settings.ChromaticAberration);
            materialToUpdate.SetFloat(TrackingBandStrengthId, settings.TrackingBandStrength);
            materialToUpdate.SetFloat(TrackingBandSpeedId, settings.TrackingBandSpeed);
        }
    }
}
