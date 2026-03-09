using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PomodoroTimer;

public partial class MainWindow : Window
{
    // ─── Timer state ───────────────────────────────────────────────
    private enum TimerState { Idle, Running, Paused }
    private TimerState _state = TimerState.Idle;

    private int _minutesSet = 0;        // Minutes configured by the user
    private int _secondsRemaining = 0;  // Seconds remaining during countdown

    private readonly DispatcherTimer _timer;

    // ─── Constructor ───────────────────────────────────────────────
    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        UpdateDisplay();
        UpdateButtons();
    }

    // ─── Timer tick ────────────────────────────────────────────────
    private void Timer_Tick(object? sender, EventArgs e)
    {
        _secondsRemaining--;

        if (_secondsRemaining <= 0)
        {
            _secondsRemaining = 0;
            _timer.Stop();
            _state = TimerState.Idle;
            _minutesSet = 0;

            UpdateDisplay();
            UpdateButtons();
            PlayCompletionSound();
        }
        else
        {
            UpdateDisplay();
        }
    }

    // ─── Display ───────────────────────────────────────────────────
    private void UpdateDisplay()
    {
        if (_state == TimerState.Idle)
            TbDisplay.Text = $"{_minutesSet:D2}:00";
        else
            TbDisplay.Text = $"{_secondsRemaining / 60:D2}:{_secondsRemaining % 60:D2}";
    }

    // ─── Button states ────────────────────────────────────────────
    private void UpdateButtons()
    {
        bool idle    = _state == TimerState.Idle;
        bool running = _state == TimerState.Running;
        bool paused  = _state == TimerState.Paused;

        BtnStart.IsEnabled  = (idle && _minutesSet > 0) || paused;
        BtnStop.IsEnabled   = running;
        BtnReset.IsEnabled  = !idle || _minutesSet > 0;
        BtnMinUp.IsEnabled  = idle;
        BtnMinDown.IsEnabled = idle;

        // Update title bar with remaining time when running/paused
        Title = running || paused
            ? $"Pomodoro Timer — {TbDisplay.Text}"
            : "Pomodoro Timer";
    }

    // ─── Minute increment / decrement ─────────────────────────────
    private void BtnMinUp_Click(object sender, RoutedEventArgs e)
    {
        if (_state != TimerState.Idle) return;
        if (_minutesSet < 99) _minutesSet++;
        UpdateDisplay();
        UpdateButtons();
    }

    private void BtnMinDown_Click(object sender, RoutedEventArgs e)
    {
        if (_state != TimerState.Idle) return;
        if (_minutesSet > 0) _minutesSet--;
        UpdateDisplay();
        UpdateButtons();
    }

    // ─── Preset buttons ───────────────────────────────────────────
    private void BtnPreset_Click(object sender, RoutedEventArgs e)
    {
        if (_state != TimerState.Idle) return;
        if (sender is System.Windows.Controls.Button btn &&
            int.TryParse(btn.Tag?.ToString(), out int minutes))
        {
            _minutesSet = minutes;
            UpdateDisplay();
            UpdateButtons();
        }
    }

    // ─── Start ────────────────────────────────────────────────────
    private void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        if (_state == TimerState.Idle)
        {
            if (_minutesSet <= 0) return;
            _secondsRemaining = _minutesSet * 60;
        }
        // If Paused — just resume with current _secondsRemaining

        _state = TimerState.Running;
        _timer.Start();
        UpdateDisplay();
        UpdateButtons();
    }

    // ─── Stop (pause) ─────────────────────────────────────────────
    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        if (_state != TimerState.Running) return;
        _timer.Stop();
        _state = TimerState.Paused;
        UpdateButtons();
    }

    // ─── Reset ────────────────────────────────────────────────────
    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _state = TimerState.Idle;
        _minutesSet = 0;
        _secondsRemaining = 0;
        UpdateDisplay();
        UpdateButtons();
    }

    // ─── Settings ─────────────────────────────────────────────────
    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        ((App)Application.Current).ShowSettings();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnHide_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    // ─── Sound ────────────────────────────────────────────────────
    private void PlayCompletionSound()
    {
        var soundPath = ((App)Application.Current).Settings.SelectedSoundPath;
        try
        {
            if (!string.IsNullOrWhiteSpace(soundPath) && File.Exists(soundPath))
            {
                var player = new SoundPlayer(soundPath);
                player.Play();
                return;
            }
        }
        catch { /* fall through to default */ }

        SystemSounds.Exclamation.Play();
    }

    // ─── Close → hide to tray ─────────────────────────────────────
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
