namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Portal Traveler - Rigidbody" )]
[Icon( "all_out" )]

public sealed class PortalTravelerRigidbody : PortalTraveler
{
	[RequireComponent] private Rigidbody Rigidbody { get; set; }

	public override void TeleportTo( Transform destinationTransform )
	{
		var velAmt = Rigidbody.Velocity.Length;
		var velDir = WorldTransform.NormalToLocal( Rigidbody.Velocity );
		var prevScale = WorldScale;

		base.TeleportTo( destinationTransform );

		Rigidbody.Velocity = WorldTransform.NormalToWorld( velDir ) * (WorldScale / prevScale) * velAmt;
	}
}
