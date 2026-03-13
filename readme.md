# PyJETI

Python interface for the **JETI specbos 1201** (tested) and **JETI specbos 1211** (untested) spectroradiometers on Windows.

The package bundles the JETI RadioEx SDK DLLs so no separate SDK installation is needed. See [License](#license) for details.

## Requirements

- Windows (32-bit or 64-bit)
- Python ≥ 3.10
- FTDI D2XX USB driver — typically installed automatically by Windows when you first connect the instrument. If measurements fail or no devices are detected, download the driver manually from [ftdichip.com/drivers/d2xx-drivers](https://ftdichip.com/drivers/d2xx-drivers/).

## Installation

```bash
pip install pyjeti
```

Or directly from GitHub:

```bash
pip install git+https://github.com/SBelgers/pyjeti.git@main
```

## Quick Start

```python
from pyjeti.jeti import Spectrometer

with Spectrometer() as jeti:
    print("DLL version:", jeti.get_dll_version())
    print("Connected devices:", jeti.count_connected_devices())

    serials = jeti.get_serial()
    print("Board serial:       ", serials["board_serial"])
    print("Spectrometer serial:", serials["spec_serial"])
    print("Device serial:      ", serials["device_serial"])

    spd = jeti.measure()
    print("Wavelengths (nm):    ", spd[0])
    print("Irradiance (W/m²/nm):", spd[1])
```

## API Reference

### `Spectrometer(radio_ex_dll_path=None, simulate=False)`

Create a spectrometer handle. Intended to be used as a context manager so the
device connection is properly opened and closed.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `radio_ex_dll_path` | `str \| None` | `None` | Path to a custom `jeti_radio_ex64.dll`. Uses the bundled DLL when `None`. |
| `simulate` | `bool` | `False` | Simulation mode — no hardware required; returns random spectral data. |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `open(device_id=0)` | `None` | Open connection to the device. Called automatically by the context manager. |
| `close()` | `None` | Close the connection. Called automatically by the context manager. |
| `get_dll_version()` | `tuple[int, int, int]` | JETI RadioEx DLL version `(major, minor, build)`. |
| `count_connected_devices()` | `int` | Number of connected JETI specbos devices. |
| `get_serial(device_id=0)` | `dict` | Serial numbers: keys `board_serial`, `spec_serial`, `device_serial`. |
| `measure(integration_time_ms=0, averaging=1, step_nm=1)` | `np.ndarray` | 2×N array of `[wavelengths_nm, irradiance_W_m2_nm]`. |
| `validate_device_id(device_id)` | `bool` | Raises if the device ID is out of range. |

### `measure()` parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `integration_time_ms` | `0` | Integration time in ms. `0` triggers auto-exposure. |
| `averaging` | `1` | Number of measurements to average. |
| `step_nm` | `1` | Wavelength step size in nm. `1` gives 401 points (380–780 nm). |

### Return value of `measure()`

A `(2, N)` NumPy array:

```python
spd = jeti.measure()
wavelengths = spd[0]   # e.g. [380., 381., ..., 780.] nm
irradiance  = spd[1]   # W m⁻² nm⁻¹
```

## Simulation Mode

Useful for development and testing without hardware:

```python
with Spectrometer(simulate=True) as jeti:
    spd = jeti.measure()
    print(spd.shape)   # (2, 401)
```

All methods work in simulation mode but return placeholder or random data and
emit a `UserWarning`.

## Hardware Notes

### specbos 1201

Handheld spectroradiometer, 380–780 nm, 1 nm resolution. Fully tested.

### specbos 1211

Similar form factor with extended UV/NIR range. The same API applies, but
**untested**. The current `measure()` implementation hardcodes 380–780 nm, so
wavelengths outside that range will not be captured.

## Troubleshooting

**`count_connected_devices()` returns 0**  
→ The FTDI D2XX USB driver may be missing. Download from
[ftdichip.com/drivers/d2xx-drivers](https://ftdichip.com/drivers/d2xx-drivers/)
and install, then reconnect the instrument.

**DLL load error / `OSError` on import**  
→ If you have a newer version of the JETI SDK installed, pass the path to its
DLL explicitly:

```python
Spectrometer(radio_ex_dll_path=r"C:\path\to\jeti_radio_ex64.dll")
```

**Error code exceptions**  
All JETI error codes are mapped to descriptive messages. The exception text
includes the hex code, name, and description, e.g.:
`Error: 0x03 (JETI_ERR_DEVICE_NOT_OPEN - Device handle is invalid)`.

## License

The Python source code is released under the **MIT License** (see `LICENSE`).

The JETI RadioEx SDK DLLs in `pyjeti/jeti_drivers/` are governed by the
[JETI SDK EULA](pyjeti/jeti_drivers/license.txt).
Distribution as part of an integrated software product is permitted under
clause 2a of that agreement. You may not disassemble, decompile,
reverse-engineer, transfer, modify, sell, lease, or sublicense those DLLs.
