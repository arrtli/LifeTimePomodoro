using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;

namespace PomodoroTimer;

public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private MainWindow? _mainWindow;

    internal AppSettings Settings { get; private set; } = AppSettings.Load();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        _trayIcon.Icon = CreateTrayIcon();
        _trayIcon.TrayMouseDoubleClick += (_, _) => ShowMainWindow();

        var menu = new ContextMenu();

        var showItem = new MenuItem { Header = "Показать" };
        showItem.Click += (_, _) => ShowMainWindow();
        menu.Items.Add(showItem);

        var settingsItem = new MenuItem { Header = "Настройки" };
        settingsItem.Click += (_, _) => ShowSettings();
        menu.Items.Add(settingsItem);

        menu.Items.Add(new Separator());

        var exitItem = new MenuItem { Header = "Выход" };
        exitItem.Click += (_, _) => { _trayIcon.Dispose(); Shutdown(); };
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenu = menu;

        // Apply autostart setting on first run
        ApplyAutostart(Settings.StartWithWindows);

        _mainWindow = new MainWindow();
        _mainWindow.Show();
    }

    internal void ShowMainWindow()
    {
        if (_mainWindow == null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    internal void ShowSettings()
    {
        var dlg = new SettingsWindow(Settings);
        dlg.Owner = _mainWindow;
        if (dlg.ShowDialog() == true && dlg.Result != null)
        {
            Settings = dlg.Result;
            Settings.Save();
            ApplyAutostart(Settings.StartWithWindows);
        }
    }

    internal static void ApplyAutostart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: true);
            if (key == null) return;

            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
            if (enable)
                key.SetValue("PomodoroTimer", $"\"{exePath}\"");
            else
                key.DeleteValue("PomodoroTimer", throwOnMissingValue: false);
        }
        catch { /* ignore registry errors */ }
    }

    private static System.Drawing.Icon CreateTrayIcon()
    {
        var bmp = new System.Drawing.Bitmap(32, 32);
        using var g = System.Drawing.Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(System.Drawing.Color.Transparent);

        // Red circle background
        using var bgBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(220, 53, 69));
        g.FillEllipse(bgBrush, 1, 1, 30, 30);

        // White "P" letter
        using var font = new System.Drawing.Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold);
        using var fBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
        var sf = new System.Drawing.StringFormat
        {
            Alignment = System.Drawing.StringAlignment.Center,
            LineAlignment = System.Drawing.StringAlignment.Center
        };
        g.DrawString("P", font, fBrush, new System.Drawing.RectangleF(0, 0, 32, 32), sf);

        return System.Drawing.Icon.FromHandle(bmp.GetHicon());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}


