using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderManager : MonoBehaviour
{
    private void Update()
    {
        Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime);
    }
}
