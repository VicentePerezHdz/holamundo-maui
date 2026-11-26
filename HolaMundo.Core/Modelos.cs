namespace HolaMundo.Core;

/// <summary>Operaciones de la calculadora. El símbolo es lo que se muestra en el Picker.</summary>
public enum Operacion { Suma, Resta, Multiplicacion, Division }

public static class OperacionExtensiones
{
    public static string Simbolo(this Operacion o) => o switch
    {
        Operacion.Suma => "+",
        Operacion.Resta => "−",
        Operacion.Multiplicacion => "×",
        Operacion.Division => "÷",
        _ => "?",
    };
}

/// <summary>Opción del combobox (Picker). ToString() es lo que pinta el Picker.</summary>
public record OpcionCatalogo(string Clave, string Texto, string Mensaje)
{
    public override string ToString() => Texto;
}

/// <summary>Éxito o error de una operación de negocio (igual que Resultado en el proyecto Kotlin).</summary>
public abstract record Resultado
{
    public sealed record Exito(string Valor) : Resultado;
    public sealed record Error(string Mensaje) : Resultado;
}
