# Documento de transición: Hola Mundo .NET MAUI (sucesor de Xamarin)

> Manual para entender qué se construyó, qué hace cada clase, interfaz y archivo XAML, y cómo volver a hacerlo.
> Fecha: 2026-11-23 · Ruta: `C:\HolaMundo\06-MAUI`

## 0. ¿Por qué MAUI y no Xamarin?

Microsoft terminó el soporte de **Xamarin y Xamarin.Forms el 1 de mayo de 2024**. Ya no recibe parches y las tiendas exigen SDK de Android/iOS que Xamarin no soporta. Su sucesor directo es **.NET MAUI**: mismo C#, mismo XAML y mismo patrón MVVM. En los proyectos que todavía usan "Xamarin" casi siempre se trata de **mantener o migrar** una app Xamarin a MAUI, así que conviene conocer ambos. La tabla de la sección 6 muestra qué cambia.

Tu equipo tiene el workload de MAUI 10 (Android, iOS, Mac Catalyst, Windows). El proyecto apunta solo a **Android** (`net10.0-android`), igual que el proyecto Kotlin. Para agregar Windows se suma `net10.0-windows10.0.19041.0` en `TargetFrameworks`, pero eso descarga el Windows App SDK, cuya descarga falló al crear este proyecto.

## 1. Qué hace la aplicación

La misma pantalla del proyecto Kotlin, offline:

| Tarjeta | Control MAUI | Binding / comando | Quién resuelve |
|---|---|---|---|
| Saludo | `Entry` + `Button` | `Nombre`, `SaludarCommand`, `MensajeSaludo`, `SaludoEsError` | `SaludoService` |
| Calculadora | 2 `Entry` + `Picker` + `Button` | `NumeroA`, `NumeroB`, `IndiceOperacion`, `CalcularCommand`, `ResultadoCalculo` | `CalculadoraService` (decimal) |
| Combobox | `Picker` | `Lenguajes`, `LenguajeSeleccionado`, `MensajeLenguaje`, `CargarCommand` | `CatalogoService` (async) |

## 2. Cómo correrlo

```powershell
cd C:\HolaMundo\06-MAUI
dotnet test HolaMundo.Core.Tests                         # 9 pruebas xUnit, sin emulador
dotnet build HolaMundo.Maui -f net10.0-android           # genera el .apk en HolaMundo.Maui\bin\Debug\net10.0-android
dotnet build HolaMundo.Maui -f net10.0-android -t:Run    # instala y abre en el emulador/teléfono conectado
```

En Visual Studio 2022/2026: abre `HolaMundoMaui.slnx`, elige `HolaMundo.Maui` como proyecto de inicio, selecciona un emulador Android y presiona ▶.

## 3. Arquitectura

```
┌──────────────────────── HolaMundo.Maui (net10.0-android) ────────────────────────┐
│ MauiProgram.CreateMauiApp()  ── registra en DI: servicios, MainViewModel, MainPage │
│ App → AppShell → ShellContent(MainPage)  (Shell pide MainPage al contenedor)       │
│ MainPage.xaml  ── bindings ──►  BindingContext = MainViewModel                     │
│ MainPage.xaml.cs: constructor(MainViewModel), OnAppearing → CargarCommand          │
└──────────────────────────────────────┬────────────────────────────────────────────┘
                                       │ referencia de proyecto
┌──────────────────────── HolaMundo.Core (net10.0, sin MAUI) ───────────────────────┐
│ MainViewModel (CommunityToolkit.Mvvm: [ObservableProperty], [RelayCommand])        │
│     ├─► ISaludoService ───────► SaludoService                                      │
│     ├─► ICalculadoraService ──► CalculadoraService                                 │
│     └─► ICatalogoService ─────► CatalogoService (async)                            │
│ Modelos: Operacion, OpcionCatalogo, Resultado (Exito / Error)                      │
└──────────────────────────────────────▲────────────────────────────────────────────┘
                                       │
                   HolaMundo.Core.Tests (xUnit): prueba el ViewModel directo
```

