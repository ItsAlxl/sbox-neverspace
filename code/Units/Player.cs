using System;

namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Player" )]
[Icon( "person_outline" )]

public sealed class Player : Component
{
	const float SPEED_RUN = 200.0f;
	const float SPEED_AIR_MAX = 50.0f;

	const float EYE_HEIGHT = 64.0f;

	const float FRICTION_GROUND = 6.0f;
	const float FRICTION_AIR = 0.2f;

	const float JUMP_POWER = 300.0f;

	[RequireComponent] private CharacterController CharacterController { get; set; }
	[RequireComponent] private PortalTraveler PortalTraveler { get; set; }
	[RequireComponent] private GravityPawn GravityPawn { get; set; }
	[Property] CameraComponent PlayerCamera { get; set; }

	private float FacingPitch { get; set; }
	private Vector3 WishVelocity { get; set; }

	private float CurrentFriction { get => CharacterController.IsOnGround ? FRICTION_GROUND : FRICTION_AIR; }

	private RealTimeSince lastGrounded;
	private RealTimeSince lastJump;
	private readonly Vector3 eyePos = new Vector3( 0, 0, EYE_HEIGHT );

	protected override void OnAwake()
	{
		PlayerCamera ??= Scene.Camera;

		PortalTraveler.TeleportHook = TeleportTo;
		PortalTraveler.MovtHook = Movement;
		PortalTraveler.IsCameraViewer = true;
	}

	public void OnCameraUpdate()
	{
		PlayerCamera.WorldScale = WorldScale;
		PlayerCamera.WorldPosition = WorldTransform.PointToWorld( eyePos );
		PlayerCamera.WorldRotation = WorldTransform.RotationToWorld( new Angles( FacingPitch, 0, 0 ) );
		PlayerCamera.Transform.ClearInterpolation();
	}

	protected override void OnFixedUpdate()
	{
		//Movement();
	}

	protected override void OnUpdate()
	{
		FacingInput();
	}

	public void TeleportTo( Transform destinationTransform )
	{
		var velAmt = CharacterController.Velocity.Length;
		var velDir = WorldTransform.NormalToLocal( CharacterController.Velocity );

		PortalTraveler.BaseTeleport( destinationTransform );

		CharacterController.Velocity = WorldTransform.NormalToWorld( velDir ) * WorldScale * velAmt;
	}

	private void FacingInput()
	{
		var f = Input.AnalogLook;
		FacingPitch = (FacingPitch + f.pitch).Clamp( -90, 90 );
		WorldRotation = WorldTransform.RotationToWorld( new Angles( 0, f.yaw, 0 ) );
	}

	private void Movement()
	{
		if ( CharacterController is null ) return;

		var cc = CharacterController;

		Vector3 halfGravity = GravityPawn.GetCurrentGravity() * Time.Delta * 0.5f;

		WishVelocity = Input.AnalogMove;

		if ( lastGrounded < 0.2f && lastJump > 0.3f && Input.Pressed( "jump" ) )
		{
			lastJump = 0;
			cc.Punch( Vector3.Up * JUMP_POWER );
		}

		if ( !WishVelocity.IsNearlyZero() )
		{
			WishVelocity = WorldRotation * WishVelocity;
			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.ClampLength( 1 );
			WishVelocity *= SPEED_RUN;

			if ( !cc.IsOnGround )
			{
				WishVelocity = WishVelocity.ClampLength( SPEED_AIR_MAX );
			}
			WishVelocity *= WorldScale;
		}

		cc.ApplyFriction( CurrentFriction * WorldScale.x );

		if ( cc.IsOnGround )
		{
			cc.Accelerate( WishVelocity );
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}
		else
		{
			cc.Velocity += halfGravity;
			cc.Accelerate( WishVelocity );
		}

		cc.Move();

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			lastGrounded = 0;
		}
		else
		{
			cc.Velocity += halfGravity;
		}
	}
}
