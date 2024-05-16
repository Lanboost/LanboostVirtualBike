using Godot;
using System;

public partial class Movement : Node3D
{
    [Export]
    Slider powerSlider;

    [Export]
    Label powerLabel;

    [Export]
    Label timeLabel;

    [Export]
    Label distanceLabel;

    [Export]
    Label speedLabel;

    [Export]
    Label workLabel;

    [Export]
    Button bluetoothButton;

    [Export]
    Button backButton;

    [Export]
    Control bluetoothControl;

    [Export]
    TestScript testScript;

    [Export]
    PathFollow3D follower;

    double distance = 0;
    double speed = 0;


    protected double _CurrentWork;
    #region property change event
    public double CurrentWork
    {
        get => _CurrentWork;
        set
        {
            if (_CurrentWork == value)
            {
                return;
            }
            _CurrentWork = value;
            CurrentWorkOnChanged?.Invoke(this);
        }
    }
    public event PropertyChangeEvent CurrentWorkOnChanged;
    #endregion



    double currentDelta = 0;

    double time;
    double joules;

    public override void _Ready()
    {
        base._Ready();
        powerSlider.ValueChanged += (value) =>
        {
            CurrentWork = value;
            powerLabel.Text = $"{value:0} Watt";
        };


        bluetoothButton.Pressed += () =>
        {
            bluetoothControl.Visible = true;
        };

        backButton.Pressed += () =>
        {
            bluetoothControl.Visible = false;
        };

        testScript.PowerChanged += (newPower) =>
        {
            powerSlider.Value = 0;
            CurrentWork = newPower;
            powerLabel.Text = $"{newPower:0} Watt";
        };
    }

    double AirDrag(double speed)
    {
        // https://www.gribble.org/cycling/power_v_speed.html
        var cd = 0.63;
        var a = 1.509;
        var rho = 1.22601;
        return 0.5 * cd *a* rho * speed * speed;
    }

    void UpdateSpeed(double time)
    {
        var mass = 75;

        //workLabel.Text = AirDrag(speed).ToString();
        var rollingResistance = 8;

        var minusSpeed = ((rollingResistance + AirDrag(speed)) * time / mass);

        if(minusSpeed > speed)
        {
            speed = 0;
        }
        else
        {
            speed -= minusSpeed;
        }

        var energyOfSpeed = 0.5 * mass * speed * speed;

        var resultingSpeed = Math.Sqrt((CurrentWork * time + energyOfSpeed) / mass * 2);
        speed = resultingSpeed;
        distance += speed * time;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        currentDelta += delta;
        var updateRate = delta;
        //if(currentDelta >= updateRate)
        {
            currentDelta -= updateRate;
            UpdateSpeed(updateRate);
            time += updateRate;
            var kmph = (speed * 3.6);
            speedLabel.Text = $"{kmph:0.0} km/h";

            if(distance < 1000)
            {
                distanceLabel.Text = $"{distance:0} m";
            }
            else
            {
                var km = distance / 1000;
                distanceLabel.Text = $"{km:0.0} km";
            }

            timeLabel.Text = TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");

            //follower.Progress = (float) distance;
            var gs = this.GetNode<GameData>("/root/GameDataSingleton");
            if(gs.gameClient.Player != null)
            {
                gs.gameClient.Player.MyDistance = (float)distance;
            }

            joules += CurrentWork * updateRate;

            // Human efficiency cancels convertion between joules -> cal
            workLabel.Text = $"{joules * 0.001:0.#} kcal";

        }
    }

}
