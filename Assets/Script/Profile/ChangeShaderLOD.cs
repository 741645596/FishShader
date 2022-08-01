using UnityEngine;
using System.Collections;
/// <summary>
/// https://www.pianshen.com/article/58131129400/
/// </summary>
public class ChangeShaderLOD : MonoBehaviour
{
    public Shader myShader;

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 20, 200, 50), "shader 600"))
        {
            myShader.maximumLOD = 600;
        }
        if (GUI.Button(new Rect(215, 20, 200, 50), "shader 500"))
        {
            myShader.maximumLOD = 500;
        }
        if (GUI.Button(new Rect(420, 20, 200, 50), "shader 400"))
        {
            myShader.maximumLOD = 400;
        }
        if (GUI.Button(new Rect(625, 20, 200, 50), "shader 300"))
        {
            myShader.maximumLOD = 300;
        }

        if (GUI.Button(new Rect(10, 80, 200, 50), "global 600"))
        {
            //myShader.maximumLOD = -1;
            Shader.globalMaximumLOD = 600;
        }
        if (GUI.Button(new Rect(215, 80, 200, 50), "global 500"))
        {
            //myShader.maximumLOD = -1;
            Shader.globalMaximumLOD = 500;
        }
        if (GUI.Button(new Rect(420, 80, 200, 50), "global 400"))
        {
            //myShader.maximumLOD = -1;
            Shader.globalMaximumLOD = 400;
        }
        if (GUI.Button(new Rect(625, 80, 200, 50), "global 300"))
        {
            // myShader.maximumLOD = -1;
            Shader.globalMaximumLOD = 300;
        }
    }
}
