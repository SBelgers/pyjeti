README: TO BE UPDATED


Install with `pip install git+https://github.com/SBelgers/pyjeti.git@main`

Usage example:
```
with Spectrometer() as jeti:
    version = jeti.get_dll_version()
    print(f"JETI Radiometer DLL Version: {version[0]}.{version[1]}.{version[2]}")

    connected_devices = jeti.count_connected_devices()
    print(f"connected devices: {connected_devices}")

    serial_numbers = jeti.get_serial()
    print(f"Board Serial: {serial_numbers['board_serial']}")
    print(f"Spectrometer Serial: {serial_numbers['spec_serial']}")
    print(f"Device Serial: {serial_numbers['device_serial']}")

    spd = jeti.measure()
    print("Spectral Power Distribution (SPD):")
    print(spd)
```