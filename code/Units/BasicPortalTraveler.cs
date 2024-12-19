namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Basic Portal Traveler" )]
[Icon( "transfer_within_a_station" )]

public sealed class BasicPortalTraveler : Component, IPortalTraveler
{
	public Transform TravelerTransform
	{
		get => WorldTransform;
		set => value = WorldTransform;
	}

	public void TeleportTo( Transform destinationTransform )
	{
		WorldTransform = destinationTransform;
		Transform.ClearInterpolation();
	}
}
