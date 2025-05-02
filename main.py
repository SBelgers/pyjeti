# Example code
from jeti import JETISpectrometerEx

if __name__ == "__main__":
    with JETISpectrometerEx() as jeti:
        print(jeti.get_dll_version())
