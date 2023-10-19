Shader "Lit/GoochBump"
{
    Properties
    {
        [Normal] _BumpMap ("Bumpmap", 2D) = "bump" {}

        _DiffuseColor ("DiffuseColor", Color) = (1.0, 1.0, 1.0, 1.0)
        _CoolScale ("CoolScale", Range(0.0, 1.0)) = 1.0
        _WarmScale ("WarmScale", Range(0.0, 1.0)) = 1.0
        _DiffuseLower ("DiffuseLower", Range(-1.0, 1.0)) = 0.0
        _DiffuseUpper ("DiffuseUpper", Range(0.0, 1.0)) = 0.0
        _SpecScale ("SpecReflectionConstant", float) = 1.0
        _SpecRoughness ("SpecRoughness", Range(0.0, 500.0)) = 32.0
        _OutlineThickness ("OutlineThickness", Range(0.0, 25.0)) = 1.0
        _OutlineColor ("OutlineColor", Color) = (0.0, 0.0, 0.0, 1.0)
    }
    SubShader
    {
        Pass
        {
            Tags {"LightMode"="ForwardBase"}
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);

                o.uv = v.texcoord;

                return o;
            }
            
            sampler2D _BumpMap;

            uniform fixed4 _DiffuseColor;
            uniform float _CoolScale;
            uniform float _WarmScale;
            uniform float _DiffuseLower;
            uniform float _DiffuseUpper;
            uniform float _SpecScale;
            uniform int _SpecRoughness;

            fixed4 frag (v2f i) : SV_Target
            {
                i.normal /= length(i.normal);
                float3 tex_normal = UnpackNormal(tex2D(_BumpMap, i.uv));
                tex_normal = tex_normal.xzy;
                tex_normal = normalize(tex_normal);
                tex_normal.y = 1.0f - tex_normal.y;
                i.normal = normalize(i.normal + tex_normal);
                i.viewDir /= length(i.viewDir);
                fixed4 color;

                fixed4 I_diffuse;
                fixed4 k_cool = fixed4(0.0, 0.0, _CoolScale, 1.0);
                k_cool.xyz += _DiffuseLower * _DiffuseColor.xyz;
                fixed4 k_warm = fixed4(_WarmScale, _WarmScale, 0.0, 1.0);
                k_warm.xyz += _DiffuseUpper * _DiffuseColor.xyz;
                float val = -dot(_WorldSpaceLightPos0.xyz, i.normal);
                I_diffuse = ((1+val)/2) * k_cool + (1 - (1+val)/2) * k_warm; 

                float3 reflection_vec = 2 * (dot(_WorldSpaceLightPos0, i.normal)) * i.normal - _WorldSpaceLightPos0;

                fixed4 I_spec;
                I_spec = _LightColor0 * clamp(_SpecScale * pow(dot(reflection_vec, i.viewDir), floor(_SpecRoughness)), 0.0, 1.0);
                
                color = I_diffuse + I_spec;

                // color = fixed4(i.normal, 1.0f);

                return color;
            }
            ENDCG
        }
        Pass 
        {
            Cull Front

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BumpMap;

            float _OutlineThickness;

            float4 vert(float4 position : POSITION, float3 normal : NORMAL) : SV_POSITION {
                float4 clipPos = UnityObjectToClipPos(position);
                float3 clipNorm = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M, normal));

                clipPos.xy += normalize(clipNorm.xy) * _OutlineThickness * clipPos.w * 2.0 / _ScreenParams.xy;

                return clipPos;
            }

            uniform fixed4 _OutlineColor;

            fixed4 frag() : SV_TARGET {
                return _OutlineColor;
            }

            ENDCG

        }
    }
}