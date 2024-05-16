using Godot;
using Mirage;
using System;

public delegate void PropertyChangeEvent(object sender);

public enum GameClientState
{
    None,
    InitialStartup,
    Menu,
    Connecting,
    Game,
    Disconnected
}

public partial class GameClient : Node
{
    protected GameClientState _GameClientState = GameClientState.None;
    #region property change event
    public GameClientState GameClientState
    {
        get => _GameClientState;
        set
        {
            if (_GameClientState == value)
            {
                return;
            }
            _GameClientState = value;
            GameClientStateOnChanged?.Invoke(this);
        }
    }
    public event PropertyChangeEvent GameClientStateOnChanged;
    #endregion

    [Export]
    public GameNetworkManager gameNetworkManager;

    public bool WorldLoading = true;
    public int worldLoadPercentage = 0;

    //public Player Player;

    protected Player _Player;
    #region property change event
    public Player Player
    {
        get => _Player;
        set
        {
            GD.Print($"Chaning player to: {value}");
            if (_Player == value)
            {
                return;
            }
            _Player = value;
            PlayerOnChanged?.Invoke(this);
        }
    }
    public event PropertyChangeEvent PlayerOnChanged;
    #endregion



    //public Camera3D Camera;

    public override void _Ready()
    {
        GameClientState = GameClientState.InitialStartup;
        gameNetworkManager.NetworkStateOnChanged += GameStateChangeBasedOnNetwork;

    }

    bool start = true;
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (start)
        {

            //gameNetworkManager.networkManager.StartHost();
            start = false;
        }
    }

    public void GameStateChangeBasedOnNetwork(object sender)
    {
        if (gameNetworkManager.NetworkState == NetworkState.Disconnected)
        {
            GameClientState = GameClientState.Disconnected;
        }
        else if (gameNetworkManager.NetworkState == NetworkState.Connecting)
        {
            GameClientState = GameClientState.Connecting;
        }
        else if (gameNetworkManager.NetworkState == NetworkState.Connected)
        {

            gameNetworkManager.networkManager.Client.Player.OnIdentityChanged += Player_OnIdentityChanged;

            //var entRoot = gameNetworkManager.networkManager.Client.Player.Identity.Root as EntityRoot;


            //this.GetNode<GameData>("/root/GameDataSingleton").player = entRoot.entity as Player;

            /*var root = obj.Root;
            if (root is EntityRoot entRoot)
            {
                entRoot.entity.gameData = this;
                entRoot.entity.RunPostReady();

                if (entRoot.entity.EntityType == EEntityType.Player)
                {
                    if (obj.HasAuthority)
                    {
                        // Change???
                        this.player = entRoot.entity as Player;
                        PlayerSpawn?.Invoke(this.player);
                    }
                }
            }*/

            GameClientState = GameClientState.Game;
        }
    }

    private void Player_OnIdentityChanged(NetworkIdentity obj)
    {
        var gameData = this.GetNode<GameData>("/root/GameDataSingleton");
        var entRoot = gameNetworkManager.networkManager.Client.Player.Identity.Root as EntityRoot;


        //this.GetNode<GameData>("/root/GameDataSingleton").player = entRoot.entity as Player;
        this.Player = entRoot.entity as Player;

        //PlayerSpawn?.Invoke(this.player);
    }

    public void Connect()
    {

    }

    public void Exit() { }
    public void Disconnect() { }
    public void Host() { }
}
