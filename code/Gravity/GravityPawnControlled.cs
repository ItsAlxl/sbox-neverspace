using System;

namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Gravity Pawn - Controlled" )]
[Icon( "fitness_center" )]

public sealed class GravityPawnControlled : GravityPawn
{
	const float FLOOR_THRESHOLD = -10.0f;

	[RequireComponent] private CapsuleCollider CapsuleCollider { get; set; }

	public Vector3 TargetVelocity = Vector3.Zero;
	public Vector3 LocalVelocity = Vector3.Zero;
	public bool IsGrounded = false;
	public bool JumpNext = false;

	public float MovtSpeed = 150.0f;
	public float AirMovtClamp = 50.0f;

	public float AccelGround = 6.0f;
	public float AccelAir = 0.2f;

	public float MaxFallSpeed = -800.0f;
	public float JumpPower = 250.0f;

	public void ParseMovement( Vector3 wishMovement )
	{
		if ( !wishMovement.IsNearlyZero( 0.01f ) )
		{
			wishMovement *= MovtSpeed;
			if ( !IsGrounded )
			{
				wishMovement = wishMovement.ClampLength( AirMovtClamp );
			}
		}
		TargetVelocity = wishMovement;
	}

	public void Move()
	{
		var gravLength = CurrentGravity.Length;
		var gravityNormal = CurrentGravity / gravLength;
		if ( !(WorldTransform.Down - gravityNormal).IsNearlyZero( 0.05f ) )
		{
			var cross = WorldTransform.Down.Cross( gravityNormal );
			WorldRotation = cross.IsNearlyZero( 0.05f ) ? (WorldRotation.RotateAroundAxis( Vector3.Right, 180.0f )) : WorldRotation.RotateAroundAxis( WorldTransform.NormalToLocal( cross ), MathX.RadianToDegree( (float)Math.Asin( cross.Length ) ) );
		}

		if ( IsGrounded )
		{
			if ( JumpNext )
			{
				JumpNext = false;
				LocalVelocity = LocalVelocity.WithZ( JumpPower );
				IsGrounded = false;
			}
			else
			{
				LocalVelocity = LocalVelocity.WithZ( 0.0f );
			}
		}
		else
		{
			if ( LocalVelocity.z < MaxFallSpeed )
			{
				LocalVelocity = LocalVelocity.WithZ( (MaxFallSpeed - LocalVelocity.z) * Time.Delta * AccelAir );
			}
			else
			{
				LocalVelocity = LocalVelocity.WithZ( LocalVelocity.z - gravLength * Time.Delta );
			}
		}
		LocalVelocity += (TargetVelocity - LocalVelocity).WithZ( 0.0f );// * Time.Delta * (IsGrounded ? AccelGround : AccelAir);

		Capsule WorldBodyCapsule = new( WorldTransform.PointToWorld( CapsuleCollider.Start ), WorldTransform.PointToWorld( CapsuleCollider.End ), WorldScale.x * (CapsuleCollider.Radius + 1.0f) );
		var capsuleRef = WorldBodyCapsule.CenterA + WorldBodyCapsule.Radius * WorldTransform.Down;
		WorldBodyCapsule.CenterB -= capsuleRef;
		WorldBodyCapsule.CenterA -= capsuleRef;

		var worldVelocity = WorldTransform.NormalToWorld( LocalVelocity ) * LocalVelocity.Length * WorldScale.x * Time.Delta;
		var worldTargetPosition = WorldPosition + worldVelocity;
		var tr = Scene.Trace
			.Capsule( WorldBodyCapsule, WorldPosition, worldTargetPosition )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();
		if ( tr.Hit && tr.StartedSolid && tr.Normal.DotSign( worldVelocity ) > 0 )
		{
			tr = Scene.Trace
				.Capsule( WorldBodyCapsule, WorldPosition, worldTargetPosition )
				.IgnoreGameObjectHierarchy( GameObject )
				.IgnoreGameObjectHierarchy( tr.GameObject )
				.Run();
		}
		WorldPosition = tr.EndPosition + tr.Normal * WorldScale.x;

		var wasGrounded = IsGrounded;
		if ( tr.Hit )
		{
			LocalVelocity = LocalVelocity.SubtractDirection( WorldTransform.NormalToLocal( tr.Normal ) );
			IsGrounded = tr.Normal.Dot( WorldTransform.Up ) > FLOOR_THRESHOLD;
		}
		else
		{
			var groundCheck = Scene.Trace
				.Ray( WorldPosition, WorldPosition + WorldTransform.Down * WorldScale.z * 1.0f )
				.Run();
			IsGrounded = groundCheck.Hit && groundCheck.Normal.Dot( WorldTransform.Up ) > FLOOR_THRESHOLD;
		}
	}

	public override bool IsValidGravTrigger( Collider c )
	{
		return c == CapsuleCollider ? false : base.IsValidGravTrigger( c );
	}
}
