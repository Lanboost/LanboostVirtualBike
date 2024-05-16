using Godot;
using System;

public partial class SettingsPopup : Control
{

    public override void _Ready()
    {
        base._Ready();

        var gd = this.GetNode<GameData>("/root/GameDataSingleton");

        gd.DisplaySettingsOnChanged += Gd_DisplaySettingsOnChanged;
    }

    private void Gd_DisplaySettingsOnChanged(object sender)
    {
        var gd = this.GetNode<GameData>("/root/GameDataSingleton");
        this.Visible = gd.DisplaySettings;
    }
}
