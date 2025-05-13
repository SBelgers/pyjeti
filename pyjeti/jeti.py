import ctypes
import time
import platform
import json
import warnings
import numpy as np
from tqdm import tqdm
import importlib.resources
from typing import Optional

_ERROR_CODES_PATH = importlib.resources.files(__package__).joinpath(
    "jeti_error_codes.json"
)
with open(_ERROR_CODES_PATH, "r") as file:
    _ERROR_CODES = json.load(file)


class Spectrometer:
    """
    A class to interface with the JETI spectrometer devices.

    This class provides methods to interact with JETI spectrometers, including
    opening and closing connections, retrieving device information, and performing
    spectral measurements.
    """

    def __init__(
        self,
        radio_ex_dll_path: Optional[str] = None,
        simulate: bool = False,
    ) -> None:
        """
        Initialize the Spectrometer instance.

        Args:
            radio_ex_dll_path (Optional[str], optional): Path to the JETI RadioEx DLL.
            If None, the default path based on the system architecture will be used.
            Defaults to None.
            simulate (bool, optional): If True, enables simulation mode where no actual
                hardware interaction occurs. Defaults to False.
        """
        self._load_dll(radio_ex_dll_path)
        self.device_handle = None
        self.simulate = simulate

    def __enter__(self):
        self.open()
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        self.close()

    def get_dll_version(self) -> tuple[str, str, str]:
        """
        Retrieve the version of the JETI RadioEx DLL.

        Returns:
            tuple[str, str, str]: A tuple containing the major version, minor version,
                and build number of the DLL.
        """
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
            dll_path = importlib.resources.files(__package__).joinpath(
                "jeti_drivers\\Win64\\jeti_radio_ex64.dll"
            )
            self.dll = ctypes.WinDLL(dll_path)
        elif platform.architecture()[0] == "32bit":
            dll_path = importlib.resources.files(__package__).joinpath(
                "jeti_drivers\\Win64\\jeti_radio_ex.dll"
            )
            self.dll = ctypes.WinDLL(dll_path)

    def count_connected_devices(self) -> int:
        """
        Count the number of connected JETI spectrometer devices.

        Returns:
            int: The number of connected devices.
        """
        num_devices = ctypes.c_ulong()
        status = self.dll.JETI_GetNumRadioEx(ctypes.byref(num_devices))
        self._validate_status(status)
        return num_devices.value

    def validate_device_id(self, device_id: int) -> bool:
        """
        Validate the provided device ID.

        Args:
            device_id (int): The ID of the device to validate.

        Raises:
            Exception: If the device ID is invalid or out of range.

        Returns:
            bool: True if the device ID is valid, False otherwise.
        """
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
        """
        Get the serial numbers of the connected JETI spectrometer devices.

        If the spectrometer is in simulation mode, it returns placeholder serial numbers.

        Args:
            device_id (int, optional): The ID of the device. Defaults to 0.

        Returns:
            dict[str, str]: A dictionary containing the serial numbers of the board,
                spectrometer, and device.
        """
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
        """
        Validates the status code returned by an operation and raises an exception if the status indicates an error.

        Args:
            status (int): The status code to validate. A value of 0 indicates success, while any other value indicates an error.

        Raises:
            Exception: If the status code is not 0, an exception is raised with details about the error, including the error code,
                error name, and description retrieved from the `_ERROR_CODES` dictionary.
        """
        if status != 0:
            formatted_status = f"0x{status:02X}"
            error_info = _ERROR_CODES.get(formatted_status)
            self.close()
            raise Exception(
                f"Error: {formatted_status} ({error_info['name']} - {error_info['description']})"
            )

    def open(self, device_id: int = 0) -> None:
        """
        Opens a connection to the device with the specified device ID.

        Args:
            device_id (int, optional): The ID of the device to open. Defaults to 0.
        Notes:
            If `simulate` mode is enabled, no actual device will be opened, and a warning will be issued.

        """
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
        """
        Closes the connection to the device.
        """
        if self.simulate:
            warnings.warn("Simulated mode: No device to close.")
            self.device_handle = None
            return
        if self.device_handle:
            self.dll.JETI_CloseRadioEx(self.device_handle)
            self.device_handle = None

    def _prepare_measurement(
        self, integration_time_ms: int = 0, averaging: int = 1, step_nm: int = 1
    ) -> int:
        """
        Prepares the measurement settings for the device.
        This method configures the device with the specified integration time,
        averaging, and step size for measurements.

        Args:
            integration_time_ms (int, optional): The integration time in milliseconds. Defaults to 0.
            averaging (int, optional): The number of measurements to average. Defaults to 1.
            step_nm (int, optional): The step size in nanometers. Defaults to 1.

        Raises:
            Exception: If the device is not opened or the preparation fails.

        Returns:
            int: Status code returned by the device after preparation.
        """
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
        """
        Perform a spectral measurement using the JETI device.
        This method measures the spectral power distribution (SPD) of light
        using the JETI spectroradiometer. It supports both simulated and
        real measurement modes. In simulated mode, random spectral data is
        generated.

        Args:
            integration_time_ms (int, optional): Integration time for the measurement
                in milliseconds. If set to 0, the integration time is automatically
                determined. Defaults to 0.
            averaging (int, optional): Number of measurements to average for noise
                reduction. Defaults to 1.
            step_nm (int, optional): Wavelength step size in nanometers. Determines
                the resolution of the measurement. Defaults to 1.

        Raises:
            Exception: If the device is not opened or if an error occurs during
                the measurement process.

        Returns:
            np.ndarray: A 2D array where the first row contains the wavelengths
                and the second row contains the corresponding spectral irradiance
                values.
        """
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
        """
        Monitors the measurement process of the device, updating progress bars for timeout
        and expected duration, and raises an exception if the operation times out.

        Args:
            integration_time_ms (int, optional): The expected duration of the measurement
                in milliseconds. Defaults to 0.
            timeout_ms (int, optional): The maximum time to wait for the measurement to
                complete in milliseconds. Defaults to 240000.

        Raises:
            Exception: If the measurement process exceeds the specified timeout.
        """
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

    def _get_integration_time(self) -> float:
        """
        Retrieves the integration time of the connected device.


        Raises:
            Exception: If the device is not opened or accessible.

        Returns:
            float: The integration time of the device in seconds
        """
        if not self.device_handle:
            raise Exception("Device not opened.")
        integration_time = ctypes.c_float()
        status = self.dll.JETI_RadioTintEx(
            self.device_handle, ctypes.byref(integration_time)
        )
        self._validate_status(status)
        return integration_time.value
