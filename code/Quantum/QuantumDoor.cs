namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Door" )]
[Icon( "camera_outdoor" )]

public class QuantumDoor : QuantumController
{
	[Property] GameObject WatchedGo { get; set; }
	[Property] Vector3 MinDiff { get; set; }

	private bool ThresholdCrossed
	{
		get
		{
			if ( WatchedGo == null )
				return false;
			var diff = WatchedGo.WorldPosition - WorldPosition;
			return diff.x.MeetsThreshold( MinDiff.x ) && diff.y.MeetsThreshold( MinDiff.y ) && diff.z.MeetsThreshold( MinDiff.z );
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		WatchedGo ??= Scene.GetAllComponents<Interactor>().ElementAt( 0 )?.GameObject;
		MinDiff *= WorldScale;

		foreach ( var g in GetControlledGos() )
			g.Enabled = false;
	}

	protected override void OnObserve() { }

	protected override void OnUnobserve()
	{
		if ( ThresholdCrossed )
		{
			foreach ( var g in GetControlledGos() )
				g.Enabled = true;
			Enabled = false;
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
