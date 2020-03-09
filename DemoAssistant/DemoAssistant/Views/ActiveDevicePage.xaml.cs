using ExpoHelpers;
using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DemoAssistant.Views
{
    public partial class ActiveDevicePage : ContentPage
    {
        private ActiveDevice activeDevice = null;

        public ActiveDevicePage()
        {
            InitializeComponent();
            VisualStateManager.GoToState(this.VisualStateContainer, "Inactive");
            VisualStateManager.GoToState(this.VisualStateContainer, "AppLaunchHidden");
        }

        private void ActiveDevicePageBindingContextChanged(object sender, EventArgs e)
        {
            var newActiveDevice = this.BindingContext as ActiveDevice;
            if(this.activeDevice != null && this.activeDevice != newActiveDevice)
            {
                this.activeDevice.PropertyChanged -= this.ActiveDevicePropertyChanged;
                this.activeDevice.AppLaunchStatusChanged -= this.AppLaunchStatusChanged;
            }

            this.activeDevice = newActiveDevice;

            if (this.activeDevice != null)
            {
                this.activeDevice.PropertyChanged += this.ActiveDevicePropertyChanged;
                this.activeDevice.AppLaunchStatusChanged += this.AppLaunchStatusChanged;
                this.RunningProcessesChanged();
                this.AppLaunchStatusChanged(AppLaunchStatus.None);
            }
        }

        private void ActiveDevicePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "RunningProcesses" || args.PropertyName == "Connected")
            {
                this.RunningProcessesChanged();
            }
        }

        private void RunningProcessesChanged()
        {
            string appId;
            string experienceAppPackageName;
            string trainingAppPackageName;

            DevicePortal.RunningProcesses runningProcesses = this.activeDevice.RunningProcesses;

            AppSettings.GetAppPackageInfo(AppPackageSetting.ExperienceApp, out appId, out experienceAppPackageName);
            AppSettings.GetAppPackageInfo(AppPackageSetting.TrainingApp, out appId, out trainingAppPackageName);
            string kioskModeAppPackageName = this.activeDevice.KioskModePackageName;

            bool experienceRunning = false;
            bool trainingRunning = false;
            bool kioskAppRunning = false;
            if (runningProcesses != null)
            {
                foreach (var process in runningProcesses.Processes)
                {
                    if (process.PackageFullName == experienceAppPackageName)
                    {
                        experienceRunning = true;
                    }
                    else if (process.PackageFullName == trainingAppPackageName || process.Name == AppSettings.TrainingAppExecutableName)
                    {
                        trainingRunning = true;
                    }
                    else if (process.PackageFullName == kioskModeAppPackageName)
                    {
                        kioskAppRunning = true;
                    }
                }
            }

            VisualStateManager.GoToState(this.VisualStateContainer, experienceRunning ? "ExperienceRunningState" : "ExperienceNotRunningState");
            VisualStateManager.GoToState(this.VisualStateContainer, trainingRunning ? "TrainingRunningState" : "TrainingNotRunningState");
            VisualStateManager.GoToState(this.VisualStateContainer, kioskAppRunning ? "KioskAppRunningState" : "KioskAppNotRunningState");
        }

        private async void AppLaunchStatusChanged(AppLaunchStatus status)
        {
            string newState;

            switch (status)
            {
                default:
                case AppLaunchStatus.None:
                    newState = "AppLaunchHidden";
                    break;

                case AppLaunchStatus.CommandSent:
                    newState ="AppLaunchStarted";
                    break;

                case AppLaunchStatus.Failed:
                    newState = "AppLaunchFailed";
                    break;

                case AppLaunchStatus.Succeeded:
                    newState = "AppLaunchSucceeded";
                    break;
            }

            Debug.WriteLine($"VisualStateManager.GoToState(this.VisualStateContainer, {newState});");
            ViewExtensions.CancelAnimations(this.LaunchStatusLabel);
            VisualStateManager.GoToState(this.VisualStateContainer, newState);
            this.LaunchStatusLabel.Opacity = 1.0;
            await ViewExtensions.FadeTo(this.LaunchStatusLabel, 0.0, 500);

        }

        private async void LaunchKioskAppClick(object sender, EventArgs args)
        {
            if(this.activeDevice == null)
            {
                return;
            }

            await this.activeDevice.LaunchKioskModeApplicationAsync();
        }

        private async void LaunchTrainingAppClick(object sender, EventArgs args)
        {
            await this.LaunchApp(AppPackageSetting.TrainingApp);
        }

        private async void LaunchExperienceAppClick(object sender, EventArgs args)
        {
            await this.LaunchApp(AppPackageSetting.ExperienceApp);
        }

        private async Task LaunchApp(AppPackageSetting app)
        {
            string appId;
            string packageName;

            if (app == AppPackageSetting.ExperienceApp && AppSettings.StopAllAppsBeforeExperienceLaunch)
            {
                await this.StopAllAppsImpl();
            }

            AppSettings.GetAppPackageInfo(app, out appId, out packageName);
            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(packageName) && this.activeDevice != null)
            {
                await this.activeDevice.LaunchApplicationAsync(appId, packageName);
            }
        }

        private async Task StopAllAppsImpl()
        {
            if (this.activeDevice != null)
            {
                string appId;
                string packageName;

                AppSettings.GetAppPackageInfo(AppPackageSetting.ExperienceApp, out appId, out packageName);
                if (!(string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(packageName)))
                {
                    await this.activeDevice.TerminateApplicationAsync(packageName, AppSettings.ForceTerminateExperience);
                }

                // This code below doesn't do anything - the training app currently lives in the shell space so
                // we need to kill it by it's executable name
                // Leaving it around in case the Training app ever becomes a real app
                //AppSettings.GetAppPackageInfo(AppPackageSetting.TrainingApp, out appId, out packageName);
                //if (!(string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(packageName)))
                //{
                //    await this.activeDevice.TerminateApplicationAsync(packageName, true);
                //}

                // Kill the training app by it's executable name.  There is no way currently for the
                // user to set that (we may be able to hack something)
                if (this.activeDevice.RunningProcesses.Processes != null)
                {
                    foreach (var process in this.activeDevice.RunningProcesses.Processes)
                    {
                        if (process.Name == AppSettings.TrainingAppExecutableName)
                        {
                            await this.activeDevice.TerminateProcessAsync(process.ProcessId);
                        }
                    }
                }

                // Kill the kiosk mode app nicely - dont' force it
                if (!string.IsNullOrEmpty(this.activeDevice.KioskModePackageName))
                {
                    await this.activeDevice.TerminateApplicationAsync(this.activeDevice.KioskModePackageName, false);
                }
            }
        }
    }
}