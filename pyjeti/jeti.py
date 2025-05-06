import ctypes
import time
import platform
import json
import warnings
import numpy as np
from tqdm import tqdm
import importlib.resources
from typing import Optional

with importlib.resources.files(__package__).joinpath("jeti_error_codes.json") as path:
    with open(path, "r") as file:
        _ERROR_CODES = json.load(file)


class Spectrometer:
    def __init__(
        self,
        radio_ex_dll_path: Optional[str] = None,
        simulate: bool = False,
    ):
        self._load_dll(radio_ex_dll_path)
        self.device_handle = None
        self.simulate = simulate

    def __enter__(self):
        self.open()
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        self.close()

    def get_dll_version(self) -> tuple[str, str, str]:
        major_version = ctypes.c_ushort()
        minor_version = ctypes.c_ushort()
        build_number = ctypes.c_ushort()
        status = self.dll.JETI_GetRadioExDLLVersion(
            ctypes.byref(major_version),
            ctypes.byref(minor_version),
            ctypes.byref(build_number),
        )
        self._validate_status(status)
        return (major_version.value, minor_version.value, build_number.value)

    def _load_dll(self, path: str) -> None:
        if path is not None:
            self.dll = ctypes.WinDLL(path)
            return
        if platform.architecture()[0] == "64bit":
            with importlib.resources.files(__package__).joinpath(
                "jeti_drivers\Win64\jeti_spectro_ex64.dll"
            ) as dll_path:
                self.dll = ctypes.WinDLL(dll_path)
        elif platform.architecture()[0] == "32bit":
            with importlib.resources.files(__package__).joinpath(
                "jeti_drivers\Win64\jeti_spectro_ex64.dll"
            ) as dll_path:
                self.dll = ctypes.WinDLL(dll_path)

    def count_connected_devices(self) -> int:
        num_devices = ctypes.c_ulong()
        status = self.dll.JETI_GetNumRadioEx(ctypes.byref(num_devices))
        self._validate_status(status)
        return num_devices.value

    def validate_device_id(self, device_id: int) -> bool:
        if self.simulate:
            warnings.warn("Simulated mode: No device ID validation performed.")
            return True
        num_devices = self.count_connected_devices()
        if device_id < 0 or device_id >= num_devices:
            raise Exception(
                f"Invalid device ID: {device_id}. Valid range: 0 to {num_devices - 1}."
            )
        return True

    def get_serial(self, device_id: int = 0) -> dict[str, str]:
        if self.simulate:
            warnings.warn("Simulated mode: Serial is not real.")
            return {
                "board_serial": "-1",
                "spec_serial": "-1",
                "device_serial": "-1",
            }
        self.validate_device_id(device_id)

        board_serial = ctypes.create_string_buffer(256)
        spec_serial = ctypes.create_string_buffer(256)
        device_serial = ctypes.create_string_buffer(256)

        status = self.dll.JETI_GetSerialRadioEx(
            device_id, board_serial, spec_serial, device_serial
        )
        self._validate_status(status)
        return {
            "board_serial": board_serial.value.decode(),
            "spec_serial": spec_serial.value.decode(),
            "device_serial": device_serial.value.decode(),
        }

    def _validate_status(self, status: int) -> None:
        if status != 0:
            formatted_status = f"0x{status:02X}"
            error_info = _ERROR_CODES.get(formatted_status)
            self.close()
            raise Exception(
                f"Error: {formatted_status} ({error_info['name']} - {error_info['description']})"
            )

    def open(self, device_id: int = 0) -> None:
        if self.simulate:
            warnings.warn("Simulated mode: No device opened.")
            self.device_handle = ctypes.c_ulonglong(0)
            return

        self.validate_device_id(device_id)

        self.device_handle = ctypes.c_ulonglong()
        if device_id is None:
            device_id = 0
        status = self.dll.JETI_OpenRadioEx(device_id, ctypes.byref(self.device_handle))
        self._validate_status(status)

    def close(self) -> None:
        if self.simulate:
            warnings.warn("Simulated mode: No device to close.")
            self.device_handle = None
            return
        if self.device_handle:
            self.dll.JETI_CloseRadioEx(self.device_handle)
            self.device_handle = None

    def _prepare_measurement(self, integration_time_ms=0, averaging=1, step_nm=1):
        if self.simulate:
            warnings.warn("Simulated mode: No measurement preparation.")
            return 0

        if not self.device_handle:
            raise Exception("Device not opened.")

        status = self.dll.JETI_PrepareMeasureEx(
            self.device_handle,
            ctypes.c_float(integration_time_ms),
            ctypes.c_ushort(averaging),
            ctypes.c_ulong(step_nm),
        )

        self._validate_status(status)

        return status

    def measure(
        self, integration_time_ms: int = 0, averaging: int = 1, step_nm: int = 1
    ) -> np.ndarray:
        if self.simulate:
            warnings.warn("Simulated mode: No measurement performed.")
            start_wavelength = 380
            end_wavelength = 780
            num_values = int((end_wavelength - start_wavelength) / step_nm) + 1
            wl = np.arange(start_wavelength, end_wavelength + step_nm, step_nm).astype(
                int
            )
            spectrum = np.random.rand(num_values).astype(np.float32) / 1000
            spd = np.vstack((wl, spectrum))
            return spd

        if not self.device_handle:
            raise Exception("Device not opened.")
        if integration_time_ms == 0:
            status = self._prepare_measurement(integration_time_ms, averaging, step_nm)
            self._validate_status(status)
            integration_time_est = self._get_integration_time()
        else:
            integration_time_est = integration_time_ms
        status = self.dll.JETI_MeasureEx(
            self.device_handle,
            ctypes.c_float(integration_time_ms),
            ctypes.c_ushort(averaging),
            ctypes.c_ulong(step_nm),
        )

        self._validate_status(status)
        self._measuring_block(integration_time_est)

        start_wavelength = 380
        end_wavelength = 780
        num_values = int((end_wavelength - start_wavelength) / step_nm) + 1

        irradiance_values = (ctypes.c_float * num_values)()
        self.dll.JETI_SpecRadEx(
            self.device_handle,
            ctypes.c_ulong(start_wavelength),
            ctypes.c_ulong(end_wavelength),
            irradiance_values,
        )
        wl = np.arange(start_wavelength, end_wavelength + step_nm, step_nm)
        spd = np.vstack((wl, irradiance_values))

        return spd

    def _measuring_block(
        self, integration_time_ms: int = 0, timeout_ms: int = 240000
    ) -> None:
        is_busy = ctypes.c_int()
        start_time = time.time()
        with (
            tqdm(total=timeout_ms, desc="Timeout", unit="ms") as timeout_bar,
            tqdm(
                total=integration_time_ms,
                desc="Expected Duration",
                unit="ms",
            ) as duration_bar,
        ):
            while True:
                status = self.dll.JETI_MeasureStatusEx(
                    self.device_handle, ctypes.byref(is_busy)
                )
                if not is_busy.value:
                    break
                elapsed_time = round((time.time() - start_time) * 1000)
                if elapsed_time > timeout_ms:
                    raise Exception(
                        f"Timeout waiting for measurement to complete. Device status: {status}"
                    )
                timeout_bar.update(elapsed_time - timeout_bar.n)
                duration_bar.update(
                    min(elapsed_time, integration_time_ms) - duration_bar.n
                )

    def _get_integration_time(self):
        if not self.device_handle:
            raise Exception("Device not opened.")
        integration_time = ctypes.c_float()
        status = self.dll.JETI_RadioTintEx(
            self.device_handle, ctypes.byref(integration_time)
        )
        self._validate_status(status)
        return integration_time.value


# Usage example
if __name__ == "__main__":
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
