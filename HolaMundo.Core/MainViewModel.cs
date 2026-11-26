using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HolaMundo.Core;

/// <summary>
/// ViewModel MVVM con CommunityToolkit.Mvvm. Los generadores de código crean, a partir de los atributos:
///  - [ObservableProperty] campo "nombre"  -> propiedad pública "Nombre" que avisa a la vista (INotifyPropertyChanged)
///  - [RelayCommand] método "Saludar"      -> propiedad "SaludarCommand" (ICommand) para el botón
/// No referencia MAUI: por eso vive en HolaMundo.Core y se prueba con xUnit sin emulador.
/// </summary>
public partial class MainViewModel(ISaludoService saludo, ICalculadoraService calculadora, ICatalogoService catalogo)
    : ObservableObject
{
    // ----- 1. Saludo -----
    [ObservableProperty] private string? nombre;
    [ObservableProperty] private string? mensajeSaludo;
    [ObservableProperty] private bool saludoEsError;

    [RelayCommand]
    private void Saludar() => (MensajeSaludo, SaludoEsError) = Pintar(saludo.Saludar(Nombre));

    // ----- 2. Calculadora -----
    public IReadOnlyList<Operacion> Operaciones { get; } = Enum.GetValues<Operacion>();
    public IReadOnlyList<string> SimbolosOperacion { get; } = Enum.GetValues<Operacion>().Select(o => o.Simbolo()).ToList();

    [ObservableProperty] private string? numeroA = "10";
    [ObservableProperty] private string? numeroB = "5";
    [ObservableProperty] private int indiceOperacion;
    [ObservableProperty] private string? resultadoCalculo;
    [ObservableProperty] private bool calculoEsError;

    [RelayCommand]
    private void Calcular() =>
        (ResultadoCalculo, CalculoEsError) = Pintar(calculadora.Calcular(NumeroA, NumeroB, Operaciones[Math.Max(0, IndiceOperacion)]));

    // ----- 3. Combobox -----
    public ObservableCollection<OpcionCatalogo> Lenguajes { get; } = [];
    [ObservableProperty] private OpcionCatalogo? lenguajeSeleccionado;
    [ObservableProperty] private bool cargando;

    /// <summary>Método parcial que el generador llama cuando cambia LenguajeSeleccionado.</summary>
    partial void OnLenguajeSeleccionadoChanged(OpcionCatalogo? value) => OnPropertyChanged(nameof(MensajeLenguaje));

    public string? MensajeLenguaje => LenguajeSeleccionado?.Mensaje;

    /// <summary>AsyncRelayCommand: mientras corre, CargarCommand.IsRunning = true y no se ejecuta dos veces.</summary>
    [RelayCommand]
    private async Task CargarAsync(CancellationToken ct)
    {
        if (Lenguajes.Count > 0) return;
        Cargando = true;
        try
        {
            foreach (var o in await catalogo.ObtenerLenguajesAsync(ct)) Lenguajes.Add(o);
        }
        finally { Cargando = false; }
    }

    private static (string Texto, bool EsError) Pintar(Resultado r) => r switch
    {
        Resultado.Exito e => (e.Valor, false),
        Resultado.Error e => (e.Mensaje, true),
        _ => ("", true),
    };
}
