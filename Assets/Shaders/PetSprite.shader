Shader "Moonlit/PetSprite"
{
    Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _Cutoff("Discard faint transparent fringe",Range(0,1))=0.35 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float _Cutoff;
            CBUFFER_END
            struct App {float3 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            struct Vary {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            Vary Vert(App v){Vary o;o.positionCS=TransformObjectToHClip(v.positionOS);o.uv=v.uv;o.color=v.color;return o;}
            half4 Frag(Vary v):SV_Target{half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,v.uv);clip(c.a-_Cutoff);return c*v.color;}
            ENDHLSL
        }
    }
}
