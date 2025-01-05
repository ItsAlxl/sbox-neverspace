using System;

namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Reset" )]
[Icon( "restart_alt" )]

public sealed class QuantumReset : QuantumController
{
	[Property] GameObject ResetTarget { get; set; }
	private Transform resetTrans;

	protected override void OnStart()
	{
		resetTrans = ResetTarget.WorldTransform;
		base.OnStart();
	}

	protected override void OnObserve()
	{
		if ( ResetTarget != null )
		{
			var c = ResetTarget.Components.Get<Carriable>();
			if ( c != null )
			{
				c.ForceDrop();
			}

			ResetTarget.WorldTransform = resetTrans;

			var b = ResetTarget.Components.Get<Rigidbody>();
			if ( b != null )
			{
				b.Velocity = b.AngularVelocity = Vector3.Zero;
			}
		}
	}

	protected override void OnUnobserve() { }
}
