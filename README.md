# Hola Mundo · .NET MAUI 10

[![CI](https://github.com/VicentePerezHdz/holamundo-maui/actions/workflows/ci.yml/badge.svg)](https://github.com/VicentePerezHdz/holamundo-maui/actions/workflows/ci.yml) [![Licencia: MIT](https://img.shields.io/badge/licencia-MIT-blue.svg)](LICENSE)

App multiplataforma con .NET MAUI, el sucesor de Xamarin.Forms. Tiene tres funciones: saludo con input de texto, calculadora y combobox con mensaje.

## Qué demuestra
- MVVM con CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`).
- Inyección de dependencias en `MauiProgram`.
- XAML con bindings compilados.
- Lógica en un proyecto `Core` que se prueba sin emulador.

## Cómo correrlo
```powershell
dotnet test HolaMundo.Core.Tests                        # 9 pruebas xUnit
dotnet build HolaMundo.Maui -f net10.0-android          # genera el APK
dotnet build HolaMundo.Maui -f net10.0-android -t:Run   # lo instala y abre en el emulador o teléfono
```

La explicación completa (incluida la migración desde Xamarin) está en [`docs/Documento_Transicion.md`](docs/Documento_Transicion.md).

## Licencia

[MIT](LICENSE)
