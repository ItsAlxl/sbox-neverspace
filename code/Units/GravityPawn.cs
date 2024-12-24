using Sandbox;

namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Gravity Pawn" )]
[Icon( "play_for_work" )]

public sealed class GravityPawn : Component
{
	[Property] Vector3 BaseGravity { get; set; } = new( 0.0f, 0.0f, -800.0f );

	public Vector3 CurrentGravity { get => BaseGravity; }
}
