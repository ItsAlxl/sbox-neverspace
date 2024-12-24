namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Portal Traveler - Player" )]
[Icon( "contacts" )]

public sealed class PortalTravelerPlayer : PortalTraveler
{
	const float CC_RADIUS = 16.0f;
	const float CC_HEIGHT = 72.0f;
	const float CC_STEP_HEIGHT = 18.0f;

	const float SPEED_RUN = 200.0f;
	const float SPEED_AIR_MAX = 50.0f;
	const float FRICTION_GROUND = 6.0f;
	const float FRICTION_AIR = 0.2f;
	const float JUMP_POWER = 300.0f;

	[RequireComponent] private CharacterController CharacterController { get; set; }
	[RequireComponent] private GravityPawn GravityPawn { get; set; }

	private RealTimeSince lastGrounded;
	private RealTimeSince lastJump;

	private Vector3 WishVelocity { get; set; }

	private float CurrentFriction { get => CharacterController.IsOnGround ? FRICTION_GROUND : FRICTION_AIR; }

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

	public override void TeleportTo( Transform destinationTransform )
	{
		CharacterController.Velocity = GetTransformedVector3( CharacterController.Velocity, destinationTransform );
		base.TeleportTo( destinationTransform );
		ApplyCharConfig();
	}

	public override void OnMovement()
	{
		if ( CharacterController is null ) return;

		var cc = CharacterController;

		Vector3 halfGravity = GravityPawn.CurrentGravity * Time.Delta * 0.5f * WorldScale.z;

		WishVelocity = Input.AnalogMove;

		if ( lastGrounded < 0.2f && lastJump > 0.3f && Input.Pressed( "jump" ) )
		{
			lastJump = 0;
			cc.Punch( Vector3.Up * JUMP_POWER * WorldScale.z );
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