La decisión clave es separar **Core** (C# puro) de la app MAUI. El ViewModel no sabe nada de pantallas, así que se prueba sin emulador. Es la misma idea que `:core` / `:app` en el proyecto Kotlin.

## 4. Recorrido de un evento (Saludar)

1. El usuario escribe en el `Entry`. El binding `Text="{Binding Nombre}"` es de **dos vías**, así que cada tecla actualiza `MainViewModel.Nombre`.
2. Toca **Saludar**. El `Button` tiene `Command="{Binding SaludarCommand}"` y ejecuta `Saludar()`.
3. `Saludar()` llama `ISaludoService.Saludar(Nombre)`, que devuelve `Resultado.Exito` o `Resultado.Error`.
4. `Pintar()` convierte el resultado en `(texto, esError)` y asigna `MensajeSaludo` y `SaludoEsError`.
5. Las propiedades generadas lanzan `PropertyChanged`. El `Label` se actualiza solo, y el `DataTrigger` lo pinta rojo si `SaludoEsError` es `true`.

El combo: `OnAppearing()` ejecuta `CargarCommand`, que llena la `ObservableCollection` y avisa a la vista de cada elemento nuevo. Al elegir una opción, el binding de dos vías asigna `LenguajeSeleccionado`; el método parcial `OnLenguajeSeleccionadoChanged` notifica `MensajeLenguaje` y el `Label` se actualiza.

## 5. Inventario de clases, interfaces y archivos

### HolaMundo.Core

| Archivo | Tipo | Miembros | Responsabilidad |
|---|---|---|---|
| `Modelos.cs` | enum, record, record abstracto | `Operacion`, `OperacionExtensiones.Simbolo()`, `OpcionCatalogo`, `Resultado.Exito / Resultado.Error` | Modelos |
| `Servicios.cs` | interfaces + clases | `ISaludoService.Saludar`, `ICalculadoraService.Calcular`, `ICatalogoService.ObtenerLenguajesAsync` y sus implementaciones | Reglas de negocio |
| `MainViewModel.cs` | `ObservableObject` parcial | propiedades generadas `Nombre`, `MensajeSaludo`, `SaludoEsError`, `NumeroA`, `NumeroB`, `IndiceOperacion`, `ResultadoCalculo`, `CalculoEsError`, `LenguajeSeleccionado`, `Cargando`; comandos `SaludarCommand`, `CalcularCommand`, `CargarCommand`; `Lenguajes`, `SimbolosOperacion`, `MensajeLenguaje` | Estado y comandos de la pantalla |

### HolaMundo.Maui

| Archivo | Responsabilidad |
|---|---|
| `MauiProgram.cs` | Contenedor de DI y fuentes (como `Program.cs`) |
| `App.xaml(.cs)` | Recursos globales (colores, estilos) y crea la ventana con `AppShell` |
| `AppShell.xaml` | Navegación; define la ruta `MainPage` |
| `MainPage.xaml` | La vista: `Border` (tarjetas), `Entry`, `Picker`, `Button`, `Label` con `DataTrigger`, `x:DataType` para bindings compilados |
| `MainPage.xaml.cs` | Recibe el ViewModel por DI y carga el combo en `OnAppearing` |
| `Platforms/Android/*` | `MainActivity`, `MainApplication` y `AndroidManifest.xml` generados por la plantilla |
| `Resources/*` | Ícono, splash, fuentes, estilos y colores de la plantilla |
| `HolaMundo.Maui.csproj` | `TargetFrameworks=net10.0-android`, `ApplicationId=com.holamundo.maui` |

### HolaMundo.Core.Tests

| Prueba | Qué valida |
|---|---|
| `Saludar_con_nombre_valido_muestra_mensaje` | flujo feliz |
| `Saludar_con_nombre_invalido_marca_error` (2 casos) | validaciones |
| `Calcular_las_cuatro_operaciones` (4 casos) | calculadora con `InvariantCulture` |
| `Dividir_entre_cero_es_error` | regla |
| `Cargar_llena_el_combo_y_al_elegir_avisa_el_mensaje` | comando async + `PropertyChanged` con un servicio falso |

## 6. Xamarin.Forms vs .NET MAUI vs Kotlin

| Tema | Xamarin.Forms (legado) | .NET MAUI (este) | Kotlin (proyecto 02) |
|---|---|---|---|
| Proyectos | uno compartido + uno por plataforma (`.Android`, `.iOS`) | **un solo proyecto** con carpeta `Platforms/` | módulos Gradle |
| Arranque | `App.xaml.cs` + `MainActivity` | `MauiProgram.CreateMauiApp()` con builder | `App.kt` + injectors |
| DI | sin contenedor estándar (Autofac, DryIoc, `DependencyService`) | `Microsoft.Extensions.DependencyInjection` integrado | Dagger 2 |
| Personalizar controles | **Renderers** | **Handlers** (más ligeros) | Views / ViewBinding |
| Namespaces XAML | `xamarin.com/schemas/2014/forms` | `schemas.microsoft.com/dotnet/2021/maui` | XML layouts |
| Layouts | `StackLayout`, `Frame` | `VerticalStackLayout`, `Grid`, `Border` | `LinearLayout`, `MaterialCardView` |
| MVVM | `INotifyPropertyChanged` a mano o MvvmLight | CommunityToolkit.Mvvm con generadores | ViewModel + LiveData |
| Recursos | imágenes por plataforma | `Resources/` único, se generan los tamaños | `res/` |
| Combobox | `Picker` | `Picker` | `Spinner` |

**Migrar una app Xamarin a MAUI** (lo que suelen pedir): usar el .NET Upgrade Assistant, cambiar namespaces `Xamarin.Forms` → `Microsoft.Maui.Controls`, pasar los Renderers a Handlers, reemplazar `DependencyService` por DI en `MauiProgram`, unir los proyectos por plataforma en `Platforms/` y actualizar los paquetes NuGet a sus versiones MAUI.

## 7. Conceptos clave

| Concepto | En una frase |
|---|---|
| MVVM | La vista solo tiene bindings; el ViewModel tiene estado y comandos; el modelo tiene las reglas |
| `INotifyPropertyChanged` | Así la vista se entera de que una propiedad cambió; el toolkit lo genera |
| `ICommand` / `RelayCommand` | El botón ejecuta un método del ViewModel sin eventos en el code-behind |
| `AsyncRelayCommand` | Comando async con `IsRunning` y cancelación |
| `ObservableCollection` | Lista que avisa altas y bajas a la vista |
| Binding de dos vías | `Entry.Text` ↔ propiedad: la vista escribe y el ViewModel también |
| Compiled bindings (`x:DataType`) | El compilador valida los bindings y son más rápidos |
| `DataTrigger` | Cambia estilos según una propiedad, sin código |
| Shell | Navegación por rutas (`Shell.Current.GoToAsync("detalle?id=1")`) |
| Ciclo de vida | `OnAppearing` / `OnDisappearing` en páginas; `Window.Created/Stopped/Resumed` en la app |
| Handlers | Mapean un control MAUI a su control nativo (`EditText` en Android) |

## 8. Paso a paso para volver a hacer el proyecto

1. `mkdir C:\HolaMundo\06-MAUI; cd C:\HolaMundo\06-MAUI`
2. `dotnet new maui -n HolaMundo.Maui -o HolaMundo.Maui` (o en Visual Studio: **.NET MAUI App**).
3. En el `.csproj`, deja `<TargetFrameworks>net10.0-android</TargetFrameworks>` si solo quieres Android.
4. `dotnet new classlib -n HolaMundo.Core -o HolaMundo.Core -f net10.0` y borra `Class1.cs`.
5. `dotnet add HolaMundo.Core package CommunityToolkit.Mvvm`
6. `dotnet new xunit -n HolaMundo.Core.Tests -o HolaMundo.Core.Tests -f net10.0`
7. Referencias: `dotnet add HolaMundo.Maui reference HolaMundo.Core` y `dotnet add HolaMundo.Core.Tests reference HolaMundo.Core`.
8. `dotnet new sln -n HolaMundoMaui` y `dotnet sln add` de los tres proyectos.
9. En Core: `Modelos.cs` → `Servicios.cs` → `MainViewModel.cs` (`partial class` que hereda de `ObservableObject`).
10. Pruebas: `MainViewModelTests.cs` → `dotnet test`.
11. En la app: registra todo en `MauiProgram.cs`, cambia `MainPage` para recibir el ViewModel por constructor y escribe el XAML con `x:DataType`.
12. `dotnet build -f net10.0-android -t:Run`.

## 9. Plantilla para una HU nueva ("despedirse")

1. `ISaludoService.Despedir` + implementación.
2. En `MainViewModel`: `[ObservableProperty] mensajeDespedida` y `[RelayCommand] Despedir()`.
3. En el XAML: `Button Command="{Binding DespedirCommand}"` y un `Label`.
4. Prueba en `MainViewModelTests`.
5. Documento de transición con las secciones 3, 4 y 5.

## 10. Problemas comunes

| Síntoma | Causa | Solución |
|---|---|---|
| `No se pudo descargar Microsoft.WindowsAppSDK… ResponseEnded` | descarga de 90+ MB cortada | reintentar `dotnet restore`, o quitar el destino Windows (como aquí) |
| `dotnet new maui` se queda pegado | la restauración automática se cuelga en esa descarga | `dotnet new maui --no-restore` y luego `dotnet restore` |
| La vista no se actualiza | propiedad sin notificación | usa `[ObservableProperty]` sobre un campo privado en minúscula, en una clase `partial` |
| `Binding: property not found` | nombre mal escrito | `x:DataType` lo convierte en error de compilación |
| `MainPage` sin constructor vacío | Shell no la encuentra en DI | regístrala con `AddTransient<MainPage>()` |
| "2,5" no se calcula | cultura del teléfono en español | el servicio usa `InvariantCulture` (punto decimal); para aceptar coma usa `CultureInfo.CurrentCulture` |
