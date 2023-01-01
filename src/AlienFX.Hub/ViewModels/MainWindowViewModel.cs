using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using AlienFX.Devices;
using AlienFX.Hub.Models;
using AlienFX.Invoke;
using Microsoft.Win32;

namespace AlienFX.Hub.ViewModels;

/// <summary>
/// Class <c>MainWindowViewModel</c> models the main window <c>ViewModel</c>.
/// </summary>
public class MainWindowViewModel
{
    private IntPtr _handle;
    private HwndSource? _source;
    private List<AFXDevice> _afxDevices = new();
    private readonly AlienFXState _state = new();

    /// <summary>Gets the application title.</summary>
    public static string Title => "AlienFX Hub";
    /// <summary>Gets the application description.</summary>
    public static string Description => "This application controls AlienFX compatible lights";
    /// <summary>Gets the application version.</summary>
    public static string Version 
    {
        get
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);

            return fileVersion.FileVersion!;
        }
    }

    /// <summary>
    /// This method initialize the main window and attach window messages hook.
    /// </summary>
    public void Initialize()
    {
        WindowInteropHelper helper = new(Application.Current.MainWindow);

        _handle = helper.EnsureHandle();
        _source = HwndSource.FromHwnd(_handle);
        _source.AddHook(WindowsMessageHook);

        SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);

        _afxDevices = AFX.FindDevices();

        InitializeAlienFX();

        RegisterHotKeys();
    }

    private void InitializeAlienFX()
    {
        _ = UpdatePowerSettings();

        UpdateLights();
    }

    private void UpdateLights() //TODO: Need to support a mapping
    {
        byte brightness = ((byte)(_state.IsAlienFXOn ? _state.Brightness : 0));

        _afxDevices.ForEach(x => x.SetBrightness(brightness));
    }

    private bool UpdatePowerSettings()
    {
        bool powerSettingsHasUpdated = false;

        SystemPowerStatus sps = AFX.GetSystemPowerStatus();
        bool isAlienFXOn = AFX.GetAlienFXPowerSetting();
        _state.AcLineStatus = sps.ACLineStatus;

        if (isAlienFXOn != _state.IsAlienFXOn)
        {
            _state.IsAlienFXOn = isAlienFXOn;
            powerSettingsHasUpdated = true;
        }

        return powerSettingsHasUpdated;
    }

    private void ToggleBrightness()
    {
        if (_state.Brightness == 0)
        {
            _state.Brightness = 255;
        }
        else
        {
            _state.Brightness = 0;
        }
    }

    private void RegisterHotKeys()
    {
        AFX.RegisterHotKey(_handle, User32.HOTKEY_ID, 0, User32.VK_F18);
    }

    private IntPtr WindowsMessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        switch (msg)
        {
            case WM_HOTKEY:
                switch (wParam.ToInt32())
                {
                    case User32.HOTKEY_ID:
                        int vkey = (((int)lParam >> 16) & 0xFFFF);
                        if (vkey == User32.VK_F18)
                        {
                            bool powerSettingsHasUpdated = UpdatePowerSettings();

                            if (_state.IsAlienFXOn)
                            {
                                ToggleBrightness();
                            }

                            if (_state.IsAlienFXOn || powerSettingsHasUpdated)
                            {
                                UpdateLights();
                            }
                        }
                        handled = true;
                        break;
                }
                break;
        }
        return IntPtr.Zero;
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (UpdatePowerSettings())
        {
            UpdateLights();
        }
    }
}
