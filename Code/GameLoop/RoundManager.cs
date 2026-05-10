using Sandbox;
using Sandbox.Diagnostics;

public enum RoundState
{
    WaitingForPlayers,
    Active,
    RoundOver,
    MatchEnd
}

public sealed class RoundManager : GameObjectSystem<RoundManager>, ISceneStartup, IScenePhysicsEvents
{
    public RoundManager ( Scene scene ) : base( scene )
    {
        
    }

	void ISceneStartup.OnHostInitialize()
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
        SpawnPlayers();
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
    public void SpawnPlayers()
    {
        Assert.NotNull( PlayerPrefab );
        
        // PlayerPrefab.Clone( GetComponent<SpawnPoint>().WorldPosition );
    }
}