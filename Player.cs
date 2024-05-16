using Godot;
using Mirage;
using System;
using System.Collections.Generic;


public class PositionData
{
    public float distance;
    public float speed;
    public double serverTime;

    public PositionData(float distance, float speed, double serverTime)
    {
        this.distance = distance;
        this.speed = speed;
        this.serverTime = serverTime;
    }
}


public partial class Player : NetworkBehaviour
{
    [Export]
    public Camera3D Camera;

    [Export]
    AnimationPlayer animationPlayer;

    List<PositionData> positionDatas = new List<PositionData>();



    public double PositionTime;


    protected float _Distance;
    #region property change event
    public float Distance
    {
        get => _Distance;
        set
        {
            if (_Distance == value)
            {
                return;
            }
            _Distance = value;
            DistanceOnChanged?.Invoke(this);
        }
    }
    public event PropertyChangeEvent DistanceOnChanged;
    #endregion

    int lastTick = 0;

    public float MyDistance = 0;
    public float MyLastDistance = 0;

    public override void _Ready()
    {
        base._Ready();
        var gd = this.GetNode<GameData>("/root/GameDataSingleton");
        var pathfollow = new PathFollow3D();
        gd.path3d.AddChild(pathfollow);
        //this.Reparent(pathfollow);

        this.DistanceOnChanged += (sender) =>
        {
            pathfollow.Progress = Distance;
            this.GetParent<Node3D>().Transform = pathfollow.Transform;
        };


        gd.movement.CurrentWorkOnChanged += Movement_CurrentWorkOnChanged;
    }


    private void Movement_CurrentWorkOnChanged(object sender)
    {
        //if cadance exist, use that for animation speed instead, else use watts
        var gd = this.GetNode<GameData>("/root/GameDataSingleton");
        var watts = gd.movement.CurrentWork;

        var speed = Math.Min(1, Mathf.Lerp(0.3, 1, watts / 70));
        if (watts < 5)
        {
            speed = 0;
        }
        if(watts > 250)
        {
            speed = Math.Min(1.5f, Mathf.Lerp(1, 1.5f, (watts-250)/75));
        }


        animationPlayer.SpeedScale = (float)speed;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if ((this.IsServer() && Identity.Owner == null) || this.HasAuthority())
        {

            var currTick = (int) (this.Identity.World.Time.ServerTime * 1000 / 20);
            if(currTick != lastTick)
            {
                lastTick = currTick;
                CheckChanged();
            }
        }

        var gd = this.GetNode<GameData>("/root/GameDataSingleton");
        if(gd.gameClient.Player == this) {
            //Camera.Visible = true;
            //GD.Print($"Enabling camera for {this} -> {gd.gameClient.Player} {this.IsServer()}");
        }
        else
        {
            if(Camera != null)
            {
                this.RemoveChild(Camera);
                Camera.QueueFree();
                Camera = null;
            }
            //Camera.Visible = false;
        }


        // Update position
        if ((this.IsServer() && Identity.Owner == null) || this.HasAuthority())
        {
            this.Distance = MyDistance;
        }
        else
        {
            var serverTime = this.Identity.World.Time.ServerTime-0.2;
            //GD.Print($"serverTime: {serverTime} {positionDatas.Count}");
            List<PositionData> toRemove = new List<PositionData>();
            for(int i=1; i< positionDatas.Count; i++)
            {
                if (positionDatas[i].serverTime < serverTime)
                {
                    toRemove.Add(positionDatas[i - 1]);
                }
            }
            foreach(var  remove in toRemove)
            {
                positionDatas.Remove(remove);
            }

            if(positionDatas.Count >=2)
            {
                
                var t = (serverTime - positionDatas[0].serverTime)/(positionDatas[1].serverTime - positionDatas[0].serverTime);

                var distance = Mathf.Lerp(positionDatas[0].distance, positionDatas[1].distance, t);
                this.Distance = (float) distance;
                //GD.Print($"positionDatas: is over 2 {this.Distance} {t}");
            }
        }
    }

    private void CheckChanged()
    {
        //if(MyLastDistance != MyDistance)
        //{
            //MyLastDistance = MyDistance;
            ServerSendUpdate(MyDistance, 0);
        //}
    }

    [ServerRpc(channel = Channel.Unreliable)]
    private void ServerSendUpdate(float distance, float speed)
    {
        //GD.Print($"Server Got update, sending {distance} {this.Identity.World.Time.ServerTime}");
        ClientSendUpdate(distance, speed, this.Identity.World.Time.ServerTime);
        
    }

    [ClientRpc(target = RpcTarget.Observers, channel = Channel.Unreliable)]
    private void ClientSendUpdate(float distance, float speed, double serverTime)
    {
        //if (!this.IsServer())
        {
        //    GD.Print($"Client Got update, sending {distance}, {serverTime}");
        }
        positionDatas.Add(new PositionData(distance, speed, serverTime));
    }

}
