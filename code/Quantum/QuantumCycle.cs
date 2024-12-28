namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Cycle" )]
[Icon( "flip_camera_ios" )]

public sealed class QuantumCycle : QuantumController
{
	[Property] int ActiveStage { get; set; }
	[Property] bool LoopCycle { get; set; } = true;

	protected override void OnAwake()
	{
		base.OnAwake();
		ApplyCurrentStage();
	}

	protected override void OnObserved() { }

	protected override void OnUnobserved()
	{
		ActiveStage++;
		ApplyCurrentStage();
	}

	private void ApplyCurrentStage()
	{
		var controlledGos = GetControlledGos();
		if ( LoopCycle )
			ActiveStage %= controlledGos.Count;
		for ( int i = 0; i < controlledGos.Count; i++ )
			controlledGos[i].Enabled = i == ActiveStage;
	}
}
