from setuptools import setup

setup(
    name="pyjeti",
    version="0.1",
    packages=["pyjeti"],
    install_requires=["tqdm", "numpy"],
    package_data={
        "pyjeti": [
            "jeti_drivers\Win32\jeti_spectro_ex.dll",
            "jeti_drivers\Win64\jeti_spectro_ex64.dll",
            "jeti_error_codes.json",
        ]
    },
)
