using Godot;
using Mirage;
using System;

public enum NetworkState
{
    None,
    Connecting,
    Authenticating,
    Loading,
    Connected,
    Disconnected,
}

public partial class GameNetworkManager : Node
{
    public string DisconnectReason;
    protected NetworkState _NetworkState = NetworkState.None;
    #region property change event
    public NetworkState NetworkState
    {
        get => _NetworkState;
        set
        {
            if (_NetworkState == value)
            {
                return;
            }
            _NetworkState = value;
            NetworkStateOnChanged?.Invoke(this);
        }
    }
    public event PropertyChangeEvent NetworkStateOnChanged;
    #endregion


    [Export]
    public PackedScene playerScene;


    [Export]
    public NetworkManager networkManager;
    public override void _Ready()
    {
        networkManager.Client.Started.AddListener(ClientStarted);
        networkManager.Client.Disconnected.AddListener((ClientDisconnected) => {
            DisconnectReason = ClientDisconnected.ToString();
            NetworkState = NetworkState.Disconnected;
        });

        networkManager.Client.Connected.AddListener((player) => {
            NetworkState = NetworkState.Authenticating;
            // TODO store player...
        });
        networkManager.Client.Authenticated.AddListener((player) => {
            NetworkState = NetworkState.Connected;
        });


        networkManager.Server.Started.AddListener(() =>
        {

            var gameData = this.GetNode<GameData>("/root/GameDataSingleton");
            //gameData.GameServer = new GameServer(gameData);
        });
        networkManager.Server.Authenticated += Server_Authenticated;

    }


    int accountCounter = 0;
    private void Server_Authenticated(NetworkPlayer player)
    {
        GD.Print("Callleedd....");
        var clone = playerScene.Instantiate();
        if (clone is Node3D node3d)
        {
            node3d.Position = Vector3.One;

            GD.Print($"Spawning at {node3d.Position}");
        }

        GetTree().Root.AddChild(clone);

        var entity = ((EntityRoot)clone).entity as Player;
        ((EntityRoot)clone).Visible = false;

        //entity.accountId = accountCounter;


        accountCounter++;
        //entity.skillExp = new int[2];
        //entity.skillExp[0] = 1;
        //entity.skillExp[1] = 1;

        var gd = this.GetNode<GameData>("/root/GameDataSingleton");



        var identity = clone.GetNetworkIdentity();
        identity.PrefabHash = PrefabHashHelper.GetPrefabHash(playerScene);
        networkManager.ServerObjectManager.AddCharacter(player, identity);
        GD.Print($"Adding character for player {player.Connection}");

        //gd.GameServer.InstanceHandler.AssignPlayerToInstanceForZone(0, entity);
    }

    private void ClientStarted()
    {
        NetworkState = NetworkState.Connecting;
        //networkManager.Client.World.onSpawn += World_onSpawn;
    }

    /*
    private void World_onSpawn(NetworkIdentity obj)
    {
        var root = obj.Root;
        if (root is EntityRoot entRoot)
        {
            if (entRoot.entity.EntityType == EEntityType.Player)
            {
                if (obj.HasAuthority)
                {
                    // Change???
                    this.player = entRoot.entity as Player;
                    PlayerSpawn?.Invoke(this.player);
                }
            }
        }
    }*/
}
