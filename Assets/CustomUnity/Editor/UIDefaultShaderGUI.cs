using UnityEditor;
using UnityEngine.Rendering;

namespace CustomUnity
{
    internal class UIDefaultShaderGUI : ShaderGUI
    {
        public override void OnGUI(UnityEditor.MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            MaterialProperty color = FindProperty("_Color", properties);
            MaterialProperty stencilComp = FindProperty("_StencilComp", properties);
            MaterialProperty stencil = FindProperty("_Stencil", properties);
            MaterialProperty stencilOp = FindProperty("_StencilOp", properties);
            MaterialProperty stencilWriteMask = FindProperty("_StencilWriteMask", properties);
            MaterialProperty stencilReadMask = FindProperty("_StencilWriteMask", properties);
            MaterialProperty colorMask = FindProperty("_ColorMask", properties);
            MaterialProperty alphaClip = FindProperty("_UseUIAlphaClip", properties);

            materialEditor.ColorProperty(color, color.displayName);
            EnumPopup<CompareFunction>(materialEditor, stencilComp);
            RangeProperty(materialEditor, stencil);
            EnumPopup<StencilOp>(materialEditor, stencilOp);
            RangeProperty(materialEditor, stencilWriteMask);
            RangeProperty(materialEditor, stencilReadMask);
            RangeProperty(materialEditor, colorMask);
            ShaderProperty(materialEditor, alphaClip);

            EditorGUILayout.Space();

            materialEditor.RenderQueueField();
            materialEditor.DoubleSidedGIField();
        }

        void ShaderProperty(UnityEditor.MaterialEditor materialEditor, MaterialProperty property)
        {
            if(property != null) materialEditor.ShaderProperty(property, property.displayName);
        }

        void RangeProperty(UnityEditor.MaterialEditor materialEditor, MaterialProperty property)
        {
            if(property != null) materialEditor.RangeProperty(property, property.displayName);
        }

        void EnumPopup<T>(UnityEditor.MaterialEditor materialEditor, MaterialProperty property)
        {
            if(property != null) {
                EditorGUI.showMixedValue = property.hasMixedValue;
                var mode = (int)property.floatValue;

                EditorGUI.BeginChangeCheck();
                mode = EditorGUILayout.Popup(property.displayName, mode, System.Enum.GetNames(typeof(T)));
                if(EditorGUI.EndChangeCheck()) {
                    materialEditor.RegisterPropertyChangeUndo(property.name);
                    property.floatValue = mode;
                }

                EditorGUI.showMixedValue = false;
            }
        }
    }
}
