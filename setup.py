from setuptools import setup

setup(
    name="pyjeti",
    version="0.1",
    packages=["pyjeti"],
    install_requires=["tqdm", "numpy"],
    package_data={
        "pyjeti": [
            "jeti_drivers/*",
            "jeti_error_codes.json",
        ]
    },
)
