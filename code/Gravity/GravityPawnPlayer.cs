namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Gravity Pawn - Player" )]
[Icon( "fitness_center" )]

public sealed class GravityPawnPlayer : GravityPawn
{
	const float FLOOR_THRESHOLD = 0.5f;

	[RequireComponent] private CapsuleCollider CapsuleCollider { get; set; }
	[RequireComponent] private Interactor Interactor { get; set; }

	public Vector3 TargetVelocity = Vector3.Zero;
	public Vector3 LocalVelocity = Vector3.Zero;
	public bool IsGrounded = false;
	public bool JumpNext = false;

	public float MovtSpeed = 150.0f;
	public float AirMovtClamp = 100.0f;

	public float AccelGround = 6.0f;
	public float AccelAir = 0.2f;

	public float MaxFallSpeed = -2500.0f;
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
		if ( gravLength > 0.01f )
		{
			var upNormal = -CurrentGravity / gravLength;
			if ( !(WorldTransform.Up - upNormal).IsNearlyZero( 0.001f ) )
			{
				if ( !IsGravAffected )
				{
					// don't get stuck in the grav object that we're detaching from
					WorldPosition += WorldTransform.Up * (CapsuleCollider.Radius + CapsuleCollider.End.z) * WorldScale.z * 1.5f;
				}
				WorldRotation = Rotation.LookAt( Vector3.VectorPlaneProject( WorldTransform.Forward, upNormal ), upNormal );
			}
		}

		if ( IsGrounded )
		{
			if ( JumpNext )
			{
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
				LocalVelocity = LocalVelocity.WithZ( LocalVelocity.z + (MaxFallSpeed - LocalVelocity.z) * Time.Delta * AccelAir );
			}
			else
			{
				LocalVelocity = LocalVelocity.WithZ( LocalVelocity.z - gravLength * Time.Delta );
			}
		}
		JumpNext = false;
		LocalVelocity += (TargetVelocity - LocalVelocity).WithZ( 0.0f );// * Time.Delta * (IsGrounded ? AccelGround : AccelAir); // causes jitters, eg when walking into a wall

		var lookTransform = WorldTransform.WithRotation( WorldTransform.RotationToWorld( Interactor.EyeAngles.WithPitch( 0.0f ) ) );
		var worldVelocity = lookTransform.NormalToWorld( LocalVelocity ) * LocalVelocity.Length * WorldScale.x * Time.Delta;
		Capsule WorldBodyCapsule = new( WorldTransform.PointToWorld( CapsuleCollider.Start ), WorldTransform.PointToWorld( CapsuleCollider.End ), WorldScale.x * (CapsuleCollider.Radius + 1.0f) );
		/*
		Gizmo.Draw.Color = Color.Blue;
		Gizmo.Draw.LineCapsule( WorldBodyCapsule );
		Gizmo.Draw.Color = Color.Orange;
		Gizmo.Draw.LineCapsule( new( WorldBodyCapsule.CenterA + worldVelocity, WorldBodyCapsule.CenterB + worldVelocity, WorldBodyCapsule.Radius ) );
		//*/
		var capsuleRef = WorldBodyCapsule.CenterA + WorldBodyCapsule.Radius * WorldTransform.Down;
		WorldBodyCapsule.CenterB -= capsuleRef;
		WorldBodyCapsule.CenterA -= capsuleRef;

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

		IsGrounded = false;
		if ( tr.Hit )
		{
			LocalVelocity = LocalVelocity.SubtractDirection( WorldTransform.NormalToLocal( tr.Normal ) );
			TestGroundedness( tr );
		}
		if ( !IsGrounded )
		{
			TestGroundedness(
				Scene.Trace
					.Ray( WorldPosition, WorldPosition + WorldTransform.Down * WorldScale.z )
					.Run()
			);
		}
	}

	private void TestGroundedness( SceneTraceResult tr )
	{
		IsGrounded = tr.Hit && tr.Normal.Dot( WorldTransform.Up ) > FLOOR_THRESHOLD;
	}

	public override bool IsValidGravTrigger( Collider c )
	{
		return c != CapsuleCollider && base.IsValidGravTrigger( c );
	}
}
