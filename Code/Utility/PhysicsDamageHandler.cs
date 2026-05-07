using System;

namespace Sandbox;

public sealed class PhysicsDamageHandler : Component, Component.ICollisionListener
{
	[Property] public bool EnableLogging { get; set; } = false;
	
	public void OnCollisionStart( Collision collision )
	{
		if (!collision.Other.Body.GameObject.Tags.Contains( "prop" )) return;
		
		float impact = Math.Abs(collision.Self.Body.Velocity.x + collision.Self.Body.Velocity.y);
		
		Log.Info( $"Player took ${impact} damage." );
		
		if ( impact > 500  )
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
