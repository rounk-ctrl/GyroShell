﻿#region Copyright (BSD 3-Clause License)
/*
 * GyroShell - A modern, extensible, fast, and customizable shell platform.
 * Copyright 2022-2024 Pdawg
 *
 * Licensed under the BSD 3-Clause License.
 * SPDX-License-Identifier: BSD-3-Clause
 */
#endregion

using GyroShell.Library.Events;
using GyroShell.Library.Services.Managers;
using System;
using System.Runtime.InteropServices;
using static GyroShell.Library.Helpers.Win32.Win32Interop;

namespace GyroShell.Services.Managers
{
    internal class ExplorerManagerService : IExplorerManagerService
    {
        private const uint EVENT_MODIFY_STATE = 0x0002;

        public IntPtr m_hTaskBar { get; set; }
        public IntPtr m_hMultiTaskBar { get; set; }
        public IntPtr m_hStartMenu { get; set; }

        public void Initialize()
        {
            m_hTaskBar = FindWindow("Shell_TrayWnd", null);
            m_hMultiTaskBar = FindWindow("Shell_SecondaryTrayWnd", null);
            m_hStartMenu = FindWindowEx(m_hStartMenu, IntPtr.Zero, "Button", "Start");

            if (m_hStartMenu == IntPtr.Zero)
            {
                m_hStartMenu = FindWindow("Button", null);
            }
        }

        public event EventHandler<SystemTaskbarControlChangedEventArgs> SystemControlStateChanged;

        private bool _isStartMenuOpen;
        public bool IsStartMenuOpen
        {
            get => _isStartMenuOpen;
            set
            {
                _isStartMenuOpen = value;
                SystemControlStateChanged?.Invoke(this, new SystemTaskbarControlChangedEventArgs(SystemTaskbarControlChangedEventArgs.SystemControlChangedType.Start, value));
            }
        }

        private bool _isActionCenterOpen;
        public bool IsActionCenterOpen
        {
            get => _isActionCenterOpen;
            set
            {
                _isActionCenterOpen = value;
                SystemControlStateChanged?.Invoke(this, new SystemTaskbarControlChangedEventArgs(SystemTaskbarControlChangedEventArgs.SystemControlChangedType.ActionCenter, value));
            }
        }

        private bool _isSystemControlsOpen;
        public bool IsSystemControlsOpen
        {
            get => _isSystemControlsOpen;
            set
            {
                _isActionCenterOpen = value;
                SystemControlStateChanged?.Invoke(this, new SystemTaskbarControlChangedEventArgs(SystemTaskbarControlChangedEventArgs.SystemControlChangedType.SystemControls, value));
            }
        }


        public void ShowTaskbar()
        {
            SetVisibility(true);
        }

        public void HideTaskbar()
        {
            SetVisibility(false);
        }

        public void ToggleStartMenu()
        {
            SendMessage(m_hTaskBar, /*WM_SYSCOMMAND*/ 0x0112, (IntPtr) /*SC_TASKLIST*/ 0xF130, (IntPtr)0);
        }

        public void ToggleControlCenter()
        {
            ShellExecute(IntPtr.Zero, "open", "ms-actioncenter:controlcenter/&suppressAnimations=false&showFooter=true&allowPageNavigation=true" /* CNTRLCTR, bool, bool, bool */, null, null, 1);
        }

        public void ToggleActionCenter()
        {
            ShellExecute(IntPtr.Zero, "open", "ms-actioncenter:" /* ACTION CENTER */, null, null, 1);
        }

        public void ToggleAutoHideExplorer(bool doHide)
        {
            if (doHide)
            {
                // MainBar
                APPBARDATA abd = new APPBARDATA();

                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = m_hTaskBar;
                abd.lParam = (IntPtr)ABState.ABS_AUTOHIDE;

                SHAppBarMessage((int)ABMsg.ABM_SETSTATE, ref abd);
            }
            else
            {
                // MainBar
                APPBARDATA abd = new APPBARDATA();

                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = m_hTaskBar;
                abd.lParam = (IntPtr)ABState.ABS_TOP;

                SHAppBarMessage((int)ABMsg.ABM_SETSTATE, ref abd);

                // MultiBar
                if (m_hMultiTaskBar != IntPtr.Zero)
                {
                    APPBARDATA abdM = new APPBARDATA();

                    abd.cbSize = Marshal.SizeOf(abdM);
                    abd.hWnd = m_hMultiTaskBar;
                    abd.lParam = (IntPtr)ABState.ABS_TOP;

                    SHAppBarMessage((int)ABMsg.ABM_SETSTATE, ref abdM);
                }
            }
        }

        public void NotifyWinlogonShowShell()
        {
            IntPtr handle = OpenEvent(EVENT_MODIFY_STATE, false, "msgina: ShellReadyEvent");

            if (handle != IntPtr.Zero)
            {
                SetEvent(handle);
                CloseHandle(handle);
            }
        }

        private void SetVisibility(bool isVisible)
        {
            if (!isVisible)
            {
                SetWindowPos(m_hTaskBar, (IntPtr)WindowZOrder.HWND_BOTTOM, 0, 0, 0, 0, (int)SWPFlags.SWP_HIDEWINDOW | (int)SWPFlags.SWP_NOMOVE | (int)SWPFlags.SWP_NOSIZE | (int)SWPFlags.SWP_NOACTIVATE);
                SetWindowPos(m_hMultiTaskBar, (IntPtr)WindowZOrder.HWND_BOTTOM, 0, 0, 0, 0, (int)SWPFlags.SWP_HIDEWINDOW | (int)SWPFlags.SWP_NOMOVE | (int)SWPFlags.SWP_NOSIZE | (int)SWPFlags.SWP_NOACTIVATE);
            }
            else
            {
                SetWindowPos(m_hTaskBar, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 48, 0, 0, (int)SWPFlags.SWP_SHOWWINDOW);
                SetWindowPos(m_hMultiTaskBar, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 48, 0, 0, (int)SWPFlags.SWP_SHOWWINDOW);
            }
        }
    }
}