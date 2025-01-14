using System;

namespace Neverspace;

[Group( "Neverspace - Interaction" )]
[Title( "Keyhole" )]
[Icon( "lock" )]

public sealed class Keyhole : Component, Component.ITriggerListener
{
	[Property] Key PairedKey { get; set; }
	[Property] Vector3 KeyPos { get; set; } = Vector3.Up * 0.75f;

	public event Action OnKeyInserted;
	public bool HasKey { get; set; } = false;

	public void OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Components.FirstOrDefault( ( c ) => c == PairedKey ) is Key key )
		{
			HasKey = true;
			key.LockIn( WorldTransform.PointToWorld( KeyPos ) );
			OnKeyInserted?.Invoke();
		}
	}
}
