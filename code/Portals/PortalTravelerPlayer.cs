using System;

namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal Traveler - Player" )]
[Icon( "contacts" )]

public sealed class PortalTravelerPlayer : PortalTraveler
{
	const float FLOOR_CHECK_LENGTH = 1.0f;
	const float CC_RADIUS = 0.5f;
	const float CC_HEIGHT = 0.5f;
	const float CC_STEP_HEIGHT = 18.0f;

	const float SPEED_RUN = 200.0f;
	const float SPEED_AIR_MAX = 50.0f;
	const float FRICTION_GROUND = 6.0f;
	const float FRICTION_AIR = 0.2f;
	const float JUMP_POWER = 300.0f;

	[RequireComponent] private CharacterController CharacterController { get; set; }
	[RequireComponent] private GravityPawn GravityPawn { get; set; }
	[RequireComponent] private Interactor Interactor { get; set; }

	private RealTimeSince lastGrounded;
	private RealTimeSince lastJump;

	private Vector3 WishVelocity { get; set; }

	private bool IsGrounded { get; set; } = false;
	private float CurrentFriction { get => IsGrounded ? FRICTION_GROUND : FRICTION_AIR; }

	protected override void OnAwake()
	{
		base.OnAwake();
		IsCameraViewer = true;
		ApplyCharConfig();
	}

	private void ApplyCharConfig()
	{
		var cc = CharacterController;
		cc.Radius = CC_RADIUS * WorldScale.x;
		cc.Height = CC_HEIGHT * WorldScale.z;
		cc.StepHeight = CC_STEP_HEIGHT * WorldScale.z;
	}

	public override void TeleportThrough( Portal portal )
	{
		CharacterController.Velocity = GetTransformedVector3( CharacterController.Velocity, portal.GetEgressTransform( TravelerTransform ) );
		Interactor.SkipLerps();
		base.TeleportThrough( portal );
		ApplyCharConfig();
	}

	public override void OnMovement()
	{
		base.OnMovement();
		if ( CharacterController is null ) return;
		var gravity = GravityPawn.CurrentGravity;

		var gravityNormal = gravity.Normal;
		if ( !(WorldTransform.Down - gravityNormal).IsNearlyZero( 0.05f ) )
		{
			var cross = WorldTransform.Down.Cross( gravityNormal );
			WorldRotation = WorldRotation.RotateAroundAxis( WorldTransform.NormalToLocal( cross ), MathX.RadianToDegree( (float)Math.Asin( cross.Length ) ) );
		}

		var cc = CharacterController;

		Vector3 halfGravity = gravity * Time.Delta * 0.5f * WorldScale.z;

		WishVelocity = Input.AnalogMove;

		if ( lastGrounded < 0.2f && lastJump > 0.3f && Input.Pressed( "jump" ) )
		{
			lastJump = 0;
			cc.Punch( JUMP_POWER * WorldRotation.Up * WorldScale.z );
		}

		if ( !WishVelocity.IsNearlyZero( 0.01f ) )
		{
			WishVelocity = WorldTransform.NormalToWorld( WishVelocity ) * SPEED_RUN;
			if ( !IsGrounded )
			{
				WishVelocity = WishVelocity.ClampLength( SPEED_AIR_MAX );
			}
			WishVelocity *= WorldScale;
		}

		cc.ApplyFriction( CurrentFriction * WorldScale.x );

		if ( IsGrounded )
		{
			cc.Accelerate( WishVelocity );
		}
		else
		{
			cc.Velocity += halfGravity;
			cc.Accelerate( WishVelocity );
		}

		cc.Move();
		var floorTr = Scene.Trace
			.Ray( WorldPosition, WorldPosition + WorldTransform.Down * FLOOR_CHECK_LENGTH * WorldScale.z )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();
		IsGrounded = floorTr.Hit;

		if ( IsGrounded )
		{
			cc.Velocity = cc.Velocity.SubtractDirection( WorldTransform.Down );
			lastGrounded = 0;
		}
		else
		{
			cc.Velocity += halfGravity;
		}
	}
}
