using System;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Character shader editor
/// </summary>
public class CharactarShaderGUI : ShaderGUI
    {
        public enum BlendMode
        {
            Opaque,
            Cutout,
            AlphaBlended,
            AlphaAdditve
        }
 
        public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));

        MaterialProperty blendMode = null;
        MaterialProperty alphaCutoff = null;
        MaterialEditor m_MaterialEditor;
        

        public void FindProperties(MaterialProperty[] props)
        {
            blendMode = FindProperty("_Mode", props);
            alphaCutoff = FindProperty("_AlphaCutoff", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            base.OnGUI(materialEditor, props);
            FindProperties(props); 
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;
            ShaderPropertiesGUI(material);

        }

        public void ShaderPropertiesGUI(Material material)
        {
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.BeginChangeCheck();
            {
                BlendModePopup();
            }
            if (EditorGUI.EndChangeCheck())
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
            }
        }

        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;
            float cutoff = alphaCutoff.floatValue;
            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup("BlendMode", (int)mode, blendNames);
            if (mode == BlendMode.Cutout)
            {
                cutoff = EditorGUILayout.Slider("Cutoff", cutoff, 0, 1);
            }
            if (EditorGUI.EndChangeCheck())
            {
                blendMode.floatValue = (float)mode;
                alphaCutoff.floatValue = cutoff;
            }

            EditorGUI.showMixedValue = false;
        }


        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                //Opaque
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.renderQueue = 2000;
                    break;
                //alphaTest
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.renderQueue = 2450;
                    break;
                //Blended
                case BlendMode.AlphaBlended:
                    material.SetOverrideTag("RenderType", "TransparentAlphaBlended");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.renderQueue = 3000;
                    break;
                //Additve
                case BlendMode.AlphaAdditve:
                    material.SetOverrideTag("RenderType", "TransparentAlphaAdditve");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.renderQueue = 3000;
                    break;
            }
        }
    }


