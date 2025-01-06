namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Controller" )]
[Icon( "preview" )]

public abstract class QuantumController : Component
{
	const float PROXIMITY_THRESHOLD = 4096.0f;

	[Property] public BBox ObservableBounds { get; set; }
	[Property] Interactor Player { get; set; }
	[Property] bool IgnoreProximity { get; set; } = false;

	public bool Observed = false;
	protected bool ObservedState = false;
	public bool ConvertBoundsToWorld = true;

	protected abstract void OnObserve();
	protected abstract void OnUnobserve();

	protected virtual List<GameObject> GetControlledGos()
	{
		return GameObject.Children;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Player ??= Scene.GetAllComponents<Interactor>().ElementAt( 0 );
		if ( ObservableBounds.Size.IsNearlyZero( 0.001f ) )
		{
			ObservableBounds = GetControlledGos().AggregateGoChildBounds();
		}
		else if ( ConvertBoundsToWorld )
		{
			ObservableBounds = WorldTransform.BBoxToWorld( ObservableBounds );
		}
		OnUnobserve();
		Tags.Add( "quantum" );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		var nowObserved = ObservableBounds.IsInCameraBounds( Player.PlayerCamera );
		if ( IgnoreProximity || ObservableBounds.DistanceSquaredToPoint( Player.WorldPosition ) > PROXIMITY_THRESHOLD * Player.WorldScale.x * Player.WorldScale.x )
		{
			if ( !nowObserved && ObservedState )
			{
				OnUnobserve();
				ObservedState = false;
			}
			if ( nowObserved && !Observed )
			{
				OnObserve();
				ObservedState = true;
			}
		}
		Observed = nowObserved;
		/*
		Gizmo.Draw.Color = Color.Orange;
		Gizmo.Draw.LineBBox( ObservableBounds );
		//*/
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		if ( !ObservableBounds.Size.IsNearlyZero( 0.001f ) )
		{
			Gizmo.Draw.Color = Color.Orange;
			Gizmo.Draw.LineBBox( ObservableBounds );
		}
	}
}
