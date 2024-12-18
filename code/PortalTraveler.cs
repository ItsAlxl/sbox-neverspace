namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal Traveler" )]
[Icon( "transfer_within_a_station" )]

public sealed class PortalTraveler : Component
{
	public void TeleportTo(Transform destinationTransform)
	{
		WorldTransform = destinationTransform;
		Transform.ClearInterpolation();
	}
}
