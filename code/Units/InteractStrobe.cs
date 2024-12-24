using Sandbox;

namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Interact Strobe" )]
[Icon( "tips_and_updates" )]

public sealed class InteractStrobe : Component, IInteractable
{
	public void OnInteract( Player interacter )
	{
		foreach ( var m in GameObject.Components.GetAll<ModelRenderer>() )
		{
			m.Tint = Color.Random;
		}
	}
}
