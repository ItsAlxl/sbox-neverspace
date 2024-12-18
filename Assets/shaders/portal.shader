
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
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
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
	PixelInput MainVs( VertexInput i )
	{
		return FinalizeVertex( ProcessVertex( i ) );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	CreateInputTexture2D( ViewTex, Srgb, 8, "", "", "Material,10/10", Default3( 0.0, 0.0, 0.0 ) );
	Texture2D g_tViewTex < Channel( RGB, Box( ViewTex ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >; 
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		return g_tViewTex.Sample( g_sAniso, CalculateViewportUv( i.vPositionSs.xy ) );
	}
}

