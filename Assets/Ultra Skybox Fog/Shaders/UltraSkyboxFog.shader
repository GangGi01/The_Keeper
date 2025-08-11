//Some code taken from Unity Skybox-Cubed.shader under the MIT license
Shader "Galactic Studios/Ultra Skybox Fog"
{
Properties {
    _Tint ("Tint Colour", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}

    _FogCol("Fog Colour", Color) = (.5, .5, .5, .5)
    _FogStart("Fog Begin", Range(0,1)) = 0
    _FogEnd("Fog End", Range(0,1)) = .4
    _FogIntens("Fog Density", Range(0,1)) = 1
    _MieIntens("Mie Intensity", Range(0,2)) = 0
    _MieTint("Mie Tint", Color) = (.5, .5, .5, .5)
    _MieSize("Mie Size", Range(0,1)) = .8
    _SunDir("Sun Direction", Range(0,360)) = 0
    [Toggle]_FogBottom("Apply Fog To The Bottom Of The Sky?", float) = 0
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        // ✅ 멀티뷰/싱글패스 스테레오 컴파일 경로 포함
        #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

        #include "UnityCG.cginc"

        samplerCUBE _Tex;
        half4 _Tex_HDR;
        half4 _Tint;
        half _Exposure;
        float _Rotation;
        half4 _FogCol;
        half _FogStart;
        half _FogEnd;
        half _FogIntens;
        half _MieIntens;
        half _SunDir;
        half4 _MieTint;
        fixed _FogBottom;
        half _MieSize;

        float3 RotateAroundYInDegrees (float3 v, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, v.xz), v.y).xzy;
        }

        struct appdata_t {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID      // ✅ 인스턴싱/스테레오 입력 매크로 활성화
        };

        struct v2f {
            float4 vertex   : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO          // ✅ 스테레오 varyings 포함 (여기가 없어서 에러 났던 거)
        };

        v2f vert (appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); // ✅ 스테레오 초기화

            // ✅ 보통은 지오메트리를 돌리지 않고, 샘플링 벡터를 회전시킵니다.
            float3 dir = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);

            o.vertex   = UnityObjectToClipPos(v.vertex);
            o.texcoord = dir;
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            // ✅ 프래그먼트에서 eye 인덱스 세팅(멀티뷰 경로에서 필요)
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

            half4 tex = texCUBE(_Tex, i.texcoord);
            half3 c = DecodeHDR(tex, _Tex_HDR);
            c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;

            //Calculate final fog colour
            half3 fogCol = _FogCol.rgb;
            float sina, cosa;
            sincos(_SunDir * UNITY_PI / 180.0, sina, cosa);
            fogCol *= _MieIntens * smoothstep(1 - _MieSize, 1, saturate(dot(float2(cosa, sina), i.texcoord.xz))) * _MieTint.rgb * unity_ColorSpaceDouble.rgb + 1;

            //Apply Fog
            c = lerp(c, fogCol, saturate((1 - smoothstep(_FogStart * 2, _FogEnd * 2, (_FogBottom == 0 ? abs(i.texcoord.y) : i.texcoord.y))) * _FogIntens));

            c *= _Exposure;
            return half4(c, 1);
        }
        ENDCG
    }
}
Fallback Off

}
