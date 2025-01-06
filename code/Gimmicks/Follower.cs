namespace Neverspace;

[Group( "Neverspace - Gimmicks" )]
[Title( "Follower" )]
[Icon( "follow_the_signs" )]

public sealed class Follower : Component
{
	[Property] Interactor Player { get; set; }
	[Property] float Buffer { get; set; } = -150.0f;

	protected override void OnAwake()
	{
		base.OnAwake();
		Player ??= Scene.GetAllComponents<Interactor>().ElementAt( 0 );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		var toPlrX = Player.WorldPosition.x - WorldPosition.x + Buffer;
		toPlrX = toPlrX < 0.0f ? 0.0f : toPlrX;
		WorldPosition = WorldPosition.WithX( WorldPosition.x + toPlrX );
		Transform.ClearInterpolation();
	}
}
