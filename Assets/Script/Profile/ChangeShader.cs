using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// https://blog.csdn.net/liyaxin2010/article/details/83449410
/// 
/// </summary>
public class ChangeShader : MonoBehaviour
{
    public Shader myShader;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// ֻ�滻��ǰ֡
    /// </summary>
    public void RenderWithShader()
    {
        
    }

    /// <summary>
    /// ֮������֡�����滻��
    /// </summary>
    public void SetReplacementShader()
    {
        // ��myshader ȥ�滻�����е�shader ������Ⱦ
        // ���е������ʹ��myShader������Ⱦ
        //Camera.main.SetReplacementShader(myShader, "RenderType");
        //1. �����ڳ������ҵ���ǩ�а������ַ���������Ϊ��RenderType������Shader
        //2. ��ȥ�����ַ�����Ӧ����ֵ�Ƿ���Shader1�и��ַ�����ֵһ�£����һ�£��������Ⱦ��������Ⱦ
        Camera.main.SetReplacementShader(myShader, "RenderType");
        // �Ƚ�tag������ȷ��tag����һ��
        //Camera.main.SetReplacementShader(myShader, "CheckRenderTypeTag");
    }
}
