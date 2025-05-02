# Example code
if __name__ == "__main__":
    from jeti import JETISpectrometerEx

    with open(JETISpectrometerEx()) as jeti:
        print(jeti.get_dll_version())
