using System;

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

	protected abstract void OnObserved();
	protected abstract void OnUnobserved();

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
		else
		{
			ObservableBounds = new( WorldTransform.PointToWorld( ObservableBounds.Mins ), WorldTransform.PointToWorld( ObservableBounds.Maxs ) );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		var nowObserved = ObservableBounds.IsInCameraBounds( Player.PlayerCamera );
		if ( IgnoreProximity || ObservableBounds.DistanceSquaredToPoint( Player.WorldPosition ) / Player.WorldScale.x > PROXIMITY_THRESHOLD )
		{
			if ( !nowObserved && ObservedState )
			{
				OnUnobserved();
				ObservedState = false;
			}
			if ( nowObserved && !Observed )
			{
				OnObserved();
				ObservedState = true;
			}
		}
		Observed = nowObserved;
	}
}
