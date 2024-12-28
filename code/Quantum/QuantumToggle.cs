namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Toggle" )]
[Icon( "switch_camera" )]

public sealed class QuantumToggle : QuantumController
{
	[Property] GameObject ObservedChild { get; set; }
	[Property] GameObject UnobservedChild { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		if ( ObservedChild == null && UnobservedChild == null )
		{
			ObservedChild = GetControlledGos()[0];
		}
	}

	protected override void OnObserved()
	{
		foreach ( var g in GetControlledGos() )
		{
			g.Enabled = g == ObservedChild;
		}
	}

	protected override void OnUnobserved()
	{
		foreach ( var g in GetControlledGos() )
		{
			g.Enabled = g == UnobservedChild;
		}
	}
}
