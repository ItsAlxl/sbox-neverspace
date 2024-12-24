using Sandbox;

namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Carriable" )]
[Icon( "open_in_new" )]

public sealed class Carriable : Component, IInteractable
{
	const float FOLLOW_ROT_TIME = 1.0f / 5.0f;
	const float FOLLOW_POS_TIME = 1.0f / 7.5f;

	[RequireComponent] private Rigidbody Rigidbody { get; set; }

	private Transform CarrierTransform { get => carrier.PlayerCamera.WorldTransform; }
	public Transform TargetCarrierTransform { get; set; }
	public Transform TargetWorldTransform { get => CarrierTransform.ToWorld( TargetCarrierTransform ); }

	private Player carrier;

	public void OnInteract( Player interacter )
	{
		carrier = interacter;
		TargetCarrierTransform = CarrierTransform.ToLocal( WorldTransform );
		interacter.StartCarrying( this );
	}

	protected override void OnFixedUpdate()
	{
		if ( carrier != null )
		{
			Rigidbody.SmoothRotate( TargetWorldTransform.Rotation, FOLLOW_ROT_TIME, Time.Delta );
			Rigidbody.SmoothMove( TargetWorldTransform.Position, FOLLOW_POS_TIME, Time.Delta );
		}
	}

	public void Uncarry()
	{
		carrier = null;
	}
}
