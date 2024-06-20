Shader "Custom/Gloss"
{
  Properties
  {
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        fixed3 normal : NORMAL;
        fixed4 color : COLOR;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        fixed3 normal : TEXCOORD0;
        fixed3 worldPos : TEXCOORD1;
        
        fixed4 color : COLOR;
      };

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        fixed dist = 1 - (distance(_WorldSpaceCameraPos, o.worldPos) / 10);
        o.color = v.color * dist;

        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
        fixed rim = 1.0 - saturate(dot (normalize(viewDir), i.normal));
        
        i.color *= rim * rim * rim * rim * rim * rim * rim * rim * rim;
        i.color.a = 1;

        return i.color;
      }
      ENDCG
    }
  }
}
