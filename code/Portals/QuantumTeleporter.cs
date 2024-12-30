namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal - Quantum" )]
[Icon( "offline_share" )]

public sealed class QuantumTeleporter : Portal
{
	const float TRIGGER_THICKNESS = 16.0f;

	[Property] Interactor Player { get; set; }
	[Property] BBox AreaOfEffect { get; set; }
	[Property] BBox Trigger { get; set; }
	public QuantumTeleporter EgressQTP { get => EgressPortal as QuantumTeleporter; }

	private bool PlayerInPlace { get => AreaOfEffect.Contains( Player.PlayerCamera.WorldPosition ); }
	private bool TriggerContainsEntireView
	{
		get
		{
			var cam = Player.PlayerCamera;
			if ( (WorldPosition - cam.WorldPosition).Dot( cam.WorldTransform.Forward ) < 0.0f )
				return false;

			var f = cam.GetScreenFrustum();
			for ( var i = 0; i < 4; i++ )
			{
				var start = f.GetCorner( i ) ?? Vector3.Zero;
				var end = f.GetCorner( i + 4 ) ?? Vector3.One;
				if ( !Trigger.RayIntersects( new Ray( start, Vector3.Direction( start, end ) ) ) )
				{
					return false;
				}
			}
			return true;
		}
	}
	public bool IsFocused = false;
	private bool sticky = false;

	protected override void OnStart()
	{
		base.OnStart();
		Tags.Add( "ptl-quantum" );

		Player ??= Scene.GetAllComponents<Interactor>().ElementAt( 0 );

		if ( AreaOfEffect.Size.IsNearlyZero( 0.001f ) )
		{
			AreaOfEffect = new( new Vector3( -TRIGGER_THICKNESS, -TRIGGER_THICKNESS, 0.0f ), new Vector3( TRIGGER_THICKNESS, TRIGGER_THICKNESS, TRIGGER_THICKNESS ) );
		}

		if ( Trigger.Size.IsNearlyZero( 0.001f ) )
		{
			Trigger = WorldTransform.BBoxToWorld( new( AreaOfEffect.Mins, AreaOfEffect.Maxs.WithZ( AreaOfEffect.Mins.z - TRIGGER_THICKNESS ) ) );
		}
		AreaOfEffect = WorldTransform.BBoxToWorld( AreaOfEffect );
	}

	private void TeleportContents()
	{
		foreach ( var go in Scene.FindInPhysics( AreaOfEffect ) )
		{
			var traveler = go.GetComponent<PortalTraveler>();
			if ( traveler != null )
			{
				traveler.TeleportThrough( this );
				if ( traveler.IsCameraViewer )
				{
					EgressQTP.AcceptViewerPassage();
					IsFocused = false;
				}
			}
		}
	}

	private void AcceptViewerPassage()
	{
		sticky = true;
		IsFocused = true;
	}

	/*
	protected override void OnUpdate()
	{
		base.OnUpdate();
		Gizmo.Draw.Color = Color.Blue;
		Gizmo.Draw.LineBBox( AreaOfEffect );
		Gizmo.Draw.Color = Color.Orange;
		Gizmo.Draw.LineBBox( Trigger );
	}
	//*/

	public override void OnPassageCheck()
	{
		if ( sticky )
		{
			sticky = TriggerContainsEntireView;
		}
		else
		{
			var wasFocused = IsFocused;
			IsFocused = PlayerInPlace && TriggerContainsEntireView;
			if ( IsFocused != wasFocused && IsFocused )
				TeleportContents();
		}
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		if ( !AreaOfEffect.Size.IsNearlyZero( 0.001f ) )
		{
			Gizmo.Draw.Color = Color.Blue;
			Gizmo.Draw.LineBBox( AreaOfEffect );
		}
	}
}
