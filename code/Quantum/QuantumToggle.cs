namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Toggle" )]
[Icon( "switch_camera" )]

public sealed class QuantumToggle : QuantumController
{
	[Property] GameObject ObservedChild { get; set; }
	[Property] GameObject UnobservedChild { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		if ( ObservedChild == null && UnobservedChild == null )
		{
			ObservedChild = GetControlledGos().ElementAt( 0 );
		}
	}

	protected override void OnObserve()
	{
		foreach ( var g in GetControlledGos() )
		{
			g.Enabled = g == ObservedChild;
		}
	}

	protected override void OnUnobserve()
	{
		foreach ( var g in GetControlledGos() )
		{
			g.Enabled = g == UnobservedChild;
		}
	}
}
