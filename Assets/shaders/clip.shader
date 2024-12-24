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

	float3 ClipOgn < Attribute("ClipOgn"); >;
	float3 ClipNormal < Attribute("ClipNormal"); >;
	bool ClipEnabled < Attribute("ClipEnabled"); Default(0); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );
		m.Albedo = 0.5 * i.vVertexColor;
		if (ClipEnabled)
		{
			float3 worldPosition = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
			clip(dot(ClipNormal, worldPosition - ClipOgn) + 0.2);
		}
		return ShadingModelStandard::Shade( i, m );
	}
}
