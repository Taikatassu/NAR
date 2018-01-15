// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Grid_Geometry" {

	Properties{
		_GridThickness("Grid Thickness", Float) = 0.02
		_GridSpacing("Grid Spacing", Float) = 10.0
		_GridColor("Grid Color", Color) = (0.5, 0.5, 1.0, 1.0)
		_OutsideColor("Color Outside Grid", Color) = (0.0, 0.0, 0.0, 0.0)
		_BlendColor("Blending Color Grid", Color) = (0.0, 0.0, 0.0, 0.0)
		_FadeDistance("Distance To Fade Out The Grid", float) = 25.0
		_PosXInitialOffset("X Position Initial Offset", float) = 0.0
		_PosZInitialOffset("Z Position Initial Offset", float) = 0.0
		_PosXOffset("X Position Offset", float) = 0.0
		_PosZOffset("Z Position Offset", float) = 0.0
	}

		SubShader{
		Tags{
		"Queue" = "Geometry"
		}

		Pass{
		//ORIGINAL: ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha //SrcColor OneMinusSrcColor 

CGPROGRAM
#pragma vertex vert  
#pragma fragment frag 

	uniform float _GridThickness;
	uniform float _GridSpacing;
	uniform float4 _GridColor;
	uniform float4 _OutsideColor;
	uniform float4 _BlendColor;
	uniform float _FadeDistance;
	uniform float _PosXInitialOffset;
	uniform float _PosZInitialOffset;
	uniform float _PosXOffset;
	uniform float _PosZOffset;

	struct vertexInput {
		float4 vertex : POSITION;
	};
	struct vertexOutput {
		float4 pos : SV_POSITION;
		float4 worldPos : TEXCOORD0;
	};

	vertexOutput vert(vertexInput input) {
		vertexOutput output;

		output.pos = UnityObjectToClipPos(input.vertex);
		output.worldPos = mul(unity_ObjectToWorld, input.vertex);
		// transformation of input.vertex from object coordinates to world coordinates;
		return output;
	}

	float4 frag(vertexOutput input) : COLOR{

		float dist = distance(input.worldPos, float4(0.0, 0.0, 0.0, 1.0));

		if (frac((input.worldPos.x + _PosXInitialOffset + _PosXOffset) / _GridSpacing) < _GridThickness / 2
		|| frac((input.worldPos.x + _PosXInitialOffset + _PosXOffset) / _GridSpacing) > 1 - _GridThickness / 2
		|| frac((input.worldPos.z + _PosZInitialOffset + _PosZOffset) / _GridSpacing) < _GridThickness / 2
		|| frac((input.worldPos.z + _PosZInitialOffset + _PosZOffset) / _GridSpacing) > 1 - _GridThickness / 2) {

			float4 grid = _GridColor;
			grid.a = grid.a * (1 - (dist / _FadeDistance));
			grid = grid * _BlendColor;
			return grid;
		}
		else {
			float4 outside = _OutsideColor;
			outside.a = outside.a * (1 - (dist / _FadeDistance));
			outside = outside * _BlendColor;
			return outside;
		}
	}

		ENDCG
	}
	}
}