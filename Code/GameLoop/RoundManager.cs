using System.Threading.Channels;
using Sandbox;
using Sandbox.Diagnostics;

public enum RoundState
{
    WaitingForPlayers,
    Active,
    RoundOver,
    MatchEnd
}

public sealed class RoundManager : GameObjectSystem<RoundManager>, Component.INetworkListener, ISceneStartup, IScenePhysicsEvents
{
    public RoundManager ( Scene scene ) : base( scene )
    {
        
    }

	void ISceneStartup.OnHostInitialize()
	{
        if ( !Networking.IsActive )
        {
            Networking.CreateLobby( new Sandbox.Network.LobbyConfig()
                {
                    Privacy = Sandbox.Network.LobbyPrivacy.Public,
                    MaxPlayers = 16,
                    Name = "Trash Compactor",
                    DestroyWhenHostLeaves = true
                } 
            );    
        }
	}

	void Component.INetworkListener.OnActive( Connection channel )
	{
		channel.CanSpawnObjects = false;
        var playerData = CreatePlayerInfo( channel );

        SpawnPlayer( playerData );
	}

    private PlayerData CreatePlayerInfo ( Connection channel )
    {
        var existingPlayerInfo = PlayerData.For( channel );
        if ( existingPlayerInfo.IsValid() )
            return existingPlayerInfo;

        var go = new GameObject( true, $"PlayerInfo - {channel.DisplayName}");
        var data = go.AddComponent<PlayerData>();
        data.SteamId = (long)channel.SteamId;
        data.PlayerId = channel.Id;
        data.DisplayName = channel.DisplayName;

        go.NetworkSpawn( null );
        go.Network.SetOwnerTransfer( OwnerTransfer.Fixed );

        return data;
    }

    /// <summary>
    /// Spawns player based on their player data 
    /// </summary>
    /// <param name="playerData"></param>
    public void SpawnPlayer( PlayerData playerData )
    {
        Assert.NotNull( playerData, "PlayerData is null");
        Assert.True( Networking.IsHost, $"Client tried to SpawnPlayer: {playerData.DisplayName}");

        if ( Scene.GetAll<Player>().Any( x => x.Network.Owner?.Id == playerData.PlayerId ));

        var startLocation = FindSpawnLocation().WithScale( 1 );

        // Fire pre-respawn event - listeners can modify spawn startLocation
        var respawnEvent = new PlayerRespawnEvent { PlayerData = playerData, SpawnLocation = startLocation };
        Global.IPlayerEvents.Post ( x => x.OnPlayerRespawning( respawnEvent ));
        startLocation = respawnEvent.SpawnLocation;

        // Spawn this object and make the client the owner
        var playerGo = GameObject.Clone( "prefabs/player.prefab", new CloneConfig { Name = playerData.DisplayName, StartEnabled = false, Transform = startLocation } );

        var player = playerGo.Components.Get<Player>( true );
        player.PlayerData = playerData;
    }

    [Property] public GameObject PlayerPrefab { get; set;}
    public RoundState State { get; private set; }
    public int CurrentRound { get; private set; }
    public int MaxRounds { get; set; } = 8;
    public float RoundDuration { get; set; } = 120f;
    
    public float PostRoundDuration { get; set; } = 8f;

    /// <summary>
    /// The number of players that are currently *alive*, does not include spectators.
    /// </summary>
    public int ActivePlayers => Game.ActiveScene.GetAllComponents<Player>().Count();

    private TimeSince stateTimer;

    // protected override void OnUpdate()
    // {
    //     Log.Info($"There are {ActivePlayers}.");
    //     switch ( State )
    //     {
    //         case RoundState.WaitingForPlayers:
    //             if ( EnoughPlayers() ) 
    //                 StartRound();
    //             break;

    //         case RoundState.Active:
    //             if ( RoundShouldEnd() ) 
    //                 EndRound();
    //             break;

    //         case RoundState.RoundOver:
    //             if ( stateTimer > PostRoundDuration ) NextRound();
    //             break;
    //     }
    // }

    private void StartRound()
    {
        State = RoundState.Active;
        stateTimer = 0;
        AssignRandomRoles();
        ResetPlayersForRound();
        // SpawnPlayers();
    }

    private void EndRound()
    {
        State = RoundState.MatchEnd;
        stateTimer = 0;
        AnnounceWinners();
        CleanupRound();
    }

    /// <summary>
    /// Starts the next round
    /// </summary>
    private void NextRound()
    {
        StartRound();
    }

