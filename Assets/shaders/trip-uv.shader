HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth();
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsVisMode( S_MODE_TOOLS_VIS );
}

COMMON
{
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"
	PixelInput MainVs( VertexInput i ) {
		return FinalizeVertex( ProcessVertex( i ) );
	}
}

PS
{
	#include "common/pixel.hlsl"

	float2 TriplanarUvEstimate( float3 vPosition, float3 vNormal )
	{
		float3 triblend = saturate(pow(abs(vNormal), 4));
		triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

		half3 axisSign = vNormal < 0 ? 1 : -1;

		return vPosition.zy * axisSign.x * triblend.x + vPosition.xz * axisSign.y * triblend.y - vPosition.xy * axisSign.z * triblend.z;
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		i.vTextureCoords.xy = TriplanarUvEstimate(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, i.vNormalWs) * 0.01;

		Material m = Material::From( i );
		m.Albedo *= i.vVertexColor.xyz;
		return ShadingModelStandard::Shade( i, m );
	}
}
