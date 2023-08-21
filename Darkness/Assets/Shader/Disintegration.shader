Shader "TNTC/Disintegration"
{
    Properties
    {
        _BaseTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        [HDR]_AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpStr("Normal Map Strenght", float) = 1
 
        _FlowMap("Flow (RG)", 2D) = "black" {}
        _DissolveTexture("Dissolve Texutre", 2D) = "white" {}
        _DissolveColor("Dissolve Color Border", Color) = (1, 1, 1, 1) 
        _DissolveBorder("Dissolve Border", float) =  0.05


        _Exapnd("Expand", float) = 1
        _Weight("Weight", Range(0,1)) = 0
        _Direction("Direction", Vector) = (0, 0, 0, 0)
        [HDR]_DisintegrationColor("Disintegration Color", Color) = (1, 1, 1, 1)
        _Glow("Glow", float) = 1

        _Shape("Shape Texutre", 2D) = "white" {} 
        _R("Radius", float) = .1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "HDRenderPipeline" }
        LOD 100
        Cull Off

        HLSLINCLUDE

        #include "Assets/Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Assets/Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        #pragma prefer_hlslcc gles
        #pragma exclude_renderers d3d11_9x
        #pragma target 4.6
        #pragma require geometry
        
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ _ADDITIONAL_LIGHTS
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT

        #pragma multi_compile _ SHADOWS_DEPTH
        #pragma multi_compile _ SHADOWS_CUBE
        #pragma multi_compile _ DIRECTIONAL
        #pragma multi_compile _ LIGHTMAP_ON 
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED 
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON 
        #pragma multi_compile _ SHADOWS_SCREEN 
        #pragma multi_compile _ SHADOWS_SHADOWMASK 
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING 
        #pragma multi_compile _ LIGHTPROBE_SH


        #pragma vertex vert
        #pragma fragment frag
        #pragma geometry geom


        TEXTURE2D(_BaseTex);            SAMPLER(sampler_BaseTex);
        TEXTURE2D(_DissolveTexture);    SAMPLER(sampler_DissolveTexture);
        TEXTURE2D(_Shape);              SAMPLER(sampler_Shape);
        TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
        TEXTURE2D(_FlowMap);            SAMPLER(sampler_FlowMap);

        

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseTex_ST;
        float4 _Color;
        float4 _AmbientColor; 
        float _BumpStr;
        float _Metallic;

        float4 _FlowMap_ST;
        float4 _DissolveColor;
        float _DissolveBorder;

        float _Exapnd;
        float _Weight;
        float4 _Direction;
        float4 _DisintegrationColor;
        float _Glow;
        float _R;
        CBUFFER_END

        

        #define UNITY_MATRIX_TEXTURE0 float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1)

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct v2g
        {
            float4 objPos : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 normal : TEXCOORD3;
            float3 worldPos : TEXCOORD1;
        };

        struct g2f
        {
            float4 worldPos : SV_POSITION;
            float2 uv : TEXCOORD0;
            half4 color : COLOR;
            float3 normal : TEXCOORD3;
        };

        // Vertex function
        v2g vert (appdata v)
        {
            v2g o;
            o.objPos = v.vertex;
            o.uv = v.uv;
            o.normal = TransformObjectToWorldNormal(v.normal);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            return o;
        }

        float random (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
        }

        float remap (float value, float from1, float to1, float from2, float to2) 
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        float randomMapped(float2 uv, float from, float to)
        {
            return remap(random(uv), 0, 1, from, to);
        }

        float4 remapFlowTexture(float4 tex)
        {
            return float4(
                remap(tex.x, 0, 1, -1, 1),
                remap(tex.y, 0, 1, -1, 1),
                0,
                remap(tex.w, 0, 1, -1, 1)
            );
        }

        float2 MultiplyUV (float4x4 mat, float2 inUV) 
        {
            float4 temp = float4 (inUV.x, inUV.y, 0, 0);
            temp = mul (mat, temp);
            return temp.xy;
        }

        [maxvertexcount(7)]
        void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
        {
            float2 avgUV = (IN[0].uv + IN[1].uv + IN[2].uv) / 3;
            float3 avgPos = (IN[0].objPos + IN[1].objPos + IN[2].objPos) / 3;
            float3 avgNormal = (IN[0].normal + IN[1].normal + IN[2].normal) / 3;

            /* Dissolve map */
            float dissolve_value = SAMPLE_TEXTURE2D_LOD(_DissolveTexture, sampler_DissolveTexture, float4(avgUV, 0, 0), 0).r;
            float t = clamp(_Weight * 2 - dissolve_value, 0 , 1);

            /* Flow map */
            float2 flowUV = TRANSFORM_TEX(mul(unity_ObjectToWorld, avgPos).xz, _FlowMap);
            float4 flowVector = remapFlowTexture(SAMPLE_TEXTURE2D_LOD(_FlowMap, sampler_FlowMap, float4(flowUV, 0, 0), 0));

            /* Calculate final position */
            float3 pseudoRandomPos = (avgPos) + _Direction;
            pseudoRandomPos += (flowVector.xyz * _Exapnd);

            /* Weight interpolation */
            float3 p =  lerp(avgPos, pseudoRandomPos, t);
            float radius = lerp(_R, 0, t);
        
            /* If weight greater than 0 */
            if(t > 0)
            {
                float3 look = _WorldSpaceCameraPos - p;
                look = normalize(look);

                float3 right = UNITY_MATRIX_IT_MV[0].xyz;
                float3 up = UNITY_MATRIX_IT_MV[1].xyz;

                float halfS = 0.5f * radius;

                float4 v[4];
                v[0] = float4(p + halfS * right - halfS * up, 1.0f);
                v[1] = float4(p + halfS * right + halfS * up, 1.0f);
                v[2] = float4(p - halfS * right - halfS * up, 1.0f);
                v[3] = float4(p - halfS * right + halfS * up, 1.0f);
        

                /* Greyscale texture */

                g2f vert;
                vert.worldPos = TransformObjectToHClip(v[0]);
                vert.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, float2(1.0f, 0.0f));
                vert.color = float4(1, 1, 1, 1);
                vert.normal = avgNormal;
                triStream.Append(vert);

                vert.worldPos = TransformObjectToHClip(v[1]);
                vert.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, float2(1.0f, 1.0f));
                vert.color = float4(1, 1, 1, 1);
                vert.normal = avgNormal;
                triStream.Append(vert);

                vert.worldPos =TransformObjectToHClip(v[2]);
                vert.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, float2(0.0f, 0.0f));
                vert.color = float4(1, 1, 1, 1);
                vert.normal = avgNormal;
                triStream.Append(vert);

                vert.worldPos = TransformObjectToHClip(v[3]);
                vert.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, float2(0.0f, 1.0f));
                vert.color = float4(1, 1, 1, 1);
                vert.normal = avgNormal;
                triStream.Append(vert);

                triStream.RestartStrip();
            }


            for(int j = 0; j<3; j++)
            {
                g2f o;
                o.worldPos = TransformObjectToHClip(IN[j].objPos);
                o.uv = TRANSFORM_TEX(IN[j].uv, _BaseTex);
                o.color = half4(0, 0, 0, 0);
                o.normal = IN[j].normal;
                triStream.Append(o); 
            }
        
            triStream.RestartStrip();
        }

        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "Forward" }
        
            
            HLSLPROGRAM

            // Fragment function
            half4 frag (g2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv)  * _Color ;
            
                float3 normal = normalize(i.normal);
                half3 tnormal = UnpackNormal(SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv));
                tnormal.xy *= _BumpStr;
                tnormal = normalize(tnormal);

                float NdotL = dot((float4)0, normal * tnormal);
                float4 light = NdotL * (half4)0;
                col *= (_AmbientColor + light);
           
                float brightness = i.color.w  * _Glow;
                col = lerp(col, _DisintegrationColor,  i.color.x);

                if(brightness > 0)
                {
                    col *= brightness + _Weight;
                }

                float dissolve = SAMPLE_TEXTURE2D(_DissolveTexture, sampler_DissolveTexture, i.uv).r;
            
                if(i.color.w == 0)
                {
                    clip(dissolve - 2*_Weight);
                    if(_Weight > 0)
                    {
                        col +=  _DissolveColor * _Glow * step( dissolve - 2*_Weight, _DissolveBorder);
                    }
                }
                else
                {
                    float s = SAMPLE_TEXTURE2D(_Shape, sampler_Shape, i.uv).r;
                    if (s < .5) 
                    {
                        discard;
                    }
                }

                return col;
            }

            ENDHLSL
        }
               

        Pass
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }

            HLSLPROGRAM

            


            half4 frag(g2f i) : SV_Target
            {
                float dissolve = SAMPLE_TEXTURE2D(_DissolveTexture, sampler_DissolveTexture, i.uv).r;

                if(i.color.w == 0)
                {
                    clip(dissolve - 2 * _Weight);
                }
                else
                {
                    float s = SAMPLE_TEXTURE2D(_Shape, sampler_Shape, i.uv).r;
                    if(s < .5) 
                    {
                        discard;
                    }
                }

                return 0;
            }

            ENDHLSL
        }
    }
}
