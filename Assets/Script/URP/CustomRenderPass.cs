using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderPass : ScriptableRenderPass
{
    public Material passMat = null;
    private RenderTargetIdentifier passSource { get; set; }

    string passTag;

    public CustomRenderPass(string tag)
    {
        passTag = tag;
    }

    public void Setup(RenderTargetIdentifier sour)
    {
        this.passSource = sour;
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(passTag);
        cmd.Blit(passSource, passSource, passMat);

        // 找到CustomVolume1组件，如果没找到或者未开启，直接return
        var stack = VolumeManager.instance.stack;
        CustomVolume1 customVolume1 = stack.GetComponent<CustomVolume1>();
        if (customVolume1 == null) { return; }
        if (!customVolume1.IsActive()) return;

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}