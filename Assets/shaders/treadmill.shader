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

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float3 worldPosition = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		i.vTextureCoords.xy = float2(worldPosition.x, worldPosition.y + worldPosition.z) * 0.01;

		Material m = Material::From( i );
		m.Albedo *= i.vVertexColor;
		return ShadingModelStandard::Shade( i, m );
	}
}
