using System;

namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Interactor" )]
[Icon( "keyboard_hide" )]

public sealed class Interactor : Component
{
	const float INTERACT_RADIUS = 2.0f;
	const float INTERACT_RANGE = 75.0f;
	const float EYE_HEIGHT = 64.0f;

	[Property] public CameraComponent PlayerCamera { get; set; }

	private float FacingPitch { get; set; }
	private readonly Vector3 eyePos = new( 0, 0, EYE_HEIGHT );

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
		PlayerCamera.WorldRotation = WorldTransform.RotationToWorld( new Angles( FacingPitch, 0, 0 ) );
		PlayerCamera.Transform.ClearInterpolation();

		PollInteraction();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		FacingInput();
	}

	private void FacingInput()
	{
		var f = Input.AnalogLook;
		FacingPitch = (FacingPitch + f.pitch).Clamp( -90, 90 );
		WorldRotation = WorldTransform.RotationToWorld( new Angles( 0, f.yaw, 0 ) );
	}

	private void PollInteraction()
	{
		if ( Input.Pressed( "use" ) )
		{
			if ( heldCarriable != null )
			{
				StopCarrying();
				return;
			}

			Portal.RunTrace(
				Scene.Trace.HitTriggers().IgnoreGameObjectHierarchy( GameObject ),
				PlayerCamera.WorldPosition,
				PlayerCamera.WorldPosition + (PlayerCamera.WorldTransform.Forward * INTERACT_RANGE * WorldScale.x),
				INTERACT_RADIUS * WorldScale.x
			).GetFirstGoComponent<IInteractable>()?.OnInteract( this );
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
