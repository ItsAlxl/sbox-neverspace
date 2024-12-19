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
	[Property] CameraComponent PlayerCamera { get; set; }

	private float FacingPitch { get; set; }
	private Vector3 WishVelocity { get; set; }

	private float CurrentFriction { get => CharacterController.IsOnGround ? FRICTION_GROUND : FRICTION_AIR; }

	private RealTimeSince lastGrounded;
	private RealTimeSince lastJump;

	protected override void OnAwake()
	{
		PlayerCamera ??= Scene.Camera;
		PlayerCamera.LocalPosition = new Vector3( 0, 0, EYE_HEIGHT );

		PortalTraveler.TeleportHook = TeleportTo;
		PortalTraveler.IsCameraViewer = true;
	}

	protected override void OnFixedUpdate()
	{
		MovementInput();
	}

	protected override void OnUpdate()
	{
		FacingInput();
	}

	public void TeleportTo( Action<Transform> tpFunc, Transform destinationTransform )
	{
		var velAmt = CharacterController.Velocity.Length;
		var velDir = WorldTransform.NormalToLocal( CharacterController.Velocity );

		tpFunc( destinationTransform );

		CharacterController.Velocity = WorldTransform.NormalToWorld( velDir ) * velAmt;
	}

	private void FacingInput()
	{
		var f = Input.AnalogLook;
		FacingPitch = (FacingPitch + f.pitch).Clamp( -90, 90 );
		WorldRotation = WorldTransform.RotationToWorld( new Angles( 0, f.yaw, 0 ) );
		PlayerCamera.LocalRotation = new Angles( FacingPitch, 0, 0 );
	}

	private void MovementInput()
	{
		if ( CharacterController is null ) return;

		var cc = CharacterController;

		Vector3 halfGravity = Scene.PhysicsWorld.Gravity * Time.Delta * 0.5f;

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
		}

		cc.ApplyFriction( CurrentFriction );

		if ( cc.IsOnGround )
		{
			cc.Accelerate( WishVelocity );
			cc.Velocity = CharacterController.Velocity.WithZ( 0 );
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
