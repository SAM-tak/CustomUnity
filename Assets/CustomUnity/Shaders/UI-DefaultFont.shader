// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/CustomUnity/Default Font" {
    Properties {
        [PerRendererData] _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Range(0, 255)) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Range(0, 255)) = 255
        _StencilReadMask ("Stencil Read Mask", Range(0, 255)) = 255

        _ColorMask ("Color Mask", Range(0, 15)) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    FallBack "UI/CustomUnity/Default"

    CustomEditor "CustomUnity.UIDefaultShaderGUI"
}
