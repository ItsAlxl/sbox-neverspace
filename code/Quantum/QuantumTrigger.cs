using System;

namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Trigger" )]
[Icon( "linked_camera" )]

public sealed class QuantumTrigger : QuantumController
{
	public event Action OnObserved;
	public event Action OnUnobserved;

	protected override void OnObserve()
	{
		OnObserved?.Invoke();
	}

	protected override void OnUnobserve()
	{
		OnUnobserved?.Invoke();
	}
}
