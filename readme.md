# PyJETI

PyJETI is a Python library for interfacing with JETI spectrometers.

## Installation

Install the package directly from GitHub using pip:

```bash
pip install git+https://github.com/SBelgers/pyjeti.git@main
```

## Usage

Here is an example of how to use PyJETI to interact with a JETI spectrometer:

```python
from pyjeti.jeti import Spectrometer

with Spectrometer() as jeti:
    # Get the DLL version
    version = jeti.get_dll_version()
    print(f"JETI Radiometer DLL Version: {version[0]}.{version[1]}.{version[2]}")

    # Count connected devices
    connected_devices = jeti.count_connected_devices()
    print(f"Connected devices: {connected_devices}")

    # Retrieve serial numbers
    serial_numbers = jeti.get_serial()
    print(f"Board Serial: {serial_numbers['board_serial']}")
    print(f"Spectrometer Serial: {serial_numbers['spec_serial']}")
    print(f"Device Serial: {serial_numbers['device_serial']}")

    # Perform a measurement
    spd = jeti.measure()
    print("Spectral Power Distribution (SPD):")
    print(spd)
```

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/SBelgers/pyjeti/blob/main/LICENSE) file for details.