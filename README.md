# Stul
C# Knihovna pro práci se stolem pro LEGO projekty
## Použití
### Vytvoření stolu
```csharp
using (Stul stul = new Stul(nazevPortu))
{
    //Práce se stolem
}
```
K vypsání všech názvů portů lze použít [`System.IO.Ports.SerialPort.GetPortNames()`](https://docs.microsoft.com/cs-cz/dotnet/api/system.io.ports.serialport.getportnames?view=dotnet-plat-ext-6.0)
### Nastavení barvy pixelů
```csharp
stul[x, y].Stav = StavPixelu.Cervena;
stul.NastavVsechnyPixely(StavPixelu.Cervena);
```
Parametr `x` může být od `0` do `Stul.sirka`

Parametr `y` může být od `0` do `Stul.vyska`

Pro nastavení všech pixelů by se měla využívat metoda `stul.NastavVsechnyPixely()`, která arduinu pošle speciální příkaz pro nastavení všech pixelů najednou.
### Reagování na magnety
Pokud chceme reagovat na magnety přiložené k jednotlivým pixelům, přidáme vlastní medotu do `stul.MagnetEvent`
```csharp
stul.MagnetEvent += Stul_MagnetEvent;
```
```csharp
private void Stul_MagnetEvent(object sender, MagnetEventArgs e)
{
    //Reagování na magnety
}
```
`MagnetEventArgs` obsahuje informace o pixelu, který detekoval magnet, jako `e.X`, `e.Y` a `e.Pixel`
## Ukázka
Pixely na stole jsou ze začátku červené a zezelenají po přiložení magnetu.
```csharp
private static void Main()
{
    using (Stul stul = new Stul("COM-5"))
    {
        stul.NastavVsechnyPixely(StavPixelu.Cervena);

        stul.MagnetEvent += Stul_MagnetEvent;
    }
}

private static void Stul_MagnetEvent(object sender, MagnetEventArgs e)
{
    e.Pixel.Stav = StavPixelu.Zelena;
}
```
