namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Door" )]
[Icon( "camera_outdoor" )]

public class QuantumDoor : QuantumController
{
	const float MIN_DIFF_SIZE = 0.01f;
	[Property] GameObject WatchedGo { get; set; }
	[Property] Vector3 MinDiff { get; set; }

	private bool ThresholdCrossed
	{
		get
		{
			var diff = WatchedGo.WorldPosition - WorldPosition;
			return CompareAxis( diff.x, MinDiff.x ) && CompareAxis( diff.y, MinDiff.y ) && CompareAxis( diff.z, MinDiff.z );
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		WatchedGo ??= Scene.GetAllComponents<Interactor>().ElementAt( 0 )?.GameObject;
		MinDiff *= WorldScale;

		foreach ( var g in GetControlledGos() )
			g.Enabled = false;
	}

	private bool CompareAxis( float dist, float target )
	{
		return target <= -MIN_DIFF_SIZE ? (dist < target) : (target < MIN_DIFF_SIZE || dist > target);
	}

	protected override void OnObserve() { }

	protected override void OnUnobserve()
	{
		if ( ThresholdCrossed )
		{
			foreach ( var g in GetControlledGos() )
				g.Enabled = true;
			Enabled = false;
			Log.Info( "Blammo!" );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( ThresholdCrossed && !Observed )
		{
			OnUnobserve();
		}
	}
}
