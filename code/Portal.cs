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
	private readonly Dictionary<PortalTraveler, int> travelerPassage = new( 1 );

	private readonly Texture dbgTex = Texture.Load( FileSystem.Mounted, "shaders/dbg_tex.png" );

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

	protected override void OnUpdate()
	{
		ViewScreen.Enabled = EgressPortal != null;
		if ( ViewScreen.Enabled )
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

	protected override void OnFixedUpdate()
	{
		if ( travelerPassage.Count > 0 )
		{
			foreach ( var kv in travelerPassage )
			{
				var traveler = kv.Key;
				var newSide = GetOffsetSide( traveler.WorldTransform );
				if ( newSide != kv.Value )
				{
					traveler.TeleportTo( GetEgressTransform( traveler.WorldTransform ) );
					travelerPassage[traveler] = 0;
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

	public void OnTriggerEnter( Collider other )
	{
		var t = other.GameObject.Components.Get<PortalTraveler>( FIND_MODE_TRAVELER );
		if ( t != null && !travelerPassage.ContainsKey( t ) )
		{
			travelerPassage.Add( t, GetOffsetSide( t.WorldTransform ) );
		}
	}

	public void OnTriggerExit( Collider other )
	{
		travelerPassage.Remove( other.GameObject.Components.Get<PortalTraveler>( FIND_MODE_TRAVELER ) );
	}
}
