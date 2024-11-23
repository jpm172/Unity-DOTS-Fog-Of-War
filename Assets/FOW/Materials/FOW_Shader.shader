Shader "Unlit/FOW_Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SeenTex ("Seen Texture", 2D) = "white" {}
        _ObstacleTex ("Obstacle Texture", 2D) = "white" {}
        
        _ObstacleColor ("Obstacle Color", Color) = (1,1,1,1) 
        _FloorColor ("Floor Color", Color) = (1,1,1,1) 
        _SeenDist ("Seen Distance", Int) = 4
       
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType"="Transparent"  }

        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Lighting off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 screenPos : POSITION_SS;
            };

            sampler2D _MainTex;
            sampler2D _SeenTex;
            sampler2D _ObstacleTex;
            
            float4 _ObstacleColor;
            float4 _FloorColor;
            
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _SeenTex_TexelSize;
            
            int _SeenDist;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos =  ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 obstacleCol = tex2D(_ObstacleTex, i.uv);
                fixed4 floorCol = tex2D(_SeenTex, i.uv);
             

                float seen = 0;
                float visible = 0;
                
                float4 startCol = _ObstacleColor;            
                
                if(obstacleCol.r > 0)
                {                
                    fixed4 visibleCol = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y*_SeenDist ));
                    visible = visibleCol.r;
                    visibleCol = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y*_SeenDist));
                    visible = max(visible,visibleCol.r);
                    visibleCol = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x*_SeenDist, 0));
                    visible = max(visible,visibleCol.r);
                    visibleCol = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x*_SeenDist, 0));
                    visible = max(visible,visibleCol.r);
                    
                        
                    if(visible < .1)
                    {
                        fixed4 seenCol = tex2D(_SeenTex, i.uv + float2(0, _MainTex_TexelSize.y*_SeenDist ));
                        seen = seenCol.r;
                        seenCol = tex2D(_SeenTex, i.uv - float2(0, _MainTex_TexelSize.y*_SeenDist));
                        seen = max(seen, seenCol.r);
                        seenCol = tex2D(_SeenTex, i.uv + float2(_MainTex_TexelSize.x*_SeenDist, 0));
                        seen = max(seen, seenCol.r);
                        seenCol = tex2D(_SeenTex, i.uv - float2(_MainTex_TexelSize.x*_SeenDist, 0));
                        seen = max(seen, seenCol.r);
                        
                        if(seen < 0.05)//prevent the walls from being marked as "seen" if close to 0
                            seen = 0;
                    }
                }
                else if(floorCol.r > 0.1)
                {
                    startCol = _FloorColor;
                    seen = 1;
                }
               
                
                float4 result = lerp(startCol*float4(seen, seen,seen, 1), float4(0,0,0,0), max(col.r, visible));

                return result;
            }
            
            
            
            ENDCG
            
        }
        
    }
}
