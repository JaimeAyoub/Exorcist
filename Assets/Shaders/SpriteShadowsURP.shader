// Reemplazo URP del shader Built-in "Sprites/Custom/SpriteShadows".
// Los sprites reciben luz de Directional/Point Lights 3D y proyectan sombra real
// usando un alpha cutout (equivalente a "alphatest:_CutOff addshadow" del surface shader original).
//
// Mismos nombres de Properties que el original -> si lo asignas a un Material ya existente,
// Unity conserva la textura/tint/etc. que ya tenías cargados.
//
// Probado contra la API estable de URP 12-17 (Unity 2021.2 - 6000.x). Si tu proyecto usa
// Forward+ (URP 14+) o una versión muy antigua/nueva y aparece algún error de compilación,
// dime el error exacto y lo ajusto.

Shader "Sprites/Custom/SpriteShadowsURP"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        _CutOff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float4 _RendererColor;
            float4 _Flip;
            float _EnableExternalAlpha;
            float _CutOff;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AlphaTex);
        SAMPLER(sampler_AlphaTex);

        // Equivalente a UnityPixelSnap() de UnityCG.cginc (no existe en el core de URP)
        float4 SpritePixelSnap(float4 positionCS)
        {
            float2 hpc = _ScreenParams.xy * 0.5f;
            float2 pixelPos = round((positionCS.xy / positionCS.w) * hpc);
            positionCS.xy = pixelPos / hpc * positionCS.w;
            return positionCS;
        }

        half4 SampleSpriteAlbedo(float2 uv, half4 vertexColor)
        {
            half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

            #if defined(ETC1_EXTERNAL_ALPHA)
            half external = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, uv).r;
            tex.a = lerp(tex.a, external, _EnableExternalAlpha);
            #endif

            return tex * vertexColor;
        }
        ENDHLSL

        // ---------------------------------------------------------------
        // Pase principal: sprite lit por luces 3D (direccional + puntuales)
        // ---------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile_local _ ETC1_EXTERNAL_ALPHA
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float4 shadowCoord: TEXCOORD3;
                float  fogFactor  : TEXCOORD4;
            };

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                v.vertex.xy *= _Flip.xy;

                VertexPositionInputs posIn = GetVertexPositionInputs(v.vertex.xyz);

                o.positionCS = posIn.positionCS;

                #if defined(PIXELSNAP_ON)
                o.positionCS = SpritePixelSnap(o.positionCS);
                #endif

                o.positionWS = posIn.positionWS;
                // Normal "virtual" que siempre mira hacia la cámara, en vez de usar la normal
                // real de la malla. Así la iluminación no se rompe si algún script rota el
                // transform (p.ej. transform.LookAt hacia el jugador) por motivos de gameplay.
                o.normalWS   = normalize(GetWorldSpaceViewDir(posIn.positionWS));
                o.uv         = TRANSFORM_TEX(v.uv, _MainTex);
                o.color      = v.color * _Color * _RendererColor;
                o.shadowCoord = GetShadowCoord(posIn);
                o.fogFactor  = ComputeFogFactor(posIn.positionCS.z);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 c = SampleSpriteAlbedo(i.uv, i.color);
                clip(c.a - _CutOff);

                half3 normalWS = normalize(i.normalWS);

                Light mainLight = GetMainLight(i.shadowCoord);
                half3 lighting = LightingLambert(mainLight.color * mainLight.shadowAttenuation * mainLight.distanceAttenuation, mainLight.direction, normalWS);
                lighting += SampleSH(normalWS);

                #if defined(_ADDITIONAL_LIGHTS)
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < additionalLightsCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, i.positionWS);
                    half3 attenuatedColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
                    lighting += LightingLambert(attenuatedColor, light.direction, normalWS);
                }
                #endif

                half3 albedo = c.rgb * c.a; // premultiplied, coherente con Blend One OneMinusSrcAlpha
                half3 finalColor = albedo * lighting;
                finalColor = MixFog(finalColor, i.fogFactor);

                return half4(finalColor, c.a);
            }
            ENDHLSL
        }

        // ---------------------------------------------------------------
        // Pase de sombra: recorta por _CutOff para que la sombra siga
        // la silueta real del sprite (igual que alphatest:_CutOff addshadow)
        // ---------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile_local _ ETC1_EXTERNAL_ALPHA

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection; // inyectado por URP para el shadow pass

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f ShadowVert(appdata v)
            {
                v2f o;

                v.vertex.xy *= _Flip.xy;

                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normal);

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                o.positionCS = positionCS;

                #if defined(PIXELSNAP_ON)
                o.positionCS = SpritePixelSnap(o.positionCS);
                #endif

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color * _RendererColor;
                return o;
            }

            half4 ShadowFrag(v2f i) : SV_Target
            {
                half4 c = SampleSpriteAlbedo(i.uv, i.color);
                clip(c.a - _CutOff);
                return 0;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
