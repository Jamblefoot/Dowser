Shader "HolisticTutorial/ToonRampLiquidBottle"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _LiquidColor ("Liquid Color (water)", Color) = (0.5,0.5,0.5,1)
        _OilColor ("Oil Color", Color) = (0,0,0,1)
        _BioColor ("Bio Color", Color) = (1,0,0.2,1)
        _LiquidHeight ("Liquid Height", Range(-1, 1)) = 0
        _OilHeight ("Oil Height", Range(0, 1)) = 0
        _BioWeight ("Bio Weight", Range(0, 1)) = 0
        _HeightMult ("Height Mult", Float) = 1
        _RampTex ("Ramp Texture", 2D) = "white" {}

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline Width", Range(0.002, 1)) = 0.005

        _MainTex ("Main Texture", 2D) = "white" {}
        _Bump ("Bump Texture", 2D) = "bump" {}
    }
    SubShader
    {
        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        CGPROGRAM
        #pragma surface surf ToonRamp

        float4 _Color;
        float4 _LiquidColor;
        float4 _OilColor;
        float4 _BioColor;
        float _LiquidHeight;
        float _OilHeight;
        float _BioWeight;
        float _HeightMult;
        sampler2D _RampTex;

        float4 LightingToonRamp(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            float diff = dot(s.Normal, lightDir);
            float h = diff * 0.5 + 0.5;
            float2 rh = h * atten;
            float3 ramp = tex2D(_RampTex, rh).rgb;

            float4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * (ramp);
            c.a = s.Alpha;
            return c;
        }

        sampler2D _MainTex;
        sampler2D _Bump;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Bump;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float3 localPos = IN.worldPos - mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
            half4 liquidCol = lerp(_LiquidColor, _BioColor, _BioWeight);
            float oilLevel = _LiquidHeight * _HeightMult - (_LiquidHeight + 1) * _OilHeight * _HeightMult;
            half3 col = localPos.y > _LiquidHeight * _HeightMult ? _Color.rgb : localPos.y > oilLevel ? _OilColor.rgb : liquidCol.rgb;
            o.Albedo = col * tex2D(_MainTex, IN.uv_MainTex).rgb;
            o.Normal = tex2D(_Bump, IN.uv_Bump).rgb;
        }
        ENDCG

        Pass
        {
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

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                //CONVERT NORMAL TO WORLD COORDINATE
                float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                //NOW PROJECT NORMAL ON VIEWING PLANE
                float2 offset = TransformViewToProjection(norm.xy);

                o.pos.xy += offset * o.pos.z * _Outline;
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
