using HolaMundo.Core;
using Microsoft.Extensions.Logging;

namespace HolaMundo.Maui;

/// <summary>
/// Punto de arranque (equivale a Program.cs en ASP.NET Core y a App.kt + Dagger en Kotlin).
/// MAUI trae el mismo contenedor de DI de .NET: AddSingleton / AddTransient.
/// </summary>
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Servicios: sin estado, una sola instancia.
		builder.Services.AddSingleton<ISaludoService, SaludoService>();
		builder.Services.AddSingleton<ICalculadoraService, CalculadoraService>();
		builder.Services.AddSingleton<ICatalogoService, CatalogoService>();

		// ViewModel y página: Transient (nuevos cada vez que se navega). Shell los pide al contenedor.
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif
		return builder.Build();
	}
}
