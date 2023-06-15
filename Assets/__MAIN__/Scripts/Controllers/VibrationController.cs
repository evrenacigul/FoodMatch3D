using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;
using Utilities;
using Behaviours;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Controllers
{
    public class VibrationController : SingletonMonoBehaviour<VibrationController>
    {
        bool vibrationActive;

        public bool SetVibrationActive { get { return vibrationActive; } set { vibrationActive = value; } }

        private void Start()
        {
            Vibration.Init();

            vibrationActive = true;
        }

        public void Vibrate(int milliseconds)
        {
            if (!vibrationActive) return;
            Vibration.Vibrate(milliseconds);
        }

        public void VibrateNope()
        {
            if (!vibrationActive) return;
            Vibration.VibrateNope();
        }

        public void VibratePop()
        {
            if (!vibrationActive) return;
            Vibration.VibratePop();
        }
    }
}