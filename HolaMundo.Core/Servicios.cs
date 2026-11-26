using System.Globalization;

namespace HolaMundo.Core;

// Interfaces: el ViewModel depende de ellas, no de las clases. En pruebas se pueden reemplazar.

public interface ISaludoService { Resultado Saludar(string? nombre); }

public interface ICalculadoraService { Resultado Calcular(string? textoA, string? textoB, Operacion operacion); }

public interface ICatalogoService { Task<IReadOnlyList<OpcionCatalogo>> ObtenerLenguajesAsync(CancellationToken ct = default); }

public class SaludoService : ISaludoService
{
    public const int MinCaracteres = 2;

    public Resultado Saludar(string? nombre)
    {
        var limpio = nombre?.Trim() ?? "";
        if (limpio.Length == 0) return new Resultado.Error("El nombre es obligatorio.");
        if (limpio.Length < MinCaracteres) return new Resultado.Error($"El nombre debe tener al menos {MinCaracteres} caracteres.");
        return new Resultado.Exito($"¡Hola, {limpio}! Saludo desde .NET MAUI.");
    }
}

public class CalculadoraService : ICalculadoraService
{
    public Resultado Calcular(string? textoA, string? textoB, Operacion operacion)
    {
        // InvariantCulture: acepta "2.5" sin importar si el teléfono está en español (que usaría "2,5").
        if (!decimal.TryParse(textoA, NumberStyles.Number, CultureInfo.InvariantCulture, out var a))
            return new Resultado.Error("El número A no es válido.");
        if (!decimal.TryParse(textoB, NumberStyles.Number, CultureInfo.InvariantCulture, out var b))
            return new Resultado.Error("El número B no es válido.");

        decimal r;
        switch (operacion)
        {
            case Operacion.Suma: r = a + b; break;
            case Operacion.Resta: r = a - b; break;
            case Operacion.Multiplicacion: r = a * b; break;
            case Operacion.Division:
                if (b == 0) return new Resultado.Error("No se puede dividir entre cero.");
                r = Math.Round(a / b, 4);
                break;
            default: return new Resultado.Error("Operación desconocida.");
        }
        string F(decimal d) => d.ToString("0.####", CultureInfo.InvariantCulture);
        return new Resultado.Exito($"{F(a)} {operacion.Simbolo()} {F(b)} = {F(r)}");
    }
}

/// <summary>Simula una lectura asíncrona de una BD local (en MAUI sería SQLite con sqlite-net-pcl).</summary>
public class CatalogoService : ICatalogoService
{
    public async Task<IReadOnlyList<OpcionCatalogo>> ObtenerLenguajesAsync(CancellationToken ct = default)
    {
        await Task.Delay(100, ct);
        return
        [
            new("maui", ".NET MAUI", "Una sola base de código C# para Android, iOS, Windows y Mac."),
            new("xamarin", "Xamarin.Forms (legado)", "Antecesor de MAUI; sin soporte desde mayo de 2024. Se migra a MAUI."),
            new("kotlin", "Kotlin / Android", "Nativo de Android; ver el proyecto 02."),
            new("csharp", "C# / .NET 8", "La API que consumirían las apps en línea (proyecto 01)."),
        ];
    }
}