    private bool EnoughPlayers()
    {
        return true; // placer
    }

    private bool RoundShouldEnd()
    {
        if ( stateTimer > RoundDuration )
            return true;
        
        // One team fully elimated
        if ( AllTrashmenDead() || AllPlayersDead() )
            return true;

        return false;
    }

    Transform FindSpawnLocation()
    {
        var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();

        if (spawnPoints.Length == 0)
        {
            return Transform.Zero;
        }

        return Random.Shared.FromArray( spawnPoints ). Transform.World;
    }

    /// <summary>
    /// Based on the total player count, automatically assign players to roles.
    /// </summary>
    private void AssignRandomRoles()
    {
        
    }


    /// <summary>
    /// Resets all players to get ready for the next round. Sets max HP, respawns, etc.
    /// </summary>
    private void ResetPlayersForRound()
    {
        
    }

    /// <summary>
    /// Returns bool based on if all players are dead. Helps determine win condition.
    /// </summary>
    /// <returns>True if all players are dead.</returns>
    public bool AllPlayersDead()
    {
        return false;
    }

    /// <summary>
    /// Returns bool based on if all trashmen are dead. Helps determine win condition.
    /// </summary>
    /// <returns>True if all trashmen are dead.</returns>
    public bool AllTrashmenDead()
    {
        return false;
    }

    /// <summary>
    /// Regardless of state, announce winning team and setup game to begin next round.
    /// </summary>
    public void AnnounceWinners()
    {
        
    }

    /// <summary>
    /// Cleans up all props currently on the map.
    /// </summary>
    public void CleanupRound()
    {
        
    }

    /// <summary>
    /// Takes all current players in the lobby, assigns roles, and spawns them in-game.
    /// </summary>
    public void SpawnPlayers( PlayerData playerData )
    {
        Log.Info("Spawning players..");
        Assert.NotNull( PlayerPrefab );
        Assert.True( Networking.IsHost, $"Client tried to SpawnPlayer: { playerData.DisplayName }");

        // if (Scene.GetAll<Player>().Any( x => x.OnPlayerRespawning( respawn event )));
    }

    	/// <summary>
	/// Called on the host when a played is killed
	/// </summary>
	public void OnDeath( Player player, DamageInfo dmg )
	{
		Assert.True( Networking.IsHost );

		Assert.True( player.IsValid(), "Player invalid" );
		Assert.True( player.PlayerData.IsValid(), $"{player.GameObject.Name}'s PlayerData invalid" );

		// var source = dmg.Attacker?.GetComponentInParent<IKillSource>( true );
		// if ( source == null ) return;

		// var isSuicide = source is Player p && p == player;

		// if ( !isSuicide )
		// 	source.OnKill( player.GameObject );

		// // Fire kill event on the killer if they're a player
		// if ( !isSuicide && source is Player killer )
		// {
		// 	var killEvent = new PlayerKillEvent { Player = killer, Victim = player.GameObject, DamageInfo = dmg };
		// 	Local.IPlayerEvents.PostToGameObject( killer.GameObject, x => x.OnKill( killEvent ) );
		// 	Global.IPlayerEvents.Post( x => x.OnPlayerKill( killEvent ) );
		// }

		player.PlayerData.Deaths++;

	// 	var weapon = dmg.Weapon;
	// 	var w = weapon.IsValid() ? weapon.GetComponentInChildren<IKillIcon>() : null;
	// 	var damageTags = dmg.Tags.ToString() + ( isSuicide ? " suicide" : "" );
	// 	var attackerTags = isSuicide ? "" : source.Tags;
	// 	var attackerName = isSuicide ? null : source.DisplayName;
	// 	var attackerSteamId = isSuicide ? 0L : source.SteamId;
	// 	Scene.RunEvent<Feed>( x => x.NotifyKill( player.DisplayName, attackerName, attackerSteamId, damageTags, attackerTags, "", w?.DisplayIcon ) );

	// 	if ( string.IsNullOrEmpty( attackerName ) )
	// 	{
	// 		SendMessage( $"{player.DisplayName} died (tags: {dmg.Tags})" );
	// 	}
	// 	else if ( weapon.IsValid() )
	// 	{
	// 		SendMessage( $"{attackerName} killed {(isSuicide ? "self" : player.DisplayName)} with {weapon.Name} (tags: {dmg.Tags})" );
	// 	}
	// 	else
	// 	{
	// 		SendMessage( $"{attackerName} killed {(isSuicide ? "self" : player.DisplayName)} (tags: {dmg.Tags})" );
	// 	}
	}
}