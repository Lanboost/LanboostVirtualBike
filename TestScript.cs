using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public partial class TestScript : Control
{
    [Export]
    public Button button1;

    [Export]
    public Button button2;

    [Export]
    public Button button3;

    [Export]
    public ItemList bluetoothDevices;

    [Export]
    public Label status;

    public string pluginName = "LanboostBluetoothGodotPlugin";

    delegate void OnDebugMessageDelegate();

    bool scanning = false;

    GodotObject androidPlugin;

    List<Dictionary> devices = new List<Dictionary>();

    public delegate void PowerEvent(int newPower);

    public event PowerEvent PowerChanged;



    public override void _Ready()
    {
        button1.Pressed += () =>
        {
            androidPlugin.Call("disconnect");
        };

        button3.Disabled = true;
        button2.Pressed += () =>
        {
            ChangeScan(true);
            androidPlugin.Call("scan");
        };

        bluetoothDevices.ItemSelected += (item) =>
        {
            button3.Disabled = false;
        };

        button3.Pressed += () =>
        {
            var indexes = bluetoothDevices.GetSelectedItems();
            if(indexes.Length == 1)
            {
                var address = devices[indexes[0]]["address"];
                androidPlugin.Call("stopScan");
                androidPlugin.Call("connect", address);
            }
        };


        base._Ready();
        if(Engine.HasSingleton(pluginName))
        {
            androidPlugin = Engine.GetSingleton(pluginName);
            if(androidPlugin != null)
            {
                androidPlugin.Connect("_on_debug_message", Callable.From<string>(OnDebugMessage));
                androidPlugin.Connect("_on_device_found", Callable.From<Dictionary>(OnDeviceFound));
                androidPlugin.Connect("_on_scan_stopped", Callable.From<string>(OnScanStopped));
                androidPlugin.Connect("_on_connection_status_change", Callable.From<string>(OnConnectionStatusChanged));
                androidPlugin.Connect("_on_characteristic_finding", Callable.From<string>(OnCharacteristicFinding));
                androidPlugin.Connect("_on_characteristic_found", Callable.From<Dictionary>(OnCharacteristicFound));
                androidPlugin.Connect("_on_characteristic_read", Callable.From<Dictionary>(onCharacteristicRead));
            }
        }
    }

    void OnConnectionStatusChanged(string message)
    {
        GD.Print("OnDebugMessage");
        GD.Print(message);
        if(message == "connected")
        {
            androidPlugin.Call("listServicesAndCharacteristics");
        }
    }

    void OnCharacteristicFinding(string message)
    {
        GD.Print("OnCharacteristicFinding");
        GD.Print(message);
    }

    void OnCharacteristicFound(Dictionary message)
    {
        GD.Print("OnCharacteristicFound");
        //GD.Print(message);
        GD.Print(message["characteristic_uuid"]);

        if ((string)message["characteristic_uuid"] == "00002a63-0000-1000-8000-00805f9b34fb")
        {
            GD.Print("Found Power Meter");
            androidPlugin.Call("subscribeToCharacteristic", message["service_uuid"], message["characteristic_uuid"]);
        }
    }
    /*
    void onCharacteristicChanged(Dictionary message)
    {
        GD.Print("onCharacteristicChanged");
        GD.Print(message);
        GD.Print(message["characteristic_uuid"]);

        if ((string)message["characteristic_uuid"] == "2A63")
        {
            GD.Print("Found Power Meter");
            androidPlugin.Call("subscribeToCharacteristic", message["service_uuid"], message["characteristic_uuid"]);
        }
    }*/

    void onCharacteristicRead(Dictionary message)
    {
        GD.Print("onCharacteristicRead");
        GD.Print(message);
        if ((string) message["service_uuid"] == "00001818-0000-1000-8000-00805f9b34fb" && 
            (string)message["characteristic_uuid"] == "00002a63-0000-1000-8000-00805f9b34fb")
        {
            int power = 0;
            int crankrev = 0;
            var data = (byte[])message["bytes"];
            using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(data)))
            {
                // file:///C:/Users/hugol/Downloads/GATT_Specification_Supplement_v5.pdf
                // 3.59 Cycling Power Measurement
                var bits = binaryReader.ReadUInt16();
                var ipower = binaryReader.ReadInt16();
                power = ipower;
                if (((bits >> 0) & 1) != 0)
                {
                    binaryReader.ReadByte();
                }
                if (((bits >> 2) & 1) != 0)
                {
                    binaryReader.ReadUInt16();
                }
                if (((bits >> 4) & 1) != 0)
                {
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt16();
                }
                // Cadence
                if (((bits >> 5) & 1) != 0)
                {
                    crankrev = binaryReader.ReadUInt16();
                    binaryReader.ReadUInt16();
                }
            }

            this.status.Text = $"Power: {power}, Cranks: {crankrev}";
            PowerChanged?.Invoke(power);

        }
    }


    void ChangeScan(bool newv)
    {
        scanning = newv;
        if(scanning)
        {
            button3.Disabled = true;
            bluetoothDevices.Clear();
            devices.Clear();
            button2.Text = "Stop Scan...";
        }
        else
        {
            button2.Text = "Start Scan";
        }
    }

    public void OnDebugMessage(string message)
    {
        GD.Print("OnDebugMessage");
        GD.Print(message);
    }

    public void OnDeviceFound(Dictionary dict)
    {
        GD.Print("OnDeviceFound");
        foreach(var d  in devices)
        {
            if ((string)d["address"] == (string)dict["address"])
            {
                return;
            }
        }
        devices.Add(dict);
        bluetoothDevices.AddItem((string)dict["name"]);
    }

    public void OnScanStopped(string message)
    {
        GD.Print("OnScanStopped");
        GD.Print(message);
        ChangeScan(false);
    }

}
