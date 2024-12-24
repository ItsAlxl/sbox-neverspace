using Microsoft.VisualBasic;

namespace Neverspace;

[Group( "Neverspace - Gimmicks" )]
[Title( "Portal" )]
[Icon( "join_left" )]

public sealed class Portal : Component, Component.ITriggerListener
{
	const float MIN_WORLD_SCALE = 0.5f;

	const FindMode FIND_MODE_TRAVELER = FindMode.Enabled | FindMode.InSelf | FindMode.InParent;

	[Property] Portal EgressPortal { get; set; }
	[Property] CameraComponent PlayerCamera { get; set; }
	[Property] float PassageXScale { get; set; } = 0.2f;
	[Property] float PassageOffset { get; set; } = -5.0f;

	ModelRenderer ViewScreen { get; set; }
	CameraComponent GhostCamera { get; set; }

	public Plane WorldPlane { get => new( WorldTransform.Position, WorldTransform.Forward ); }

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

				Plane p = EgressPortal.WorldPlane;
				p.Distance -= 1.0f * GetOffsetSide( GhostCamera.WorldPosition ); // TODO: this is wrong
				// s&box's Plane::GetDistance function is bad
				GhostCamera.CustomProjectionMatrix = p.SnapToPlane( GhostCamera.WorldPosition ).DistanceSquared( GhostCamera.WorldPosition ) < 50.0f ? null : GhostCamera.CalculateObliqueMatrix( p );
			}
		}
	}

	public void OnPassageCheck()
	{
		if ( travelerPassage.Count > 0 )
		{
			var needsCleanup = false;
			foreach ( var kv in travelerPassage )
			{
				var traveler = kv.Key;
				var newSide = GetOffsetSide( traveler.TravelerTransform.Position );
				if ( newSide != kv.Value )
				{
					traveler.SwapPassage();
					EgressPortal.AcceptTravelerPassage( traveler, newSide );
					OnTravelerExited( kv.Key );
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
				}
			}
		}
	}

	public Transform GetPortalTransform( Portal to, Transform sourceWorldTransform )
	{
		var t = to.WorldTransform.ToWorld( WorldTransform.ToLocal( sourceWorldTransform ) );
		return t.Scale.x <= MIN_WORLD_SCALE ? t.WithScale( MIN_WORLD_SCALE ) : t;
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
		return WorldTransform.Forward.Dot( worldPosition - WorldPosition ) < 0.0f ? -1 : 1;
	}

	public SceneTraceResult ContinueEgressTrace( Vector3 worldStart, Vector3 worldEnd, float radius, GameObject ignore )
	{
		return
			( // sorry lol
				GetOffsetSide( worldStart ) == GetOffsetSide( worldEnd ) ?
					Scene.Trace.Ray( worldStart, worldEnd )
						.Size( radius )
						.IgnoreGameObjectHierarchy( GameObject ) :
					Scene.Trace.Ray( GetEgressPosition( worldStart ), GetEgressPosition( worldEnd ) )
						.Size( radius * EgressPortal.WorldScale.x / WorldScale.x )
						.IgnoreGameObjectHierarchy( EgressPortal.GameObject )
			)
			.HitTriggers()
			.IgnoreGameObjectHierarchy( ignore )
			.Run();
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
		if ( t != null && !t.IsInPassage && !travelerPassage.ContainsKey( t ) )
		{
			AcceptTravelerPassage( t, GetOffsetSide( t.TravelerTransform.Position ) );
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

public class PortalGoSystem : GameObjectSystem
{
	public PortalGoSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.StartFixedUpdate, 1, CheckPassage, "CheckPassage" );
		Listen( Stage.StartFixedUpdate, 2, DrivePlayerCamera, "DrivePlayerCamera" );
		Listen( Stage.StartFixedUpdate, 3, TravelerMovement, "TravelerMovement" );
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
