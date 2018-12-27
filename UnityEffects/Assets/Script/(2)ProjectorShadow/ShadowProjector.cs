using UnityEngine;
using System.Collections;
using Assets.Script.Utils;
using System.Collections.Generic;

public class ShadowProjector : MonoBehaviour 
{
    private Projector _projector;
    //
    private Camera _lightCamera = null;
    private RenderTexture _shadowTex;
    //
    private Camera _mainCamera;
    private List<Renderer> _shadowCasterList = new List<Renderer>();
    private BoxCollider _boundsCollider;
    public float boundsOffset = 1;
    public Shader shadowReplaceShader;
	void Start () 
    {
        _projector = GetComponent<Projector>();
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        //
        if(_lightCamera == null)
        {
            _lightCamera = gameObject.AddComponent<Camera>();
            _lightCamera.orthographic = true;
            _lightCamera.cullingMask = LayerMask.GetMask("ShadowCaster");
            _lightCamera.clearFlags = CameraClearFlags.SolidColor;
            _lightCamera.backgroundColor = new Color(0,0,0,0);
            _shadowTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            _shadowTex.filterMode = FilterMode.Bilinear;
            _lightCamera.targetTexture = _shadowTex;
            _lightCamera.SetReplacementShader(shadowReplaceShader, "RenderType");
            _projector.material.SetTexture("_ShadowTex", _shadowTex);
            _projector.ignoreLayers = LayerMask.GetMask("ShadowCaster");
        }
         GameObject plane = GameObject.Find("Plane");
         foreach (Transform trans in plane.transform)
         {
             if(trans.gameObject.layer == LayerMask.NameToLayer("ShadowCaster"))
             {
                 _shadowCasterList.Add(trans.gameObject.GetComponent<Renderer>());
             }
         }

        _boundsCollider = new GameObject("Test use to show bounds").AddComponent<BoxCollider>();
	}

    void LateUpdate()
    {
        //calculate bounding box of the shadow caster
        Bounds b = new Bounds();
        for (int i = 0; i < _shadowCasterList.Count; i++)
        {
            if(_shadowCasterList[i] != null)
            {
                b.Encapsulate(_shadowCasterList[i].bounds);
            }
        }
        b.extents += Vector3.one * boundsOffset;
#if UNITY_EDITOR
        _boundsCollider.center = b.center;
        _boundsCollider.size = b.size;
#endif
        //according to the mainCamera,updating position of lightCamera and projector 
        ShadowUtils.SetLightCamera(b, _lightCamera);
        _projector.aspectRatio = _lightCamera.aspect;
        _projector.orthographicSize = _lightCamera.orthographicSize;
        _projector.nearClipPlane = _lightCamera.nearClipPlane;
        _projector.farClipPlane = _lightCamera.farClipPlane;
	}
}
