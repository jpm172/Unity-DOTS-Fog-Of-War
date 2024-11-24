Shader "Universal Render Pipeline/Custom/DotsStencilMask"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Colour", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
    
        Tags { "RenderType"="Opaque" "Queue" = "Geometry-100" "RenderPipeline" = "UniversalPipeline" }
       

        Pass
        {
            ColorMask 0
            //Blend Zero One
            ZWrite Off
            
            Stencil
            {
            
                Ref 1
                Comp Always
                Pass Replace
     
            }

            Cull Back

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            uniform float4 _BaseMap_TexelSize;

            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
                #define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor)
            #endif

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                const VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                
                output.positionCS = positionInputs.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.color = float4(input.uv.x, input.uv.y, 0, 1);
                
                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 baseMap = half4(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv));
                
                
                return baseMap * _BaseColor * input.color;
            }
            ENDHLSL
        }
    }
}