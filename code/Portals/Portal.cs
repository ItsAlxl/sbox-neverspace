namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal" )]
[Icon( "leak_add" )]

public abstract class Portal : Component
{
	public const FindMode FIND_MODE_TRAVELER = FindMode.Enabled | FindMode.InSelf | FindMode.InParent;

	[Property] public Portal EgressPortal { get; set; }
	[Property] public Walkway InstantWalkway { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Tags.Add( "portal" );
	}

	public Transform GetPortalTransform( Portal to, Transform sourceWorldTransform )
	{
		return to.WorldTransform.ToWorld( WorldTransform.ToLocal( sourceWorldTransform ) );
	}

	public Transform GetEgressTransform( Transform sourceWorldTransform )
	{
		return GetPortalTransform( EgressPortal, sourceWorldTransform );
	}

	public Vector3 GetEgressPosition( Vector3 sourceWorldPosition )
	{
		return EgressPortal.WorldTransform.PointToWorld( WorldTransform.PointToLocal( sourceWorldPosition ) );
	}

	public int GetOffsetSide( Vector3 worldPosition )
	{
		return WorldTransform.Forward.DotSign( worldPosition - WorldPosition );
	}

	public abstract void OnPassageCheck();
}

public class PortalGoSystem : GameObjectSystem
{
	public PortalGoSystem( Scene scene ) : base( scene )
	{
		if ( !scene.IsEditor )
		{
			Listen( Stage.StartFixedUpdate, 1, CheckPassage, "CheckPassage" );
			Listen( Stage.StartFixedUpdate, 2, DrivePlayerCamera, "DrivePlayerCamera" );
			Listen( Stage.StartFixedUpdate, 3, TravelerMovement, "TravelerMovement" );
		}
	}

	void CheckPassage()
	{
		foreach ( var p in Scene.GetAllComponents<Portal>() )
		{
			p.OnPassageCheck();
		}
	}

	void DrivePlayerCamera()
	{
		foreach ( var p in Scene.GetAllComponents<Interactor>() )
		{
			p.OnCameraUpdate();
		}
	}

	void TravelerMovement()
	{
		foreach ( var p in Scene.GetAllComponents<PortalTraveler>() )
		{
			p.OnMovement();
		}
	}
}
