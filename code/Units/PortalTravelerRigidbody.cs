namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Portal Traveler - Rigidbody" )]
[Icon( "all_out" )]

public sealed class PortalTravelerRigidbody : PortalTraveler
{
	[RequireComponent] private Rigidbody Rigidbody { get; set; }

	public override void TeleportThrough( Portal portal )
	{
		var destinationTransform = portal.GetEgressTransform( TravelerTransform );
		Rigidbody.Velocity = GetTransformedVector3( Rigidbody.Velocity, destinationTransform );
		Rigidbody.AngularVelocity = GetTransformedVector3( Rigidbody.AngularVelocity, destinationTransform );
		base.TeleportThrough( portal );
	}
}
