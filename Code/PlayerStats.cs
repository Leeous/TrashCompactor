using System;
using Sandbox;

public sealed class PlayerStats : Component
{
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; private set; }
	
	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	protected override void OnUpdate()
	{
		
	}

	public void TakeDamage( float damage )
	{
		CurrentHealth = Math.Max( 0f, CurrentHealth - damage );

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
