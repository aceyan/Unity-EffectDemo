using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Script.Utils;

/// <summary>
/// to generate depthmap
/// </summary>
public class CreateDepthMap : MonoBehaviour 
{
    public Shader depthMapShader;
    private Camera _mainCamera;
    private Camera _lightCamera;
    private List<Vector4> _vList = new List<Vector4>();
	void Start () 
    {
        _lightCamera = GetComponent<Camera>();
        _lightCamera.depthTextureMode = DepthTextureMode.Depth;
        _lightCamera.clearFlags = CameraClearFlags.SolidColor;
        _lightCamera.backgroundColor = Color.white;//setting backgroundColor to white, it means that the background is farthest from the viewpoint and is not affected by shadows
        _lightCamera.SetReplacementShader(depthMapShader, "RenderType");//using the renderType feature to generate depthmap
        RenderTexture depthMap = new RenderTexture(Screen.width, Screen.height, 0);
        depthMap.format = RenderTextureFormat.ARGB32;
        _lightCamera.targetTexture = depthMap;
        //
        foreach (Camera item in Camera.allCameras)
        {
            if (item.CompareTag("MainCamera"))
            {
                _mainCamera = item;
                break;
            }
        }
	}

    void LateUpdate()
    {
        ShadowUtils.SetLightCamera(_mainCamera, _lightCamera);
    }
}
