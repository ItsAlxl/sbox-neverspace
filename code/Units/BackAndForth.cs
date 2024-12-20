using System;

namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Back and Forth" )]
[Icon( "swap_horiz" )]

public sealed class BackAndForth : Component
{
	[Property] public float speed = 50.0f;
	[Property] public float distance = 200.0f;
	[Property] public Vector3 localDirection = new( 1.0f, 0.0f, 0.0f );

	private float progress = 0.0f;
	private float dir = 1;

	protected override void OnUpdate()
	{
		float dProgress = speed * Time.Delta * dir;
		WorldPosition += WorldTransform.NormalToWorld( localDirection ) * dProgress;
		progress += dProgress;
		if ( (dir > 0 && progress >= distance) || (dir < 0 && progress <= 0.0f) )
		{
			dir *= -1;
		}
	}
}
