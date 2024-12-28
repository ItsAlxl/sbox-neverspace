namespace Neverspace;

[Group( "Neverspace - Interaction" )]
[Title( "Carriable" )]
[Icon( "open_in_new" )]

public sealed class Carriable : Component, IInteractable
{
	const float FOLLOW_ROT_TIME = 1.0f / 5.0f;
	const float FOLLOW_POS_TIME = 1.0f / 7.5f;

	[RequireComponent] private Rigidbody Rigidbody { get; set; }

	public Transform PortaledOrigin { get; set; }
	private Transform PortaledCarrierTransform { get => PortaledOrigin.ToWorld( carrier.CarryTransform ); }

	private Transform TargetCarrierTransform { get; set; }
	private Transform TargetWorldTransform { get => PortaledCarrierTransform.ToWorld( TargetCarrierTransform ); }

	private Interactor carrier;

	protected override void OnAwake()
	{
		base.OnAwake();
		var traveler = GameObject.Components.Get<PortalTraveler>();
		if ( traveler != null )
			traveler.OnTeleport += OnTeleport;
	}

	public void OnInteract( Interactor interacter, Transform portaledOrigin )
	{
		carrier = interacter;
		ListenToCarrierTeleport( true );
		PortaledOrigin = portaledOrigin;
		TargetCarrierTransform = PortaledCarrierTransform.ToLocal( WorldTransform );
		interacter.StartCarrying( this );
	}

	private void OnTeleport( Portal portal )
	{
		if ( carrier != null )
		{
			PortaledOrigin = portal.GetEgressTransform( PortaledOrigin );
		}
	}

	private void OnCarrierTeleport( Portal portal )
	{
		PortaledOrigin = portal.GetEgressTransform( global::Transform.Zero ).ToLocal( PortaledOrigin );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( carrier != null )
		{
			Rigidbody.SmoothRotate( TargetWorldTransform.Rotation, FOLLOW_ROT_TIME, Time.Delta );
			Rigidbody.SmoothMove( TargetWorldTransform.Position, FOLLOW_POS_TIME, Time.Delta );
		}
	}

	public void Uncarry()
	{
		ListenToCarrierTeleport( false );
		carrier = null;
	}

	private void ListenToCarrierTeleport( bool listen )
	{
		var traveler = carrier.GetComponent<PortalTraveler>();
		if ( traveler != null )
		{
			if ( listen )
				traveler.OnTeleport += OnCarrierTeleport;
			else
				traveler.OnTeleport -= OnCarrierTeleport;
		}
	}
}
