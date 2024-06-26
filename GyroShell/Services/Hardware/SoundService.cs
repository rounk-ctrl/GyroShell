﻿#region Copyright (BSD 3-Clause License)
/*
 * GyroShell - A modern, extensible, fast, and customizable shell platform.
 * Copyright 2022-2024 Pdawg
 *
 * Licensed under the BSD 3-Clause License.
 * SPDX-License-Identifier: BSD-3-Clause
 */
#endregion

using CoreAudio;
using GyroShell.Library.Services.Hardware;
using System;

namespace GyroShell.Services.Hardware
{
    internal class SoundService : ISoundService
    {
        private MMDevice _audioDevice;
        private MMDeviceEnumerator _deviceEnumerator;

        public int Volume 
        {
            get => Math.Clamp((int)(_audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100), 0, 100);
            set => _audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(value / 100f, 0f, 1f);
        }
        public bool IsMuted 
        {
            get => _audioDevice.AudioEndpointVolume.Mute;
            set => _audioDevice.AudioEndpointVolume.Mute = value;
        }

        public event EventHandler OnVolumeChanged;

        public SoundService()
        {
            _deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            _audioDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            _audioDevice.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            OnVolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
