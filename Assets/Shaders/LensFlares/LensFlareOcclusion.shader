Shader "+SummerCatchers/Special/LensFlare/Occlusion"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        { 
            "Queue" = "Transparent"
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "ForceNoShadowCasting"="True"
        }

        Cull Off
        Lighting Off
        ZTest Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            static const fixed3 MainOpaqueColor = fixed3(0.0, 0.0, 0.0);
            #include "LensFlaresCG.cginc"
        ENDCG
        }
    }

    SubShader
    {
        Tags
        { 
            "Queue" = "Transparent"
            "IgnoreProjector"="True" 
            "RenderType"="Occludie" 
            "PreviewType"="Plane"
            "ForceNoShadowCasting"="True"
        }

        Cull Off
        Lighting Off
        ZTest Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            static const fixed3 MainOpaqueColor = fixed3(1.0, 1.0, 1.0);
            #include "LensFlaresCG.cginc"
        ENDCG
        }
    }
}
