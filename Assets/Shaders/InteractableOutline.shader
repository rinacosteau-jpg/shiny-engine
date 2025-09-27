Shader "Custom/InteractableOutline" {
    Properties {
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.1)) = 0.02
        _OutlineEnabled ("Outline Enabled", Range(0, 1)) = 0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
        Cull Front
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float _OutlineThickness;
            float _OutlineEnabled;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 position : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                float3 normal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                float extrude = _OutlineThickness * _OutlineEnabled;
                clipPos.xy += normal.xy * extrude * clipPos.w;
                o.position = clipPos;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 color = _OutlineColor;
                color.a *= _OutlineEnabled;
                return color;
            }
            ENDCG
        }
    }
}
