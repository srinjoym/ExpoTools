// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ExpoHelpers
{
    public enum AppPackageSetting
    {
        TrainingApp,
        ExperienceApp,
    }

    public class AppSettings
    {
        private const string appPackage = "AppPackage";
        private static string AppPackageNameKey(AppPackageSetting app) { return appPackage + app.ToString() + "PackageName"; }
        private static string AppIdKey(AppPackageSetting app) { return appPackage + app.ToString() + "AppId"; }


        private static string selectedDevices = string.Empty;
        public static string SelectedDevices
        {
            get { GetLocalSetting(out selectedDevices, string.Empty); return selectedDevices; }
            set { SetLocalSetting(ref selectedDevices, value); }
        }

        private static string defaultUserName = string.Empty;
        public static string DefaultUserName
        {
            get { GetLocalSetting(out defaultUserName, string.Empty); return defaultUserName; }
            set { SetLocalSetting(ref defaultUserName, value); }
        }

        private static string defaultPassword = string.Empty;
        public static string DefaultPassword
        {
            get { GetLocalSetting(out defaultPassword, string.Empty); return defaultPassword; }
            set { SetLocalSetting(ref defaultPassword, value); }
        }

        private static string experienceDisplayName;
        public static string ExperienceDisplayName
        {
            get { GetLocalSetting(out experienceDisplayName, "Experience"); return experienceDisplayName; }
            set { SetLocalSetting(ref experienceDisplayName, value); }
        }

        private static string trainingAppExecutableName = string.Empty;
        public static string TrainingAppExecutableName
        {
            get { GetLocalSetting(out trainingAppExecutableName, "HoloLensSetup.exe"); return trainingAppExecutableName; }
            set { SetLocalSetting(ref trainingAppExecutableName, value); }
        }

        private static string enabledButtons = string.Empty;
        public static string EnabledButtons
        {
            get { GetLocalSetting(out enabledButtons, string.Empty); return enabledButtons; }
            set { SetLocalSetting(ref enabledButtons, value); }
        }

        private static int lowPowerLevel; // in percent
        public static int LowPowerLevel
        {
            get { GetLocalSetting(out lowPowerLevel, 10); return lowPowerLevel; }
            set { SetLocalSetting(ref lowPowerLevel, value); }
        }

        private static int chargedPowerLevel; // in percent
        public static int ChargedPowerLevel
        {
            get { GetLocalSetting(out chargedPowerLevel, 95); return chargedPowerLevel; }
            set { SetLocalSetting(ref chargedPowerLevel, value); }
        }

        private static bool stopAllAppsBeforeExperienceLaunch;
        public static bool StopAllAppsBeforeExperienceLaunch
        {
            get { GetLocalSetting(out stopAllAppsBeforeExperienceLaunch, false); return stopAllAppsBeforeExperienceLaunch; }
            set { SetLocalSetting(ref stopAllAppsBeforeExperienceLaunch, value); }
        }

        private static bool forceTerminateExperience;
        public static bool ForceTerminateExperience
        {
            get { GetLocalSetting(out forceTerminateExperience, false); return forceTerminateExperience; }
            set { SetLocalSetting(ref forceTerminateExperience, value); }
        }

        private static bool enablePersonCounter;
        public static bool EnablePersonCounter
        {
            get { GetLocalSetting(out enablePersonCounter, false); return enablePersonCounter; }
            set { SetLocalSetting(ref enablePersonCounter, value); }
        }

        private static int personCount;
        public static int PersonCount
        {
            get { GetLocalSetting(out personCount, 0); return personCount; }
            set { SetLocalSetting(ref personCount, value); }
        }

        public static void GetAppPackageInfo(AppPackageSetting app, out string appId, out string packageName)
        {
            GetLocalSetting(out appId, null, AppIdKey(app));
            GetLocalSetting(out packageName, null, AppPackageNameKey(app));
        }

        public static void SetAppPackageInfo(AppPackageSetting app, string appId, string packageName)
        {
            SetLocalSetting(appId, AppIdKey(app));
            SetLocalSetting(packageName, AppPackageNameKey(app));
        }

        private static void GetLocalSetting(out string storage, string defaultValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            storage = Preferences.Get(propertyName, defaultValue);
        }

        private static void GetLocalSetting(out int storage, int defaultValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            storage = Preferences.Get(propertyName, defaultValue);
        }

        private static void GetLocalSetting(out bool storage, bool defaultValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            storage = Preferences.Get(propertyName, defaultValue);
        }

        private static void SetLocalSetting(ref string storage, string newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (storage == newValue)
            {
                return;
            }

            storage = newValue;
            Preferences.Set(propertyName, newValue);
        }

        private static void SetLocalSetting(ref int storage, int newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (storage == newValue)
            {
                return;
            }

            storage = newValue;
            Preferences.Set(propertyName, newValue);
        }

        private static void SetLocalSetting(ref bool storage, bool newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (storage == newValue)
            {
                return;
            }

            storage = newValue;
            Preferences.Set(propertyName, newValue);
        }

        private static void SetLocalSetting(string newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            Preferences.Set(propertyName, newValue);
        }

        private static void ClearLocalSetting([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            Preferences.Remove(propertyName);
        }

    }
}
