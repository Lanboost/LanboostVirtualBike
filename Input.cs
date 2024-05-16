using Godot;
using Godot.NativeInterop;
using System;
using System.Timers;

public partial class Input : Control
{
    System.Timers.Timer aTimer;
    public override void _Ready()
    {
        base._Ready();
        aTimer = new System.Timers.Timer();
        aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        aTimer.Interval = 3000; // ~ 3 seconds
        aTimer.AutoReset = false;

    }


    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (@event is InputEventMouseButton mouseEvent)
        {
            if(mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed())
            {
                ToggleSettingsDisplay();
            }

        }
    }
    public override void _UnhandledInput(InputEvent @event)
    {

        if (@event is InputEventMouseButton mouseEvent)
        {
            GD.Print("mouse button event at ", mouseEvent.Position);
            ToggleSettingsDisplay();
            //this.GetViewport().SetInputAsHandled();

        }
        if (@event is InputEventScreenTouch touchEvent)
        {
            GD.Print("mouse button event at ", touchEvent.Position);
            ToggleSettingsDisplay();
        }
    }

    void ToggleSettingsDisplay()
    {
        var gd = this.GetNode<GameData>("/root/GameDataSingleton");
        if (gd.DisplaySettings)
        {
            gd.DisplaySettings = false;
            aTimer.Stop();
        }
        else
        {
            gd.DisplaySettings = true;
            aTimer.Start();
        }
    }

    void CloseDisplaySettings()
    {
        var gd = this.GetNode<GameData>("/root/GameDataSingleton");
        gd.DisplaySettings = false;
    }

    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        this.CallDeferred("CloseDisplaySettings");
    }


}
