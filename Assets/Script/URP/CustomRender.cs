using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CustomRender : ScriptableRendererFeature
{
    CustomRenderPass customRenderPass;
    [System.Serializable]
    public class BlurSetting
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;
        public Material material = null;

    }

    public BlurSetting setting = new BlurSetting();

    public override void Create()
    {
        customRenderPass = new CustomRenderPass("a");
        customRenderPass.passMat = setting.material;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        // renderer.cameraColorTarget就是管线渲染出来的图像，将它传给pass
        customRenderPass.Setup(src);
        renderer.EnqueuePass(customRenderPass);
    }
}
