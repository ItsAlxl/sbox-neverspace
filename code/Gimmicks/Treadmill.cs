namespace Neverspace;

[Group( "Neverspace - Gimmicks" )]
[Title( "Treadmill" )]
[Icon( "attractions" )]

public sealed class Treadmill : Component
{
	[Property] Interactor Player { get; set; }
	[Property] float ArtBuffer { get; set; } = 250.0f;

	[Property] GameObject art { get; set; }
	[Property] GameObject colOriginal { get; set; }
	[Property] GameObject pin { get; set; }
	private GameObject colClone;

	private float stepOffset;
	private float stepLength;
	private float xPerScale;

	private GameObject movingCol;
	private GameObject pinnedCol;
	private float currentX;

	protected override void OnAwake()
	{
		base.OnAwake();
		Player ??= Scene.GetAllComponents<Interactor>().ElementAt( 0 );
		art ??= Components.Get<ModelRenderer>( FindMode.InChildren ).GameObject;
		colOriginal ??= Components.Get<BoxCollider>( FindMode.InChildren ).GameObject;

		colClone = new GameObject();
		foreach ( var c in colOriginal.Components.GetAll() )
			c.CreateDuplicate( colClone );
		colClone.WorldPosition = colOriginal.WorldPosition;
		colOriginal.AddSibling( colClone, true );

		stepOffset = art.LocalPosition.x;
		stepLength = stepOffset * 2.0f;
		xPerScale = stepOffset / art.LocalScale.x;

		movingCol = colClone;
		pinnedCol = colOriginal;
		currentX = pinnedCol.LocalPosition.x;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		var plrLocalX = WorldTransform.PointToLocal( Player.WorldPosition ).x;
		plrLocalX = plrLocalX < 0.0f ? 0.0f : plrLocalX;

		var targetX = 0.5f * ((plrLocalX < stepOffset ? stepOffset : plrLocalX) + ArtBuffer);
		if ( !art.LocalPosition.x.AlmostEqual( targetX ) )
		{
			art.LocalScale = art.LocalScale.WithX( targetX / xPerScale );
			art.LocalPosition = art.LocalPosition.WithX( targetX );
			art.Transform.ClearInterpolation();

			if ( pin != null )
			{
				pin.LocalPosition = art.LocalPosition * 2.0f;
				pin.Transform.ClearInterpolation();
			}
		}

		if ( plrLocalX < stepOffset )
			return;

		var next = plrLocalX >= currentX + stepOffset;
		if ( next || plrLocalX < currentX - stepOffset )
		{
			movingCol = movingCol == colOriginal ? colClone : colOriginal;
			pinnedCol = pinnedCol == colOriginal ? colClone : colOriginal;
			currentX = next ? (currentX + stepLength) : (currentX - stepLength);
		}

		movingCol.LocalPosition = pinnedCol.LocalPosition + stepLength * (plrLocalX >= currentX ? Vector3.Forward : Vector3.Backward);
		movingCol.Transform.ClearInterpolation();
	}
}
