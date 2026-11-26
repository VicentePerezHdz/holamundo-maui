using HolaMundo.Core;

namespace HolaMundo.Core.Tests;

public class MainViewModelTests
{
    private static MainViewModel Vm(ICatalogoService? catalogo = null) =>
        new(new SaludoService(), new CalculadoraService(), catalogo ?? new CatalogoService());

    [Fact]
    public void Saludar_con_nombre_valido_muestra_mensaje()
    {
        var vm = Vm();
        vm.Nombre = "Vicente";
        vm.SaludarCommand.Execute(null);
        Assert.StartsWith("¡Hola, Vicente!", vm.MensajeSaludo);
        Assert.False(vm.SaludoEsError);
    }

    [Theory]
    [InlineData(null, "El nombre es obligatorio.")]
    [InlineData("V", "El nombre debe tener al menos 2 caracteres.")]
    public void Saludar_con_nombre_invalido_marca_error(string? nombre, string esperado)
    {
        var vm = Vm();
        vm.Nombre = nombre;
        vm.SaludarCommand.Execute(null);
        Assert.Equal(esperado, vm.MensajeSaludo);
        Assert.True(vm.SaludoEsError);
    }

    [Theory]
    [InlineData("10", "5", 0, "10 + 5 = 15")]
    [InlineData("10", "5", 1, "10 − 5 = 5")]
    [InlineData("2.5", "4", 2, "2.5 × 4 = 10")]
    [InlineData("10", "4", 3, "10 ÷ 4 = 2.5")]
    public void Calcular_las_cuatro_operaciones(string a, string b, int indice, string esperado)
    {
        var vm = Vm();
        (vm.NumeroA, vm.NumeroB, vm.IndiceOperacion) = (a, b, indice);
        vm.CalcularCommand.Execute(null);
        Assert.Equal(esperado, vm.ResultadoCalculo);
    }

    [Fact]
    public void Dividir_entre_cero_es_error()
    {
        var vm = Vm();
        (vm.NumeroA, vm.NumeroB, vm.IndiceOperacion) = ("1", "0", 3);
        vm.CalcularCommand.Execute(null);
        Assert.True(vm.CalculoEsError);
    }

    [Fact]
    public async Task Cargar_llena_el_combo_y_al_elegir_avisa_el_mensaje()
    {
        var vm = Vm(new CatalogoFalso());
        var avisos = new List<string?>();
        vm.PropertyChanged += (_, e) => avisos.Add(e.PropertyName);

        await vm.CargarCommand.ExecuteAsync(null);
        vm.LenguajeSeleccionado = vm.Lenguajes[0];

        Assert.Single(vm.Lenguajes);
        Assert.Equal("msg", vm.MensajeLenguaje);
        Assert.Contains(nameof(MainViewModel.MensajeLenguaje), avisos); // la vista se entera del cambio
    }

    private sealed class CatalogoFalso : ICatalogoService
    {
        public Task<IReadOnlyList<OpcionCatalogo>> ObtenerLenguajesAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<OpcionCatalogo>>([new("k", "Kotlin", "msg")]);
    }
}
