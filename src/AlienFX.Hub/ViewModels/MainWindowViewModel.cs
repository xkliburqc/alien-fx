using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using AlienFX.Devices;
using AlienFX.Hub.Models;
using AlienFX.Hub.Shared;
using AlienFX.Invoke;
using Microsoft.Win32;

namespace AlienFX.Hub.ViewModels;
public class MainWindowViewModel
{
    private IntPtr _handle;
    private HwndSource? _source;
    private List<AFXDevice> _afxDevices = new();
    private AlienFXState _state = new();

    public static ICommand ChangeBrightnessCommand
    {
        get
        {
            return new DelegateCommand
            {
                CanExecuteFunc = () => true,
                CommandAction = () =>
                {
                    List<Devices.AFXDevice> devices = AFX.FindDevices();
                    devices.ForEach(x => x.SetBrightness(255, true));
                }
            };
        }
    }

    public void Initialize()
    {
        WindowInteropHelper helper = new(Application.Current.MainWindow);
        _handle = helper.EnsureHandle();
        _source = HwndSource.FromHwnd(_handle);
        _source.AddHook(WindowsMessageHook);

        SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);

        _afxDevices = AFX.FindDevices();
        InitializeAlienFX();
    }

    private void InitializeAlienFX()
    {
        _ = UpdatePowerSettings();
        UpdateLights();
    }

    private void UpdateLights()
    {
        if (_state.IsAlienFXOn)
        {
            _afxDevices.ForEach(x => x.SetBrightness(_state.Brightness, true));
        }
        else
        {
            _afxDevices.ForEach(x => x.SetBrightness(0, true));
        }
        //
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
        else if (_state.Brightness == 255)
        {
            _state.Brightness = 128;
        }
        else
        {
            _state.Brightness = 0;
        }
    }

    public void RegisterHotKeys()
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

    private void SystemEvents_PowerModeChanged(object sender,
                          PowerModeChangedEventArgs e)
    {
        if (UpdatePowerSettings())
        {
            UpdateLights();
        }
    }
}
