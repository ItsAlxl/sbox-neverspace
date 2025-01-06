namespace Neverspace;

[Group( "Neverspace - Interaction" )]
[Title( "Locked Door" )]
[Icon( "balcony" )]

public sealed class LockedDoor : Component
{
	[Property] List<Keyhole> Keyholes { get; set; } = new();
	[Property] GameObject LockedGo { get; set; }
	[Property] GameObject UnlockedGo { get; set; }

	private bool HasAllKeys { get => Keyholes.All( ( k ) => k.HasKey ); }
	private bool IsUnlocked { get; set; } = false;

	protected override void OnAwake()
	{
		base.OnAwake();
		foreach ( var k in Keyholes )
		{
			k.OnKeyInserted += OnKeyChanged;
		}
		Tags.Add( "unlockable-door" );
		UpdateState();
	}

	private void UpdateState()
	{
		if ( UnlockedGo != null )
			UnlockedGo.Enabled = IsUnlocked;
		if ( LockedGo != null )
			LockedGo.Enabled = !IsUnlocked;
	}

	private void OnKeyChanged()
	{
		var unlock = HasAllKeys;
		if ( IsUnlocked != unlock )
		{
			IsUnlocked = unlock;
			UpdateState();
		}
	}
}
