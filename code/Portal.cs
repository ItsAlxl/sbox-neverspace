namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal" )]
[Icon( "join_left" )]

public sealed class Portal : Component, Component.ITriggerListener
{
	const FindMode FIND_MODE_TRAVELER = FindMode.Enabled | FindMode.InSelf | FindMode.InParent;

	[Property] Portal EgressPortal { get; set; }
	[Property] CameraComponent PlayerCamera { get; set; }

	ModelRenderer ViewScreen { get; set; }
	CameraComponent GhostCamera { get; set; }

	private Texture renderTarget;
	private readonly Dictionary<IPortalTraveler, int> travelerPassage = new( 1 );

	private bool passageHidden = false;

	protected override void OnAwake()
	{
		ViewScreen ??= GameObject.Components.GetInChildren<ModelRenderer>();
		GhostCamera ??= GameObject.Components.GetInChildren<CameraComponent>();
		PlayerCamera ??= Scene.Camera;
	}

	protected override void OnStart()
	{
		ViewScreen.MaterialOverride = Material.FromShader( "shaders/portal" ).CreateCopy();
		GhostCamera.Enabled = false;
	}

	protected override void OnPreRender()
	{
		ViewScreen.SceneObject.RenderingEnabled = !passageHidden && EgressPortal != null;
		if ( ViewScreen.SceneObject.RenderingEnabled )
		{
			GhostCamera.Enabled = ViewScreen.IsInCameraBounds( PlayerCamera );
			if ( GhostCamera.Enabled )
			{
				if ( renderTarget == null || !renderTarget.Size.AlmostEqual( Screen.Size, 0.5f ) )
				{
					renderTarget?.Dispose();
					renderTarget = Texture.CreateRenderTarget()
						.WithSize( Screen.Size )
						.Create();
					GhostCamera.RenderTarget = renderTarget;
					ViewScreen.SceneObject.Batchable = false;
					ViewScreen.SceneObject.Attributes.Set( "PortalViewTex", renderTarget );
				}
				GhostCamera.FieldOfView = PlayerCamera.FieldOfView;
				GhostCamera.WorldTransform = GetEgressTransform( PlayerCamera.WorldTransform );
			}
		}
	}

	protected override void OnUpdate()
	{
		if ( travelerPassage.Count > 0 )
		{
			foreach ( var kv in travelerPassage )
			{
				if ( kv.Value != -2 )
				{
					var traveler = kv.Key;
					var newSide = GetOffsetSide( traveler.TravelerTransform );
					if ( newSide != kv.Value )
					{
						EgressPortal.IgnoreTravelerPassage( traveler );
						traveler.TeleportTo( GetEgressTransform( traveler.TravelerTransform ) );
						travelerPassage[traveler] = 0;
					}
				}
			}

			var travelerCleanup = travelerPassage.Where( f => f.Value == 0 ).ToArray();
			foreach ( var kv in travelerCleanup )
			{
				travelerPassage.Remove( kv.Key );
			}
		}
	}

	public Transform GetEgressTransform( Transform sourceWorldTransform )
	{
		return EgressPortal.WorldTransform.ToWorld( WorldTransform.ToLocal( sourceWorldTransform ) );
	}

	public int GetOffsetSide( Transform targetWorldTransform )
	{
		return WorldTransform.Forward.Dot( WorldTransform.ToLocal( targetWorldTransform ).Position ) < 0.0f ? -1 : 1;
	}

	public void IgnoreTravelerPassage( IPortalTraveler t )
	{
		travelerPassage.Add( t, -2 );
		if ( t is Player )
		{
			//passageHidden = true;
		}
	}

	public void OnTriggerEnter( Collider other )
	{
		var t = other.GameObject.Components.Get<IPortalTraveler>( FIND_MODE_TRAVELER );
		if ( t != null && !travelerPassage.ContainsKey( t ) )
		{
			travelerPassage.Add( t, GetOffsetSide( t.TravelerTransform ) );
		}
	}

	public void OnTriggerExit( Collider other )
	{
		var t = other.GameObject.Components.Get<IPortalTraveler>( FIND_MODE_TRAVELER );
		if ( passageHidden && t is Player )
		{
			passageHidden = false;
		}
		travelerPassage.Remove( t );
	}
}
