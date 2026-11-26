using HolaMundo.Core;

namespace HolaMundo.Maui;

/// <summary>
/// Code-behind mínimo: recibe el ViewModel por constructor (DI) y lo asigna como BindingContext.
/// Toda la lógica vive en MainViewModel; la vista solo tiene bindings en el XAML.
/// </summary>
public partial class MainPage : ContentPage
{
	private readonly MainViewModel _vm;

	public MainPage(MainViewModel vm)
	{
		InitializeComponent();
		BindingContext = _vm = vm;
	}

	/// <summary>Equivale a onResume / ngOnInit: carga el combo al mostrarse la página.</summary>
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.CargarCommand.ExecuteAsync(null);
	}
}
