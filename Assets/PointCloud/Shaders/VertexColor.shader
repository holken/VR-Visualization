// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/VertexColor" {

	Properties {
      _PointAlpha ("alpha of points", Float) = 1.0
   }
   
    SubShader {
	 
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull front 

    Pass {
        LOD 200
              
                 
        CGPROGRAM
        #pragma vertex vert alpha
        #pragma fragment frag alpha

		uniform float _PointAlpha;

        struct VertexInput {
            float4 v : POSITION;
            float4 color: COLOR;
        };
         
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float4 col : COLOR;
        };
         
        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;
            o.pos = UnityObjectToClipPos(v.v);
            o.col = v.color;
            //o.alpha = _PointAlpha;
            return o;
        }
         
        float4 frag(VertexOutput o) : COLOR {
            return o.col;
        }

        ENDCG
        } 
    }
 
}
