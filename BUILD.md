# Сборка проекта

## Терминальная сборка (dotnet CLI)

```powershell
dotnet build PomodoroTimer.csproj
```

> **Важно:** `dotnet build` без аргументов подхватывает `PomodoroTimer.sln`, который включает `PomodoroTimer.Package.wapproj`. Этот проект требует MSBuild из состава Visual Studio и **не работает** с .NET SDK CLI. Всегда указывай `.csproj` явно.

## Сборка MSIX-пакета вручную

```powershell
.\Build-Msix.ps1
```

Скрипт собирает `PomodoroTimer.csproj`, упаковывает в MSIX, подписывает и устанавливает. Требует наличия SDK-инструментов (`makeappx`, `signtool`) по пути, указанному в скрипте.

## Сборка из Visual Studio

Открыть `PomodoroTimer.sln` — работает штатно, включая `PomodoroTimer.Package`.
