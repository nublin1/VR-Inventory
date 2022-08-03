Shader "Stencil/InvStencilWrite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"= "Geometry-1"}
        
        blend SrcAlpha OneMinusSrcAlpha
        Zwrite off

        Pass
        {
            Stencil {
                ref 1
                comp always
                pass replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag            

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                
            };

            struct v2f
            {
                
               
                float4 vertex : SV_POSITION;
            };

           

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
               
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {                
                return 0;
            }
            ENDCG
        }
    }
}
