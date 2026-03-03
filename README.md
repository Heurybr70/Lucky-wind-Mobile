# Participa y Gana — Xamarin.Forms Android App

Aplicación móvil para registrar y gestionar sorteos, con autenticación Firebase y almacenamiento en Cloud Firestore.

---

## Requisitos de la PC de desarrollo

| Herramienta | Versión mínima | Notas |
|---|---|---|
| Windows | 10 / 11 (64-bit) | |
| Visual Studio | 2022 (v17+) | Community, Professional o Enterprise |
| Carga de trabajo VS | **Desarrollo móvil con .NET** | Incluye Xamarin y Android SDK |
| Android SDK | API 33 (Android 13) | Se instala desde el SDK Manager de VS |
| Android NDK | Cualquiera reciente | Incluido con la carga de trabajo |
| JDK | 11 | VS lo instala automáticamente |
| Git | Cualquiera | Para clonar el repositorio |

---

## 1. Clonar el repositorio

```bash
git clone <URL-del-repositorio>
cd Lucky-wind-Mobile
```

---

## 2. Configurar Visual Studio

### Instalar la carga de trabajo correcta

1. Abrir **Visual Studio Installer**
2. Hacer clic en **Modificar** sobre Visual Studio 2022
3. Marcar **Desarrollo móvil con .NET**
4. Hacer clic en **Modificar** y esperar la instalación

### Instalar la plataforma Android necesaria

1. En Visual Studio: **Herramientas → Administrador de Android SDK**
2. Pestaña **Plataformas**: instalar **Android 13.0 (API 33)**
3. Pestaña **Herramientas**: verificar que estén instalados:
   - Android SDK Build-Tools 33+
   - Android SDK Platform-Tools
   - Android Emulator (si no se tiene dispositivo físico)

---

## 3. Abrir la solución

1. Abrir Visual Studio 2022
2. **Archivo → Abrir → Proyecto o solución**
3. Navegar a `Lucky-wind-Mobile\Lucky-wind\` y abrir `Lucky-wind.sln`
4. Esperar a que NuGet restaure los paquetes automáticamente (barra de estado inferior)

Si los paquetes no se restauran solos:
- Clic derecho sobre la solución en el Explorador de soluciones
- **Restaurar paquetes NuGet**

---

## 4. Paquetes NuGet del proyecto

Los paquetes se restauran automáticamente. Para referencia:

| Paquete | Versión |
|---|---|
| Xamarin.Forms | 5.0.0.2196 |
| Xamarin.Essentials | 1.7.0 |
| Newtonsoft.Json | 13.0.3 |
| Xamarin.GooglePlayServices.Auth | 117.0.1.5 |

---

## 5. Configurar Firebase (obligatorio)

El archivo `google-services.json` ya está incluido en `Lucky-wind.Android\`. Contiene las credenciales del proyecto Firebase `lucky-wind-5fdfb`.

### 5.1 Verificar que Firestore está activo

1. Ir a [Firebase Console](https://console.firebase.google.com)
2. Seleccionar el proyecto **lucky-wind-5fdfb**
3. Menú lateral → **Firestore Database**
4. Si no está creado: clic en **Crear base de datos** → modo producción → elegir región

### 5.2 Configurar reglas de seguridad de Firestore

En Firebase Console → **Firestore Database → Reglas**, pegar exactamente:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /users/{userId}/raffles/{raffleId} {
      allow read, write: if request.auth != null
                         && request.auth.uid == userId;
    }
  }
}
```

Clic en **Publicar**.

### 5.3 Google Sign-In — SHA-1 del keystore de la nueva PC

Google Sign-In requiere registrar el SHA-1 del keystore con el que se firma el APK en esa PC.

**Obtener el SHA-1 del debug keystore:**

```powershell
keytool -list -v -keystore "$env:USERPROFILE\.android\debug.keystore" -alias androiddebugkey -storepass android -keypass android
```

Copiar el valor **SHA1** del resultado (formato `AA:BB:CC:...`).

**Registrarlo en Firebase:**

