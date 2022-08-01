using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// https://docs.unity3d.com/2020.2/Documentation/Manual/ProfilerRendering.html?continueFlag=a795970d585940c1b9ba34c4e7cb6a3e
/// https://docs.unity3d.com/2020.2/Documentation/Manual/ProfilerMemory.html
/// </summary>
public class RenderStatsScript : MonoBehaviour
{
    public Text m_Context;
    string statsText;
    // 渲染数据
    ProfilerRecorder setPassCallsRecorder;
    ProfilerRecorder drawCallsRecorder;
    ProfilerRecorder verticesRecorder;
    ProfilerRecorder TrianglesRecorder;
    // 内存数据
    ProfilerRecorder totalReservedMemoryRecorder;
    ProfilerRecorder gcReservedMemoryRecorder;
    ProfilerRecorder systemUsedMemoryRecorder;
    ProfilerRecorder TextureUsedMemoryRecorder;
    ProfilerRecorder MeshUsedMemoryRecorder;
    ProfilerRecorder MaterialUsedMemoryRecorder;
    ProfilerRecorder AnimationUsedMemoryRecorder;

    void OnEnable()
    {
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
        TrianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        //
        totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
        gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        TextureUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
        MeshUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");
        MaterialUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Material Memory");
        AnimationUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "AnimationClip Memory");
    }

    void OnDisable()
    {
        setPassCallsRecorder.Dispose();
        drawCallsRecorder.Dispose();
        verticesRecorder.Dispose();
        TrianglesRecorder.Dispose();
        //
        totalReservedMemoryRecorder.Dispose();
        gcReservedMemoryRecorder.Dispose();
        systemUsedMemoryRecorder.Dispose();
        TextureUsedMemoryRecorder.Dispose();
        MeshUsedMemoryRecorder.Dispose();
        MaterialUsedMemoryRecorder.Dispose();
        AnimationUsedMemoryRecorder.Dispose();
    }

    void Update()
    {
        var sb = new StringBuilder(500);
        if (setPassCallsRecorder.Valid)
            sb.AppendLine($"SetPass Calls: {setPassCallsRecorder.LastValue}");
        if (drawCallsRecorder.Valid)
            sb.AppendLine($"Draw Calls: {drawCallsRecorder.LastValue}");
        if (verticesRecorder.Valid)
            sb.AppendLine($"Vertices: {verticesRecorder.LastValue}");
        if (TrianglesRecorder.Valid)
            sb.AppendLine($"Triangles: {TrianglesRecorder.LastValue}");
        // 内存数据
        if (totalReservedMemoryRecorder.Valid)
            sb.AppendLine($"Total Reserved Memory: {totalReservedMemoryRecorder.LastValue}");
        if (gcReservedMemoryRecorder.Valid)
            sb.AppendLine($"GC Reserved Memory: {gcReservedMemoryRecorder.LastValue}");
        if (systemUsedMemoryRecorder.Valid)
            sb.AppendLine($"System Used Memory: {systemUsedMemoryRecorder.LastValue}");
        if (TextureUsedMemoryRecorder.Valid)
            sb.AppendLine($"Texture Used Memory: {TextureUsedMemoryRecorder.LastValue}");
        if(MeshUsedMemoryRecorder.Valid)
            sb.AppendLine($"Mesh Used Memory: {MeshUsedMemoryRecorder.LastValue}");
        if(MaterialUsedMemoryRecorder.Valid)
            sb.AppendLine($"Material Used Memory: {MaterialUsedMemoryRecorder.LastValue}");
        if(AnimationUsedMemoryRecorder.Valid)
            sb.AppendLine($"Animation Used Memory: {AnimationUsedMemoryRecorder.LastValue}");

        statsText = sb.ToString();
        m_Context.text = statsText;
    }
}
