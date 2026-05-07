using System;

namespace Sandbox;

public sealed class PhysicsDamageHandler : Component, Component.ICollisionListener
{
	/// <summary>
	/// Enables logging for this component.
	/// </summary>
	[Property] public bool EnableLogging { get; set; } = false;
	/// <summary>
	/// The impact speed needed to register damage to a player.
	/// 
	/// Warning: Values that are too low can cause players to take damage by simply jumping into a wall.
	/// </summary>
	[Property, Title("Impact damage speed"), Description("The minimum impact speed needed to damage a player.")] public float ImpactSpeedNeededToDamage { get; set; } = 500f;
	
	public void OnCollisionStart( Collision collision )
	{
		if (!collision.Other.Body.GameObject.Tags.Contains( "prop" )) return;
		
		float impact = Math.Abs(collision.Self.Body.Velocity.x + collision.Self.Body.Velocity.y);
				
		if ( impact > ImpactSpeedNeededToDamage  )
		{
			if ( EnableLogging ) Log.Info( $"Player took ${impact} damage." );
			GetComponent<PlayerStats>().TakeDamage( collision.Self.Body.Velocity.Length );
		}
		// GetComponentInParent<PlayerStats>().TakeDamage( other.Self.Body.Velocity.x );
	}

	protected override void OnUpdate()
	{
	}
}
