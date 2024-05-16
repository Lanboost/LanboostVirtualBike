extends Node2D

# TODO: Update to match your plugin's name
var _plugin_name = "LanboostBluetoothGodotPlugin"
var _android_plugin

func _ready():
	if Engine.has_singleton(_plugin_name):
		_android_plugin = Engine.get_singleton(_plugin_name)
		var err = _android_plugin.connect("_on_debug_message", _on_debug_message)
		err = _android_plugin.connect("_on_device_found", _on_device_found)
		err = _android_plugin.connect("_on_scan_stopped", _on_scan_stopped)
		if err != OK:
			print("error connecting to signal: " + str(err))
		else:
			print("connected to signal")
	else:
		printerr("Couldn't find plugin " + _plugin_name)

func _on_Button_pressed():
	return;
	if _android_plugin:
		# TODO: Update to match your plugin's API
		print("[_on_Button_pressed] ")
		_android_plugin.scan()

func _on_debug_message(status):
	
	print("[_on_debug_message] " + status)

func _on_device_found(dict):
	
	print("[_on_device_found] ")

func _on_scan_stopped(arg):
	print("[_on_scan_stopped] ")
