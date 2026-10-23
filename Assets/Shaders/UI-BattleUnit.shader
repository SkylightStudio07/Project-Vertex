// 전투 유닛(주인공 · 동료 · 적) 공용 UI 셰이더. 출처가 다른 그림들의 화풍 차이를 줄이려고 같은 처리를 입힌다.
//  - 안쪽 외곽선: 화면 픽셀 기준 두께라 그림 해상도와 상관없이 모두 같은 굵기로 보인다
//  - 림라이트: 한 방향(기본 왼쪽 위)에서 오는 빛을 가장자리에만 얹는다
//  - 색 보정: 채도 · 밝기 · 색 곱하기로 배경 팔레트 쪽으로 당긴다
//  - 위 조명: 그림 위쪽은 밝게, 아래는 어둡게 (2.5D 무대 조명). _VRange는 유닛마다 BattleUnitLook이 넣는다
// 값은 머티리얼마다 (Resources/BattleUnit/BattleUnit_Ally · BattleUnit_Enemy). 붙이는 곳: BattleUnitLook
Shader "UI/BattleUnit"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.086, 0.094, 0.106, 0.85)
        _OutlinePx ("Outline Width (screen px)", Range(0, 6)) = 1.6

        [Header(Rim Light)]
        _RimColor ("Rim Color (a = strength)", Color) = (1, 0.97, 0.9, 0.55)
        _RimPx ("Rim Width (screen px)", Range(0, 12)) = 4
        _RimDir ("Light From (screen, x right / y up)", Vector) = (-0.7, 0.7, 0, 0)

        [Header(Top Light)]
        _TopLight ("Top Light (a = strength)", Color) = (0.8, 0.9, 1, 0)
        _TopFalloff ("Top Light Falloff", Range(0.5, 6)) = 2.2
        _BottomShade ("Bottom Shade", Range(0, 0.8)) = 0
        _VRange ("Visible V Range (min, max)", Vector) = (0, 1, 0, 0)

        [Header(Grade)]
        _Saturation ("Saturation", Range(0, 1.5)) = 1
        _Brightness ("Brightness", Range(0.5, 1.5)) = 1
        _Grade ("Grade Multiply", Color) = (1,1,1,1)
        _KeepGlow ("Keep Bright Saturated (flash, sparks)", Range(0, 1)) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            float4 _MainTex_ST;
            fixed4 _OutlineColor;
            float _OutlinePx;
            fixed4 _RimColor;
            float _RimPx;
            float4 _RimDir;
            fixed4 _TopLight;
            float _TopFalloff;
            float _BottomShade;
            float4 _VRange;
            float _Saturation;
            float _Brightness;
            fixed4 _Grade;
            float _KeepGlow;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float2 px = fwidth(uv); // 화면 1픽셀이 UV로 얼마인지 (축마다)
                fixed4 tex = tex2D(_MainTex, uv);
                half a = tex.a;

                // 안쪽 외곽선: 주변 8방향 중 가장 투명한 값과의 차
                float2 o = px * _OutlinePx;
                half nmin = 1;
                nmin = min(nmin, tex2D(_MainTex, uv + float2( o.x, 0)).a);
                nmin = min(nmin, tex2D(_MainTex, uv + float2(-o.x, 0)).a);
                nmin = min(nmin, tex2D(_MainTex, uv + float2(0,  o.y)).a);
                nmin = min(nmin, tex2D(_MainTex, uv + float2(0, -o.y)).a);
                nmin = min(nmin, tex2D(_MainTex, uv + o * 0.7071).a);
                nmin = min(nmin, tex2D(_MainTex, uv - o * 0.7071).a);
                nmin = min(nmin, tex2D(_MainTex, uv + float2(o.x, -o.y) * 0.7071).a);
                nmin = min(nmin, tex2D(_MainTex, uv + float2(-o.x, o.y) * 0.7071).a);
                half outline = saturate(a - nmin);

                // 림라이트: 빛 쪽으로 조금 옮긴 곳이 투명하면 그 가장자리가 빛을 받는다
                float2 dir = normalize(_RimDir.xy + 1e-5);
                half toward = tex2D(_MainTex, uv + dir * px * _RimPx).a;
                half rim = saturate(a - toward);

                // 색 보정
                half3 c = tex.rgb;
                half luma = dot(c, half3(0.299, 0.587, 0.114));
                // 밝고 채도 높은 픽셀(총구 섬광 · 불꽃 · 발광 눈)은 보정에서 빼서 색을 살린다
                half maxc = max(c.r, max(c.g, c.b));
                half chroma = maxc - min(c.r, min(c.g, c.b));
                half keep = _KeepGlow * smoothstep(0.7, 0.92, maxc) * smoothstep(0.22, 0.45, chroma);
                half3 original = c;
                c = lerp(luma.xxx, c, _Saturation) * _Grade.rgb * _Brightness;
                c = lerp(c, original, keep);
                // 위에서 내려오는 조명: 그림의 보이는 아래 끝(0) → 위 끝(1)
                float v = saturate((uv.y - _VRange.x) / max(_VRange.y - _VRange.x, 1e-4));
                c *= lerp(1 - _BottomShade, 1, v);
                c += _TopLight.rgb * (_TopLight.a * pow(v, _TopFalloff));
                c = lerp(c, _RimColor.rgb, rim * _RimColor.a);
                c = lerp(c, _OutlineColor.rgb, outline * _OutlineColor.a);

                fixed4 color = fixed4(c, a) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
