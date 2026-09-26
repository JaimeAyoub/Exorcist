// Reemplazo URP del shader Built-in "Sprites/Custom/SpriteShadows".
// Soporta Normal Maps, Specular Maps y proyecta/recibe sombras reales.
// Ajustado para pixel art: el flip de sprite ahora afecta correctamente normal + tangente.

Shader "Sprites/Custom/SpriteShadowsURP_Lit"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Normal and Specular)]
        [Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
        _SpecMap ("Specular Map (RGB=Color, A=Smoothness)", 2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5

        [Header(Settings)]
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        _CutOff ("Alpha Cutoff", Range(0,1)) = 0.5

        [Header(Erosion Dissolve Effect)]
        _ErosionTex ("Erosion Noise (R)", 2D) = "white" {}
        _ErosionAmount ("Erosion Amount", Range(0,1)) = 0
        _ErosionEdgeWidth ("Erosion Edge Width", Range(0.001, 0.3)) = 0.06
        _ErosionEdgeColor ("Erosion Edge Color", Color) = (1, 0.4, 0.05, 1)
        _ErosionEdgeIntensity ("Erosion Edge Intensity", Range(0, 5)) = 2
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
            float4 _SpecColor;
            float _Smoothness;
            float _ErosionAmount;
            float _ErosionEdgeWidth;
            float4 _ErosionEdgeColor;
            float _ErosionEdgeIntensity;
        CBUFFER_END

        TEXTURE2D(_MainTex);      SAMPLER(sampler_MainTex);
        TEXTURE2D(_AlphaTex);     SAMPLER(sampler_AlphaTex);
        TEXTURE2D(_NormalMap);    SAMPLER(sampler_NormalMap);
        TEXTURE2D(_SpecMap);      SAMPLER(sampler_SpecMap);
        TEXTURE2D(_ErosionTex);   SAMPLER(sampler_ErosionTex);

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
        // Pase principal: lit por luces 3D (difusa + especular + normal map)
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex  : POSITION;
                float3 normal  : NORMAL;
                float4 tangent : TANGENT; // Requiere que el sprite tenga tangentes (ver Secondary Textures)
                float4 color   : COLOR;
                float2 uv      : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
                float4 shadowCoord : TEXCOORD1;
                float  fogFactor   : TEXCOORD2;
                float3 positionWS  : TEXCOORD3;
                float3 normalWS    : TEXCOORD4;
                float3 tangentWS   : TEXCOORD5;
                float3 bitangentWS : TEXCOORD6;
                float3 viewDirWS   : TEXCOORD7;
            };

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                // --- FIX: el flip ahora se aplica a posición, normal Y tangente ---
                // Sin esto, al voltear el sprite (ej. personaje mirando a la izquierda)
                // la luz del normal map se veía como si viniera del lado equivocado.
                v.vertex.xy *= _Flip.xy;
                v.normal.xy *= _Flip.xy;
                v.tangent.xy *= _Flip.xy;

                // FIX: la normal base del sprite queda invertida respecto a la
                // convención de LookAt/LookRotation del billboard. La negamos
                // aquí (no al tangente) para no afectar la orientación del normal map.
                //v.normal = -v.normal;

                VertexPositionInputs posIn = GetVertexPositionInputs(v.vertex.xyz);
                VertexNormalInputs normalIn = GetVertexNormalInputs(v.normal, v.tangent);

                o.positionCS = posIn.positionCS;

                #if defined(PIXELSNAP_ON)
                o.positionCS = SpritePixelSnap(o.positionCS);
                #endif

                o.positionWS  = posIn.positionWS;
                o.normalWS    = normalIn.normalWS;
                o.tangentWS   = normalIn.tangentWS;
                o.bitangentWS = normalIn.bitangentWS;
                o.viewDirWS   = GetWorldSpaceViewDir(posIn.positionWS);
                
                o.uv          = TRANSFORM_TEX(v.uv, _MainTex);
                o.color       = v.color * _Color * _RendererColor;
                o.shadowCoord = GetShadowCoord(posIn);
                o.fogFactor   = ComputeFogFactor(posIn.positionCS.z);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 c = SampleSpriteAlbedo(i.uv, i.color);
                clip(c.a - _CutOff);

                // --- Erosion / Dissolve ---
                half erosionNoise = SAMPLE_TEXTURE2D(_ErosionTex, sampler_ErosionTex, i.uv).r;
                // Recorta el píxel por completo una vez que el ruido cae por debajo del umbral
                clip(erosionNoise - _ErosionAmount);
                // Máscara para el borde brillante: 1 justo en el límite de la erosión, 0 lejos de él
                half edgeMask = 1 - smoothstep(0, _ErosionEdgeWidth, erosionNoise - _ErosionAmount);

                // 1. Extraer y transformar el Normal Map
                half4 normalTex = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv);
                half3 tangentNormal = UnpackNormal(normalTex);
                half3 normalWS = TransformTangentToWorld(tangentNormal, half3x3(i.tangentWS, i.bitangentWS, i.normalWS));
                normalWS = normalize(normalWS);

                // 2. Extraer datos Especulares
                half4 specTex = SAMPLE_TEXTURE2D(_SpecMap, sampler_SpecMap, i.uv);
                half3 specColor = specTex.rgb * _SpecColor.rgb;
                half smoothness = specTex.a * _Smoothness; 
                half3 viewDirWS = normalize(i.viewDirWS);

                // 3. Iluminación Principal
                Light mainLight = GetMainLight(i.shadowCoord);
                half3 lightColorAtten = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
                
                half3 diffuse = LightingLambert(lightColorAtten, mainLight.direction, normalWS);
                half3 specular = LightingSpecular(lightColorAtten, mainLight.direction, normalWS, viewDirWS, half4(specColor, 0), smoothness);

                half3 totalDiffuse = diffuse;
                half3 totalSpecular = specular;

                // 4. Luces Adicionales (Point, Spot)
                #if defined(_ADDITIONAL_LIGHTS)
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < additionalLightsCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, i.positionWS);
                    half3 attenColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
                    
                    totalDiffuse += LightingLambert(attenColor, light.direction, normalWS);
                    totalSpecular += LightingSpecular(attenColor, light.direction, normalWS, viewDirWS, half4(specColor, 0), smoothness);
                }
                #endif

                // Añadir ambiente (Global Illumination)
                totalDiffuse += SampleSH(normalWS);

                // 5. Composición final (Premultiplied Alpha)
                half3 albedo = c.rgb * c.a; 
                
                // Multiplicamos totalSpecular por c.a para que las zonas transparentes no brillen
                half3 finalColor = (albedo * totalDiffuse) + (totalSpecular * c.a);

                // Suma el brillo del borde de erosión (efecto "quemado"/energía)
                finalColor += _ErosionEdgeColor.rgb * edgeMask * _ErosionEdgeIntensity * c.a;

                finalColor = MixFog(finalColor, i.fogFactor);

                return half4(finalColor, c.a);
            }
            ENDHLSL
        }

        // ---------------------------------------------------------------
        // Pase de sombra (Sin cambios, solo recorta silueta)
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

            float3 _LightDirection;

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

                half erosionNoise = SAMPLE_TEXTURE2D(_ErosionTex, sampler_ErosionTex, i.uv).r;
                clip(erosionNoise - _ErosionAmount);

                return 0;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
