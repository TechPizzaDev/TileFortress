using GeneralShare;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace TileFortress
{
    public static class AppInfo
    {
        private static VersionTag _versionTag;
        private static AppType _type = AppType.Unspecified;

        /// <summary>
        /// Windows: AppData/Roaming/[<see cref="AppConstants.DirAppRoot"/>]
        /// </summary>
        public static DirectoryInfo RootDirectory { get; private set; }

        /// <summary>
        /// Windows: AppData/Roaming/[<see cref="AppConstants.DirAppRoot"/>]/[<see cref="AppType"/>]
        /// </summary>
        public static DirectoryInfo WorkingDirectory { get; private set; }

        /// <summary>
        /// Windows: AppData/Roaming/[<see cref="AppConstants.DirAppRoot"/>]/[<see cref="AppConstants.DirWorlds"/>]
        /// </summary>
        public static DirectoryInfo WorldsDirectory { get; private set; }

        public static string SettingsFile { get; private set; }
        
        public static void SetCultureInfo()
        {
            var clone = CultureInfo.InvariantCulture.Clone() as CultureInfo;
            clone.NumberFormat.NumberDecimalSeparator = ".";

            CultureInfo.DefaultThreadCurrentCulture = clone;
        }

        public static void SetType(AppType type)
        {
            if (type == AppType.Unspecified)
                throw new ArgumentException(nameof(type));

            if (_type != AppType.Unspecified)
                throw new InvalidOperationException("The type can only be set once.");

            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            RootDirectory = new DirectoryInfo(Path.Combine(roamingPath, AppConstants.DirAppRoot));
            RootDirectory.Create();

            WorkingDirectory = CreateRootDirectory(type.ToString());
            WorldsDirectory = CreateRootDirectory(AppConstants.DirWorlds);

            SettingsFile = Path.Combine(WorkingDirectory.FullName, AppConstants.FileSettings);
            _type = type;
        }

        private static DirectoryInfo CreateRootDirectory(string directoryName)
        {
            var dir = new DirectoryInfo(Path.Combine(RootDirectory.FullName, directoryName));
            dir.Create();
            return dir;
        }

        public static VersionTag Version
        {
            get
            {
                if (_versionTag == null)
                {
                    if(_type == AppType.Unspecified)
                        throw new InvalidOperationException();

                    try
                    {
                        var assembly = Assembly.GetEntryAssembly();
                        string root = string.Join(".", typeof(AppInfo).Namespace, _type);
                        _versionTag = VersionTag.LoadFrom(assembly, root);
                    }
                    catch (Exception exc)
                    {
                        Log.Error(exc);
                        _versionTag = VersionTag.Undefined;
                    }
                }
                return _versionTag;
            }
        }
    }
}
