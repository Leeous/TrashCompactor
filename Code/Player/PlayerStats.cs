using System;
using Sandbox;

public sealed class PlayerStats : Component
{
	[Property] public bool EnableLogging { get; set; } = false;
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; private set; }
	
	[Property] public SoundEvent hurtSound { get; set; }
	
	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	protected override void OnUpdate()
	{
		
	}

	public void TakeDamage( float damage )
	{
		// Ensure current health cannot go below 0
		CurrentHealth = Math.Max( 0f, CurrentHealth - damage );
		
		// Log if EnableLogging is true
		if (EnableLogging) Log.Info( $"Player took ${damage} damage." );
		
		// Play hurtSound
		Sound.Play( hurtSound, GetComponent<PlayerController>().WorldPosition );
		
		if ( CurrentHealth <= 0 )
		{
			Die();
		}
	}

	public void Die()
	{
		Log.Info( "Player is currently DEAD 💀" );
	}
}
