namespace Neverspace;

[Group( "Neverspace - Interaction" )]
[Title( "Interact Strobe" )]
[Icon( "tips_and_updates" )]

public sealed class InteractStrobe : Component, IInteractable
{
	public void OnInteract( Interactor interacter, Transform _ )
	{
		foreach ( var m in GameObject.Components.GetAll<ModelRenderer>() )
		{
			m.Tint = Color.Random;
		}
	}
}
