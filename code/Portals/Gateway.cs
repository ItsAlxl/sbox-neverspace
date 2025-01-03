namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal - Gateway" )]
[Icon( "join_left" )]

public sealed class Gateway : Portal, Component.ITriggerListener
{
	[Property] CameraComponent PlayerCamera { get; set; }
	[Property] float PassageXScale { get; set; } = 0.2f;
	[Property] float PassageOffset { get; set; } = -5.0f;

	public Gateway EgressGateway { get => EgressPortal as Gateway; }
	ModelRenderer ViewScreen { get; set; }
	CameraComponent GhostCamera { get; set; }

	public Plane WorldPlane { get => new( WorldTransform.Position, WorldTransform.Forward ); }

	private Texture renderTarget;
	private readonly Dictionary<PortalTraveler, int> travelerPassage = new( 1 );

	private int viewerSide = 0;
	private int currentViewerSide = 0;

	protected override void OnAwake()
	{
		base.OnAwake();
		ViewScreen ??= GameObject.Components.GetInChildren<ModelRenderer>();
		GhostCamera ??= GameObject.Components.GetInChildren<CameraComponent>();
		PlayerCamera ??= Scene.Camera;

		ViewScreen.MaterialOverride = Material.FromShader( "shaders/portal" );
		GhostCamera.Enabled = false;

		Tags.Add( "ptl-gateway" );
		ViewScreen.Tags.Add( "ptl-viewscreen" );
	}

	public void OnViewerConfig()
	{
		if ( viewerSide != currentViewerSide )
		{
			var scale = 1.0f / WorldScale.x;
			ViewScreen.LocalScale = ViewScreen.LocalScale.WithX( viewerSide == 0 ? 0.0f : (scale * PassageXScale) );
			ViewScreen.LocalPosition = ViewScreen.LocalPosition.WithX( scale * viewerSide * PassageOffset );
			ViewScreen.Transform.ClearInterpolation();
			currentViewerSide = viewerSide;
		}

		ViewScreen.SceneObject.RenderingEnabled = EgressPortal != null;
		if ( ViewScreen.SceneObject.RenderingEnabled )
		{
			// TODO: better check for optimization; walking backwards results in flickering
			GhostCamera.Enabled = true;//ViewScreen.IsInCameraBounds( PlayerCamera );
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
				GhostCamera.BackgroundColor = PlayerCamera.BackgroundColor;
				GhostCamera.ZNear = PlayerCamera.ZNear;
				GhostCamera.ZFar = PlayerCamera.ZFar;
				GhostCamera.FieldOfView = PlayerCamera.FieldOfView;
				GhostCamera.WorldTransform = GetEgressTransform( PlayerCamera.WorldTransform );

				Plane p = EgressGateway.WorldPlane;
				p.Distance += GetEgressCameraSide();
				// s&box's Plane::GetDistance function is bad
				GhostCamera.CustomProjectionMatrix = p.SnapToPlane( GhostCamera.WorldPosition ).DistanceSquared( GhostCamera.WorldPosition ) < 50.0f ? null : GhostCamera.CalculateObliqueMatrix( p );
			}
		}

	}

	public override void OnPassageCheck()
	{
		if ( travelerPassage.Count > 0 && EgressGateway != null )
		{
			var needsCleanup = false;
			foreach ( var kv in travelerPassage )
			{
				var traveler = kv.Key;
				var newSide = GetOffsetSide( traveler.TravelerTransform.Position );
				if ( newSide != kv.Value )
				{
					traveler.SwapPassage();
					EgressGateway.OnTravelerEntered( traveler, newSide );
					OnTravelerExited( kv.Key );
					traveler.TeleportThrough( this );
					travelerPassage[traveler] = 0;
					needsCleanup = true;
				}
			}

			if ( needsCleanup )
			{
				var travelerCleanup = travelerPassage.Where( f => f.Value == 0 ).ToList();
				foreach ( var kv in travelerCleanup )
				{
					travelerPassage.Remove( kv.Key );
				}
			}
		}
	}

	static public SceneTraceResult RunTrace( SceneTrace trace, Vector3 worldStart, Vector3 worldEnd, float radius, out Transform originTransform )
	{
		originTransform = global::Transform.Zero;
		var result = trace
			.Ray( worldStart, worldEnd )
			.Size( radius )
			.HitTriggers()
			.Run();

		var gateway = result.GetGoComponent<Gateway>();
		while ( result.Hit && result.GameObject != null && gateway != null )
		{
			result = gateway.ContinueEgressTrace( trace, result.HitPosition, worldEnd, ref radius, ref originTransform );
			gateway = result.GetGoComponent<Gateway>();
		}

		return result;
	}

	static public SceneTraceResult RunTrace( SceneTrace trace, Vector3 worldStart, Vector3 worldEnd, float radius )
	{
		return RunTrace( trace, worldStart, worldEnd, radius, out _ );
	}

	private SceneTraceResult ContinueEgressTrace( SceneTrace trace, Vector3 worldStart, Vector3 worldEnd, ref float radius, ref Transform trans )
	{
		var egress = GetOffsetSide( worldStart ) != GetOffsetSide( worldEnd );
		trans = egress ? GetEgressTransform( trans ) : trans;
		return
			( // sorry lol
				egress ?
					trace.Ray( GetEgressPosition( worldStart ), GetEgressPosition( worldEnd ) )
						.Size( radius *= EgressPortal.WorldScale.x / WorldScale.x )
						.IgnoreGameObjectHierarchy( EgressPortal.GameObject )
					: trace.IgnoreGameObjectHierarchy( GameObject )
			)
			.Run();
	}

	public void ApplyViewerConfig( int side )
	{
		viewerSide = side;
	}

	private void OnTravelerEntered( PortalTraveler t, int side )
	{
		travelerPassage.Add( t, side );
		if ( t.IsCameraViewer )
		{
			ApplyViewerConfig( side );
		}
		t.BeginPassage( this, EgressGateway, side );
	}

	private void OnTravelerExited( PortalTraveler t )
	{
		if ( t.IsCameraViewer )
		{
			ApplyViewerConfig( 0 );
		}
		t.EndPassage( this );
	}

	public void OnTriggerEnter( Collider other )
	{
		var t = other.GameObject.Components.Get<PortalTraveler>( FIND_MODE_TRAVELER );
		if ( t != null && !t.IsInPassage && !travelerPassage.ContainsKey( t ) && EgressGateway != null )
		{
			OnTravelerEntered( t, GetOffsetSide( t.TravelerTransform.Position ) );
		}
	}

	public void OnTriggerExit( Collider other )
	{
		var t = other.GameObject.Components.Get<PortalTraveler>( FIND_MODE_TRAVELER );
		if ( t != null && travelerPassage.Remove( t ) )
		{
			OnTravelerExited( t );
		}
	}

	public int GetCameraSide()
	{
		return GetOffsetSide( PlayerCamera.WorldPosition );
	}

	public int GetEgressCameraSide()
	{
		return EgressGateway.GetOffsetSide( GhostCamera.WorldPosition );
	}
}
