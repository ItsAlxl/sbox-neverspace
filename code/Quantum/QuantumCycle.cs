namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Cycle" )]
[Icon( "flip_camera_ios" )]

public sealed class QuantumCycle : QuantumController
{
	[Property] int ActiveStage { get; set; } = 0;
	[Property] bool LoopCycle { get; set; } = true;
	[Property] bool AddNullStage { get; set; } = false;

	protected override void OnStart()
	{
		ActiveStage--; // OnUnobserve gets called in OnStart
		base.OnStart();
	}

	protected override void OnObserve() { }

	protected override void OnUnobserve()
	{
		ActiveStage++;
		ApplyCurrentStage();
	}

	private void ApplyCurrentStage()
	{
		var controlledGos = GetControlledGos();
		if ( LoopCycle )
			ActiveStage %= AddNullStage ? (controlledGos.Count + 1) : controlledGos.Count;
		else
			ActiveStage = ActiveStage.Clamp( 0, AddNullStage ? controlledGos.Count : (controlledGos.Count - 1) );

		for ( int i = 0; i < controlledGos.Count; i++ )
			controlledGos[i].Enabled = i == ActiveStage;
	}
}
