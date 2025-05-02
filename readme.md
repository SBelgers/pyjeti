Usage example:

```
from pyjeti import JETISpectrometerEx

with JETISpectrometerEx() as jeti:
    print(jeti.get_serial())
    print(jeti.measure())
```