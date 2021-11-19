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

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float2 uv : TEXCOORD0;
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
        float2 p = i.uv;
        // chain rule
        float2 px = ddx(p); float2 py = ddy(p);

        float fx = (2*p.x)*px.x - px.y;
        float fy = (2*p.x)*py.x - py.y;

        // signed distance
        float sd = (p.x*p.x - p.y)/sqrt(fx*fx + fy*fy);
        
        //linear alpha
        float alpha = 0.5 - sd;

        // inside
        if (sd + _StrokeThickness > 1 && alpha > 1) _Color.a = 1;
        // outside
        else if (sd + _StrokeThickness < 0 || alpha < 0) clip(-1);
        // near boundary
        else _Color.a = alpha;

        return saturate(sd + _StrokeThickness) * _Color;
      }
      ENDCG
    }
  }
}
