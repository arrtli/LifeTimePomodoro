# Сборка проекта

## 1. Терминальная сборка (dotnet CLI)

```powershell
dotnet build PomodoroTimer.csproj
dotnet build PomodoroTimer.csproj -c Release
```

> **Важно:** `dotnet build` без аргументов подхватывает `PomodoroTimer.sln`, который включает `PomodoroTimer.Package.wapproj`. Этот проект требует MSBuild из состава Visual Studio и **не работает** с .NET SDK CLI. Всегда указывай `.csproj` явно.

**Артефакты:**

```
bin\Debug\net8.0-windows10.0.17763.0\
    PomodoroTimer.exe          ← исполняемый файл
    PomodoroTimer.dll
    *.deps.json, *.runtimeconfig.json
    Images\                    ← ресурсы, копируемые при сборке

bin\Release\net8.0-windows10.0.17763.0\   ← аналогично для -c Release
```

Никаких пакетов (.msix / .msixbundle) здесь **не создаётся** — только бинарники.

---

## 2. Сборка MSIX-пакета вручную

```powershell
.\Build-Msix.ps1
```

Скрипт собирает `PomodoroTimer.csproj` (Debug), упаковывает в MSIX, подписывает и устанавливает. Требует наличия SDK-инструментов (`makeappx`, `signtool`) по пути, указанному в скрипте.

**Артефакты:**

```
bin\Debug\net8.0-windows10.0.17763.0\     ← промежуточный dotnet build (см. п. 1)

AppxStaging\                              ← временный каталог упаковки (удаляется и пересоздаётся при каждом запуске)
    AppxManifest.xml
    resources.pri
    Images\
    PomodoroTimer.exe, *.dll, …

bin\MsixPackage\
    PomodoroTimer.msix                    ← готовый подписанный пакет
```

После успешной установки пакет регистрируется в системе как обычное приложение из Store.

---

## 3. Сборка из Visual Studio

Открыть `PomodoroTimer.sln` — работает штатно, включая `PomodoroTimer.Package`.

**Артефакты** зависят от выбранной конфигурации и платформы:

```
bin\{Configuration}\net8.0-windows10.0.17763.0\
    PomodoroTimer.exe          ← бинарники основного проекта (всегда)

PomodoroTimer.Package\bin\{Platform}\{Configuration}\
    AppPackages\
        PomodoroTimer.Package_{version}_{Platform}_Test\
            PomodoroTimer.Package_{version}_{Platform}.msix        ← одиночный пакет
            PomodoroTimer.Package_{version}_{Platform}.msixbundle  ← бандл (если AppxBundle=Always)
            PomodoroTimer.Package_{version}_{Platform}.cer         ← тестовый сертификат
            Add-AppDevPackage.ps1                                  ← скрипт установки
```

> `AppxBundle=Always` задан для **всех** конфигураций и платформ в `wapproj`, поэтому Visual Studio всегда создаёт `.msixbundle`.
> Одиночный `.msix` внутри папки `AppPackages` тоже присутствует — он является составной частью бандла.

---

## Справка: два файла Package.appxmanifest

В репозитории существуют **два** манифеста пакета с разным назначением:

| Файл | Кто использует | Identity Name |
|---|---|---|
| `Package.appxmanifest` (корень) | `Build-Msix.ps1` | `PomodoroTimer` |
| `PomodoroTimer.Package\Package.appxmanifest` | Visual Studio / `wapproj` | `ArtLi.LifeTimePomodoro` |

Скрипт явно читает корневой файл:

```powershell
$manifest = Get-Content "$projectDir\Package.appxmanifest" -Raw
```

Из-за разных `Identity Name` пакет, собранный скриптом, и пакет из Visual Studio — это **разные приложения** с точки зрения Windows. Они не перезаписывают друг друга при установке.

> Если нужно единообразие — синхронизируй `Identity`, `Publisher`, `DisplayName` и `PublisherDisplayName` между двумя файлами вручную.
