namespace Neverspace;

[Group( "Neverspace - Interaction" )]
[Title( "Interactor" )]
[Icon( "keyboard_hide" )]

public sealed class Interactor : Component
{
	const float INTERACT_RADIUS = 2.0f;
	const float INTERACT_RANGE = 75.0f;
	const float EYE_HEIGHT = 64.0f;
	const float VIEW_ROT_SPEED = 8.0f;

	[Property] public CameraComponent PlayerCamera { get; set; }

	private readonly Vector3 eyePos = new( 0, 0, EYE_HEIGHT );
	private Vector2 Facing { get; set; }
	public Angles EyeAngles { get => new( Facing.x, Facing.y, 0 ); }
	private Transform cameraReference;
	private bool skipNextCamLerp = true;

	public Transform CarryTransform { get => PlayerCamera.WorldTransform; }
	public bool IsCarrying { get => heldCarriable != null; }
	private Carriable heldCarriable;
	private TimeSince carriedItemInReach;

	protected override void OnAwake()
	{
		base.OnAwake();
		PlayerCamera ??= Scene.Camera;
	}

	public void OnCameraUpdate()
	{
		if ( PlayerCamera == null )
			return;

		PlayerCamera.WorldScale = WorldScale;
		PlayerCamera.WorldPosition = WorldTransform.PointToWorld( eyePos );

		if ( skipNextCamLerp )
		{
			skipNextCamLerp = false;
			cameraReference.Rotation = WorldRotation;
		}
		else
		{
			cameraReference.Rotation = Rotation.Lerp( cameraReference.Rotation, WorldRotation, Time.Delta * VIEW_ROT_SPEED );
		}
		PlayerCamera.WorldRotation = cameraReference.RotationToWorld( EyeAngles );

		PlayerCamera.Transform.ClearInterpolation();
		PollInteraction();
	}

	public void TeleportThrough( Portal p )
	{
		cameraReference.Rotation = p.GetEgressTransform( cameraReference ).Rotation;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		FacingInput();
	}

	private void FacingInput()
	{
		var f = Input.AnalogLook;
		Facing = new( (Facing.x + f.pitch).Clamp( -90.0f, 90.0f ), Facing.y + f.yaw );
	}

	private SceneTraceResult RunInteractTrace( out Transform portaledOrigin )
	{
		var trace = Scene.Trace.IgnoreGameObjectHierarchy( GameObject ).WithoutTags( "walkway", "planetoid" );
		return Gateway.RunTrace(
			trace,
			PlayerCamera.WorldPosition,
			PlayerCamera.WorldPosition + (PlayerCamera.WorldTransform.Forward * INTERACT_RANGE * WorldScale.x),
			INTERACT_RADIUS * WorldScale.x,
			out portaledOrigin
		);
	}

	private void PollInteraction()
	{
		if ( Input.Pressed( "use" ) )
		{
			if ( IsCarrying )
			{
				StopCarrying();
				return;
			}

			RunInteractTrace( out Transform portaledOrigin ).GetGoComponent<IInteractable>()?.OnInteract( this, portaledOrigin );
		}
		else if ( IsCarrying )
		{
			var tr = RunInteractTrace( out Transform portaledOrigin );
			if ( (tr.Hit && tr.GameObject == heldCarriable.GameObject) || portaledOrigin.AlmostEqual( heldCarriable.PortaledOrigin ) )
				carriedItemInReach = 0.0f;
			if ( carriedItemInReach > 0.5f )
				StopCarrying();
		}
	}

	public void StartCarrying( Carriable c )
	{
		heldCarriable = c;
		carriedItemInReach = 0.0f;
	}

	public void StopCarrying()
	{
		heldCarriable.OnUncarry();
		heldCarriable = null;
	}
}
