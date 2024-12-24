namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Portal Traveler - Rigidbody" )]
[Icon( "all_out" )]

public sealed class PortalTravelerRigidbody : PortalTraveler
{
	[RequireComponent] private Rigidbody Rigidbody { get; set; }

	public override void TeleportTo( Transform destinationTransform )
	{
		Rigidbody.Velocity = GetTransformedVector3( Rigidbody.Velocity, destinationTransform );
		Rigidbody.AngularVelocity = GetTransformedVector3( Rigidbody.AngularVelocity, destinationTransform );
		base.TeleportTo( destinationTransform );
	}
}
