using System;

namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal" )]
[Icon( "join_left" )]

public sealed class Portal : Component, Component.ITriggerListener
{
	const FindMode FIND_MODE_TRAVELER = FindMode.Enabled | FindMode.InSelf | FindMode.InParent;

	[Property] Portal EgressPortal { get; set; }
	[Property] CameraComponent PlayerCamera { get; set; }
	[Property] float PassageXScale { get; set; } = 0.2f;
	[Property] float PassageOffset { get; set; } = -5.0f;

	ModelRenderer ViewScreen { get; set; }
	CameraComponent GhostCamera { get; set; }

	private Texture renderTarget;
	private readonly Dictionary<PortalTraveler, int> travelerPassage = new( 1 );

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

				GhostCamera.FieldOfView = PlayerCamera.FieldOfView;
				GhostCamera.WorldTransform = GetEgressTransform( PlayerCamera.WorldTransform );

				Plane p = EgressPortal.GetWorldPlane();
				p.Distance -= 1.0f * GetOffsetSide( GhostCamera.WorldTransform );
				// s&box's Plane::GetDistance function is bad
				GhostCamera.CustomProjectionMatrix = p.SnapToPlane( GhostCamera.WorldPosition ).DistanceSquared( GhostCamera.WorldPosition ) < 50.0f ? null : GhostCamera.CalculateObliqueMatrix( p );
			}
		}
	}

	protected override void OnUpdate()
	{
		if ( travelerPassage.Count > 0 )
		{
			Plane p = EgressPortal.GetWorldPlane();

			var needsCleanup = false;
			foreach ( var kv in travelerPassage )
			{
				var traveler = kv.Key;
				var newSide = GetOffsetSide( traveler.TravelerTransform );
				if ( newSide != kv.Value )
				{
					traveler.SwapPassage();
					EgressPortal.AcceptTravelerPassage( traveler, newSide );
					traveler.TeleportTo( GetEgressTransform( traveler.TravelerTransform ) );
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
					OnTravelerExited( kv.Key );
				}
			}
		}
	}

	public int GetVisualPrioritySide()
	{
		return GetOffsetSide( PlayerCamera.WorldTransform );
	}

	public Plane GetWorldPlane()
	{
		return new( WorldTransform.Position, WorldTransform.Forward );
	}

	public Transform GetPortalTransform( Portal to, Transform sourceWorldTransform )
	{
		return to.WorldTransform.ToWorld( WorldTransform.ToLocal( sourceWorldTransform ) );
	}

	public Transform GetEgressTransform( Transform sourceWorldTransform )
	{
		return GetPortalTransform( EgressPortal, sourceWorldTransform );
	}

	public int GetOffsetSide( Transform targetWorldTransform )
	{
		return WorldTransform.Forward.Dot( targetWorldTransform.Position - WorldPosition ) < 0.0f ? -1 : 1;
	}

	private void ApplyViewerConfig( bool passage, int side = 0 )
	{
		ViewScreen.LocalScale = ViewScreen.LocalScale.WithX( passage ? PassageXScale : 0.0f );
		ViewScreen.LocalPosition = ViewScreen.LocalPosition.WithX( side * PassageOffset );
	}

	private void AcceptTravelerPassage( PortalTraveler t, int side )
	{
		travelerPassage.Add( t, side );
		if ( t.IsCameraViewer )
		{
			ApplyViewerConfig( true, side );
		}
		t.BeginPassage( this, EgressPortal, side );
	}

	private void OnTravelerExited( PortalTraveler t )
	{
		if ( t.IsCameraViewer )
		{
			ApplyViewerConfig( false );
		}
		t.EndPassage( this, EgressPortal );
	}

	public void OnTriggerEnter( Collider other )
	{
		var t = other.GameObject.Components.Get<PortalTraveler>( FIND_MODE_TRAVELER );
		if ( t != null && !travelerPassage.ContainsKey( t ) )
		{
			AcceptTravelerPassage( t, GetOffsetSide( t.TravelerTransform ) );
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
}
