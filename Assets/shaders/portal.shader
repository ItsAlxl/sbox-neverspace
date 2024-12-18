HEADER {
	Description = "";
}

FEATURES {
	#include "common/features.hlsl"
}

MODES {
	VrForward();
	ToolsVisMode( S_MODE_TOOLS_VIS );
}

COMMON {
	#include "common/shared.hlsl"
}

struct VertexInput {
	#include "common/vertexinput.hlsl"
};

struct PixelInput {
	#include "common/pixelinput.hlsl"
};

VS {
	#include "common/vertex.hlsl"
	PixelInput MainVs(VertexInput i) {
		return FinalizeVertex(ProcessVertex(i));
	}
}

PS {
	#include "common/pixel.hlsl"

	RenderState(CullMode, NONE);

	Texture2D ViewTexture < Attribute( "PortalViewTex" ); SrgbRead( true ); >;

	float4 MainPs(PixelInput i) : SV_Target0 {
		Material m = Material::Init();
		m.Albedo = ViewTexture.Sample(g_sAniso, CalculateViewportUv(i.vPositionSs.xy));
		return ShadingModelStandard::Shade( i, m );
		//return ViewTexture.Sample(g_sAniso, CalculateViewportUv(i.vPositionSs.xy));
	}
}
