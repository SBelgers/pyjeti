import ctypes


class JETISpectrometerEx:
    def __init__(
        self,
        dll_path: str = r"jeti_drivers\Win64\jeti_radio_ex64.dll",
        verbose: bool = False,
    ) -> None:
        self.dll = ctypes.WinDLL(dll_path)
        self.device_handle = None
        self.verbose = verbose

    def __enter__(self):
        self.open()
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        self.close()

    def open(self) -> None:
        num_devices = ctypes.c_ulong()
        self.dll.JETI_GetNumSpectroEx(ctypes.byref(num_devices))
        if num_devices.value == 0:
            raise Exception("No JETI spectrometer devices found.")

        self.device_handle = ctypes.c_ulonglong()
        self.dll.JETI_OpenSpectroEx(0, ctypes.byref(self.device_handle))
        if not self.device_handle.value:
            raise Exception("Failed to open JETI spectrometer device.")

    def close(self) -> None:
        if self.device_handle:
            self.dll.JETI_CloseSpectroEx(self.device_handle)
            self.device_handle = None
        else:
            raise Exception("Device not opened.")

    def get_dll_version(self):
        major_version = ctypes.c_ushort()
        minor_version = ctypes.c_ushort()
        build_number = ctypes.c_ushort()
        status = self.dll.JETI_GetSpectroExDLLVersion(
            ctypes.byref(major_version),
            ctypes.byref(minor_version),
            ctypes.byref(build_number),
        )
        if status != 0:
            raise Exception(f"Failed to get DLL version. Error code: {status}")
        return (major_version.value, minor_version.value, build_number.value)

    def get_serial(self) -> dict[str, str]:
        if not self.device_handle:
            raise Exception("Device not opened.")

        board_serial = ctypes.create_string_buffer(256)
        spec_serial = ctypes.create_string_buffer(256)
        device_serial = ctypes.create_string_buffer(256)

        self.dll.JETI_GetSerialSpectroEx(0, board_serial, spec_serial, device_serial)

        return {
            "board_serial": board_serial.value.decode(),
            "spec_serial": spec_serial.value.decode(),
            "device_serial": device_serial.value.decode(),
        }

    def measure(self, integration_time_ms: int, averaging: int = 1) -> list[float]:
        if not self.device_handle:
            raise Exception("Device not opened.")

        self.dll.JETI_StartLightEx(
            self.device_handle,
            ctypes.c_float(integration_time_ms),
            ctypes.c_ushort(averaging),
        )

        # Wait for the measurement to complete
        is_busy = ctypes.c_int()
        while True:
            self.dll.JETI_SpectroStatusEx(self.device_handle, ctypes.byref(is_busy))
            if not is_busy.value:
                break

        pixel_count = ctypes.c_ulong()
        self.dll.JETI_PixelCountEx(self.device_handle, ctypes.byref(pixel_count))

        light_values = (ctypes.c_int * pixel_count.value)()
        self.dll.JETI_LightPixEx(self.device_handle, light_values)

        return list(light_values)
