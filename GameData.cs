using Godot;
using Mirage;
using System;

public partial class GameData : Node
{

    [Export]
    public GameClient gameClient;

    [Export]
    public Path3D path3d;

    [Export]
    public Movement movement;


    protected bool _DisplaySettings;
    #region property change event
    public bool DisplaySettings
    {
        get => _DisplaySettings;
        set
        {
            if (_DisplaySettings == value)
            {
                return;
            }
            _DisplaySettings = value;
            DisplaySettingsOnChanged?.Invoke(this);
        }
    }
    public event PropertyChangeEvent DisplaySettingsOnChanged;
    #endregion


}
