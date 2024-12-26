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

	private void PollInteraction()
	{
		if ( Input.Pressed( "use" ) )
		{
			if ( IsCarrying )
			{
				StopCarrying();
				return;
			}

			Portal.RunTrace(
				Scene.Trace.HitTriggers().IgnoreGameObjectHierarchy( GameObject ),
				PlayerCamera.WorldPosition,
				PlayerCamera.WorldPosition + (PlayerCamera.WorldTransform.Forward * INTERACT_RANGE * WorldScale.x),
				INTERACT_RADIUS * WorldScale.x,
				out Transform portaledOrigin
			).GetFirstGoComponent<IInteractable>()?.OnInteract( this, portaledOrigin );
		}
		else if ( IsCarrying )
		{
			Portal.RunTrace(
				Scene.Trace.HitTriggers().IgnoreGameObjectHierarchy( GameObject ),
				PlayerCamera.WorldPosition,
				PlayerCamera.WorldPosition + (PlayerCamera.WorldTransform.Forward * INTERACT_RANGE * WorldScale.x),
				INTERACT_RADIUS * WorldScale.x,
				out Transform portaledOrigin
			);
			if ( portaledOrigin != heldCarriable.PortaledOrigin )
				StopCarrying();
		}
	}

	public void StartCarrying( Carriable c )
	{
		heldCarriable = c;
	}

	public void StopCarrying()
	{
		heldCarriable.Uncarry();
		heldCarriable = null;
	}
}
