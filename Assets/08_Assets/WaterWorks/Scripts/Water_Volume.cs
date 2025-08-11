using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Water_Volume : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public RTHandle source;                 // ✅ RTHandle로 변경

        private Material _material;

        private RTHandle tempRenderTarget;      // ✅ RTHandle
        private RTHandle tempRenderTarget2;     // (필요시 사용)

        public CustomRenderPass(Material mat)
        {
            _material = mat;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Reflection)
                return;

            var cmd = CommandBufferPool.Get(nameof(Water_Volume));

            // 카메라 타겟과 동일 스펙으로 임시 RT 확보
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(
                ref tempRenderTarget, desc,
                FilterMode.Bilinear, TextureWrapMode.Clamp,
                name: "_TemporaryColourTexture"
            );

            // 필요하다면 depth용도 동일하게
            // RenderingUtils.ReAllocateIfNeeded(ref tempRenderTarget2, desc, ... , name: "_TemporaryDepthTexture");

            // ✅ URP 권장 블릿 방식
            Blitter.BlitCameraTexture(cmd, source, tempRenderTarget, _material, 0);
            Blitter.BlitCameraTexture(cmd, tempRenderTarget, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // RTHandle은 카메라 종료시 릴리즈
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (tempRenderTarget != null)
            {
                tempRenderTarget.Release();
                tempRenderTarget = null;
            }
            if (tempRenderTarget2 != null)
            {
                tempRenderTarget2.Release();
                tempRenderTarget2 = null;
            }
        }
    }

    [System.Serializable]
    public class _Settings
    {
        public Material material = null;
        public RenderPassEvent renderPass = RenderPassEvent.AfterRenderingSkybox;
    }

    public _Settings settings = new _Settings();
    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        if (settings.material == null)
            settings.material = (Material)Resources.Load("Water_Volume");

        m_ScriptablePass = new CustomRenderPass(settings.material);
        m_ScriptablePass.renderPassEvent = settings.renderPass;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // ✅ URP 12+ : cameraColorTargetHandle 사용
#if UNITY_2021_2_OR_NEWER
        m_ScriptablePass.source = renderer.cameraColorTargetHandle;
#else
        // 구(舊) URP 호환 (필요시)
        m_ScriptablePass.source = RTHandles.Alloc(renderer.cameraColorTarget, name: "_CameraColorTargetCompat");
#endif

        renderer.EnqueuePass(m_ScriptablePass);
    }
}
