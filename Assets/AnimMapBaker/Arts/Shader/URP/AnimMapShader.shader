/*
Created by Arthur Wang
*/

Shader "AnimBaker/URP/AnimMapShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AnimMap ("AnimMap", 2D) ="white" {}
		_AnimLen("Anim Length", Float) = 0
	    _PlayAnim("Play Anim", Float) = 0
	    _RowNum("Row Num", Int) = 1
	}
	
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline"}
        Cull off

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 pos : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            CBUFFER_START(UnityPerMaterial)
                float _PlayAnim;
                float _AnimLen;
                sampler2D _MainTex;
                float4 _MainTex_ST;
                sampler2D _AnimMap;
                float4 _AnimMap_TexelSize;//x == 1/width
                int _RowNum;
            CBUFFER_END 
            
            float4 ObjectToClipPos (float3 pos)
            {
                return mul (UNITY_MATRIX_VP, mul (UNITY_MATRIX_M, float4 (pos,1)));
            }
            
            v2f vert (appdata v, uint vid : SV_VertexID)
            {
                UNITY_SETUP_INSTANCE_ID(v);

                float4 pos = v.pos;
                if(_PlayAnim > 0.5)
                {
                    float f = _Time.y / _AnimLen;
                    
                    float animMap_x = (vid + 0.5) * _AnimMap_TexelSize.x;
                    float animMap_y = fmod(f, 1.0);
                    // 比例数据离散化
                    int row = floor(animMap_y / (_AnimMap_TexelSize.y * _RowNum));
                    animMap_y = (row * _RowNum + 0.5) * _AnimMap_TexelSize.y;
                    
                    if (vid >= 2048) // Maybe can be uniform
                    {
                        animMap_x = (vid - 2048 + 0.5) * _AnimMap_TexelSize.x;
                        animMap_y = (row * _RowNum + 1.5) * _AnimMap_TexelSize.y;
                    }
                    pos = tex2Dlod(_AnimMap, float4(animMap_x, animMap_y, 0, 0));
                }

                v2f o;
                float3 pos3 = pos.xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertex = ObjectToClipPos(pos3);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDHLSL
        }
	}
}
