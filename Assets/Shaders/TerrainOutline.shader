Shader "Custom/TerrainOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline Width", Range(0.002, 1)) = 1
        _DistanceDivisor ("Distance Divisor", Float) = 50
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass
        {
            Name "OutlinePass"

            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };

            float _Outline;
            float4 _OutlineColor;
            float _DistanceDivisor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                //CONVERT NORMAL TO WORLD COORDINATE
                float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                //NOW PROJECT NORMAL ON VIEWING PLANE
                float2 offset = TransformViewToProjection(norm.xy);

                float dist = length(WorldSpaceViewDir(v.vertex));

                o.pos.xy += offset * o.pos.z * _Outline * (1 + dist / _DistanceDivisor);
                o.color = _OutlineColor;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
