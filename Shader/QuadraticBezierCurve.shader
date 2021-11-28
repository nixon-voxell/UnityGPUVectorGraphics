Shader "Unlit/Quadratic Bezier"
{
  Properties
  {
    [HDR] _Color("Color", Color) = (0, 1, 1, 1)
    _StrokeThickness("Stroke Thickness", Float) = 10
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }
    LOD 100
    Cull Off

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float3 uv : TEXCOORD0;
      };

      struct v2f
      {
        float3 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }

      uniform half4 _Color;
      uniform float _StrokeThickness;

      half4 frag (v2f i) : SV_Target
      {
        float3 uv = i.uv;
        float alpha = (uv.x * uv.x - uv.y) * uv.z;

        clip(alpha);
        return _Color;
      }
      ENDCG
    }
  }
}
