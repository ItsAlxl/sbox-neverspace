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
	private float FacingPitch { get; set; }
	private Angles EyeAngles { get => new( FacingPitch, 0, 0 ); }
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
		PlayerCamera.WorldScale = WorldScale;
		PlayerCamera.WorldPosition = WorldTransform.PointToWorld( eyePos );

		cameraReference = skipNextCamLerp ? WorldTransform : cameraReference.RotateAround( Vector3.Zero, Rotation.FromToRotation( cameraReference.Up, WorldTransform.Up ) * Time.Delta * VIEW_ROT_SPEED );
		skipNextCamLerp = false;
		PlayerCamera.WorldRotation = cameraReference.RotationToWorld( EyeAngles );

		PlayerCamera.Transform.ClearInterpolation();
		PollInteraction();
	}

	public void SkipLerps()
	{
		skipNextCamLerp = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		FacingInput();
	}

	private void FacingInput()
	{
		var f = Input.AnalogLook;
		FacingPitch = (FacingPitch + f.pitch).Clamp( -90.0f, 90.0f );
		WorldRotation = WorldRotation.RotateAroundAxis( Vector3.Up, f.yaw );
		cameraReference.Rotation = cameraReference.Rotation.RotateAroundAxis( Vector3.Up, f.yaw );
	}

	private SceneTraceResult RunInteractTrace( out Transform portaledOrigin, bool portalsOnly = false )
	{
		var trace = Scene.Trace.IgnoreGameObjectHierarchy( GameObject ).WithoutTags( "walkway" );
		return Portal.RunTrace(
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
			var tr = RunInteractTrace( out Transform portaledOrigin, true );
			if ( (tr.Hit && tr.GameObject == heldCarriable.GameObject) || portaledOrigin.AlmostEqual( heldCarriable.PortaledOrigin ) )
				carriedItemInReach = 0.0f;
			if ( carriedItemInReach > 0.2f )
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
		heldCarriable.Uncarry();
		heldCarriable = null;
	}
}
