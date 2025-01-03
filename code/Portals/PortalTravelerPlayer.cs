namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal Traveler - Player" )]
[Icon( "contacts" )]

public sealed class PortalTravelerPlayer : PortalTraveler
{
	[RequireComponent] private GravityPawnPlayer GravityPawn { get; set; }
	[RequireComponent] private Interactor Interactor { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		IsCameraViewer = true;
	}

	public override void TeleportThrough( Portal portal )
	{
		Interactor.TeleportThrough( portal );
		base.TeleportThrough( portal );
	}

	public override void OnMovement()
	{
		base.OnMovement();
		if ( GravityPawn is null ) return;
		GravityPawn.ParseMovement( Input.AnalogMove );
		if ( Input.Pressed( "jump" ) )
		{
			GravityPawn.JumpNext = true;
		}
		GravityPawn.Move();
	}
}
