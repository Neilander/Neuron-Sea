using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderManager : MonoBehaviour
{
    public Material screenDissolve;

    private void Start()
    {
        screenDissolve.SetFloat("_KaiShiShiJian", -10f);
    }

    private void Update()
    {
        Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime);
    }
}
