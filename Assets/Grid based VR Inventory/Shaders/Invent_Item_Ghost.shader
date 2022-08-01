Shader "Custom/Invent_Item_Ghost"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _Color("Color (RGBA)", Color) = (1, 1, 1, 1) 
    }

        SubShader
        {
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
			LOD 200

            ZWrite on            
            Blend SrcAlpha OneMinusSrcAlpha            

            Pass
            {
				Stencil {
					ref 1
					comp Equal
				}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag            
				#pragma target 3.0

				#include "UnityCG.cginc"            
				struct appdata_t
				{
					float4 vertex   : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					half2 texcoord  : TEXCOORD0;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _Color;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;					

					return col;
				}
				ENDCG
            }
        }
}