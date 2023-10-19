Shader "Unlit/Toon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0.5, 0.65, 1, 1)
        [HDR]
        _AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        [HDR]
        _SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
        _Glossiness("Glossiness", Float) = 16
    }
    SubShader
    {
        //Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" "PassFlags" = "OnlyDirectional" }
        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _AmbientColor;
            float4 _SpecularColor;
            float _Glossiness;

            struct appdata
            {
                float2 uv: TEXCOORD0;
                float4 vertex: POSITION; // vertex of geometry
                float3 normal : NORMAL; // normal surface
            };

            struct v2f
            {
                float2 uv: TEXCOORD0;
                float4 position: SV_POSITION;
                float3 normal: NORMAL;
                float3 viewDirection : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.position = UnityObjectToClipPos(v.vertex);
                o.viewDirection = WorldSpaceViewDir(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 0. Prepare
                float3 normal = normalize(i.normal);
                float3 viewDirection = normalize(i.viewDirection);

                // 1. Diffuse light
                float nDotL = dot(_WorldSpaceLightPos0, normal);
                float lightIntensity = smoothstep(0, 0.01, nDotL); // light gradient with smoothstep
                float4 diffuseLight = lightIntensity * _LightColor0; // include the light's color in our calculation

                // 2. Specular light 
                float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDirection); // calculate vectore between light vector and view vector
                float nDotH = dot(normal, halfVector);
                float specularIntensity = smoothstep(0.005, 0.01, pow(nDotH * lightIntensity, _Glossiness * _Glossiness));
                float4 specularLight = specularIntensity * _SpecularColor;

                // 3. Blend
                return  (_AmbientColor + diffuseLight + specularLight) *
                        _Color *
                        tex2D(_MainTex, i.uv);
            }
            ENDCG
        }

        // Shadow casting support
        //      UsePass grabs a pass from a different shader and inserts it into our shader. 
        //      In this case, we are adding a pass that is used by Unity during the shadow casting step of the rendering process.
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER" 
    }
}
