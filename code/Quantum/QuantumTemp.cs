namespace Neverspace;

[Group( "Neverspace - Quantum" )]
[Title( "Quantum Temp" )]
[Icon( "shutter_speed" )]

public sealed class QuantumTemp : QuantumController
{
	[Property] GameObject NormalChild { get; set; }
	[Property] GameObject TempChild { get; set; }

	private bool showingTemp = false;
	public bool ShowTemp
	{
		get => showingTemp;
		set
		{
			var showChild = value ? TempChild : NormalChild;
			foreach ( var g in GetControlledGos() )
			{
				g.Enabled = g == showChild;
			}
			showingTemp = value;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		if ( NormalChild == null && TempChild == null )
		{
			NormalChild = GetControlledGos().ElementAt( 0 );
		}
		ShowTemp = false;
	}

	protected override void OnObserve() { }

	protected override void OnUnobserve()
	{
		ShowTemp = false;
	}
}