1. Firebase Console → **Configuración del proyecto** (ícono ⚙️)
2. Pestaña **General** → sección **Tus aplicaciones** → app Android (`lucky_win.com`)
3. Clic en **Agregar huella digital** → pegar el SHA-1 → **Guardar**
4. Descargar el nuevo `google-services.json` y reemplazar el existente en `Lucky-wind.Android\`

> ⚠️ Sin este paso, el botón **Iniciar sesión con Google** devuelve token `null` y no funciona. El login con email/contraseña sí funciona sin SHA-1.

---

## 6. Configurar dispositivo o emulador

### Opción A — Dispositivo físico Android

1. En el teléfono: **Ajustes → Acerca del teléfono** → tocar **Número de compilación** 7 veces
2. Ir a **Ajustes → Opciones de desarrollador** → activar **Depuración USB**
3. Conectar el teléfono con cable USB
4. Aceptar el diálogo de confianza en el teléfono
5. En Visual Studio, el dispositivo aparece en el selector de destino (barra superior)

### Opción B — Emulador Android

1. En Visual Studio: **Herramientas → Administrador de dispositivos Android**
2. Clic en **+ Nuevo** → seleccionar un dispositivo (ej. Pixel 5) con API 33
3. Clic en **Crear** y esperar la descarga de la imagen del sistema
4. Iniciar el emulador desde el Administrador antes de compilar

---

## 7. Compilar y ejecutar

1. En la barra superior de VS, seleccionar:
   - Configuración: **Debug**
   - Proyecto de inicio: **Lucky-wind.Android**
   - Destino: el dispositivo o emulador
2. Presionar **F5** o el botón ▶ verde

> La primera compilación tarda varios minutos porque enlaza las bibliotecas de Xamarin.

---

## 8. Estructura del proyecto

```
Lucky-wind-Mobile/
├── Lucky-wind/
│   ├── Lucky-wind.sln                    ← Abrir esto en Visual Studio
│   ├── Lucky-wind/                       ← Proyecto compartido (lógica y UI)
│   │   ├── Models/                       ← RaffleModel, UserModel
│   │   ├── Services/                     ← AuthService, RaffleService, ThemeService
│   │   ├── ViewModels/                   ← MVVM: Login, Register, Dashboard, etc.
│   │   ├── Views/                        ← Páginas XAML
│   │   └── Converters/                   ← InverseBoolConverter, BoolToColorConverter
│   └── Lucky-wind.Android/              ← Proyecto Android
│       ├── google-services.json         ← Credenciales Firebase (NO subir a repositorio público)
│       ├── Resources/
│       │   ├── drawable/                ← ic_launcher_foreground.xml (vector del ícono)
│       │   └── mipmap-*/               ← Íconos PNG en todas las densidades
│       └── Properties/
│           └── AndroidManifest.xml      ← Package: lucky_win.com, API 21–33
├── .editorconfig                        ← Fuerza UTF-8 sin BOM (crítico para aapt2)
└── README.md
```

---

## 9. Credenciales del proyecto (referencia)

| Dato | Valor |
|---|---|
| Firebase Project ID | `lucky-wind-5fdfb` |
| Android Package Name | `lucky_win.com` |
| API Key (Web) | `AIzaSyC0HVIzBngcIFjexzLgLwYAvyS0i1nnwdU` |
| Web Client ID (Google) | `542081571847-f7ig9810dsjr0fssp1jeuutk5n0sdn0k.apps.googleusercontent.com` |
| Min SDK | Android 5.0 (API 21) |
| Target SDK | Android 13 (API 33) |

---

## 10. Errores conocidos y soluciones

### `resource drawable/ic_launcher_foreground not found` al compilar

Causas en orden de frecuencia:

| Causa | Solución |
|---|---|
| La entrada fue eliminada del `.csproj` | Verificar que exista `<AndroidResource Include="Resources\drawable\ic_launcher_foreground.xml" />` en `Lucky-wind.Android.csproj` |
| El archivo XML tiene UTF-8 BOM | Verificar que VS Code no guarde el archivo con BOM; el `.editorconfig` lo previene |
| Caché de compilación corrupta | **Build → Clean Solution** → **Build → Rebuild Solution** |

### `TargetInvocationException` al guardar un sorteo

Causado por usar `.ConfigureAwait(false)` en un ViewModel antes de llamar `DisplayAlert` o `PopAsync`. Ya corregido — los ViewModels no deben usar `.ConfigureAwait(false)`.

### Google Sign-In devuelve token `null`

El SHA-1 del debug keystore de esta PC no está registrado en Firebase Console. Seguir el paso 5.3.

### `401 UNAUTHENTICATED` en Firestore tras 1 hora de uso

El IdToken de Firebase expira en 1 hora. El proyecto incluye `AuthService.RefreshTokenIfNeededAsync()` que se llama automáticamente antes de cada operación Firestore.

---

## 11. Notas importantes para el editor (VS Code + .editorconfig)

El archivo `.editorconfig` en la raíz del repositorio configura automáticamente UTF-8 **sin BOM** para todos los archivos `.xml`. Esto es crítico porque:

- aapt2 (el compilador de recursos Android) **rechaza** archivos XML que empiecen con un BOM (`EF BB BF`)
- PowerShell 5.1 y versiones antiguas de VS Code agregan BOM al guardar archivos `.xml`
- El `.editorconfig` previene este problema en cualquier editor que lo soporte (VS Code, Visual Studio, Rider, etc.)
