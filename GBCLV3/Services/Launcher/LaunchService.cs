using System;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using GBCLV3.Models.Launcher;
using GBCLV3.Utils;
using StyletIoC;
using Version = GBCLV3.Models.Launcher.Version;

namespace GBCLV3.Services.Launcher
{
    class LaunchService
    {
        #region Events

        public event Action<string> LogReceived;

        public event Action<string> ErrorReceived;

        public event Action<int> Exited;

        #endregion

        #region Private Members

        private Process _gameProcess;

        // IoC
        private readonly GamePathService _gamePathService;

        #endregion

        #region Constructor

        [Inject]
        public LaunchService(GamePathService gamePathService)
        {
            _gamePathService = gamePathService;
        }

        #endregion

        #region Public Methods

        public async Task<LaunchResult> LaunchGameAsync(LaunchProfile profile, Version version)
        {
            bool isDebugMode = _gamePathService.JreExecutablePath.EndsWith("java.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = _gamePathService.JreExecutablePath,
                WorkingDirectory = _gamePathService.WorkingDir,
                Arguments = BuildArguments(profile, version),
                UseShellExecute = isDebugMode,
                RedirectStandardOutput = !isDebugMode,
                RedirectStandardError = !isDebugMode,
            };

            _gameProcess = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            _gameProcess.Start();
            _gameProcess.Exited += (s, e) => Exited?.Invoke(_gameProcess.ExitCode);

            if (!isDebugMode)
            {
                _gameProcess.ErrorDataReceived += (s, e) => ErrorReceived?.Invoke(e.Data);
                _gameProcess.BeginErrorReadLine();

                _gameProcess.OutputDataReceived += (s, e) => LogReceived?.Invoke(e.Data);
                _gameProcess.BeginOutputReadLine();

                await Task.Run(() => _gameProcess.WaitForInputIdle());
            }

            return new LaunchResult { IsSuccessful = true };
        }

        #endregion

        #region Private Methods

        private string BuildArguments(LaunchProfile profile, Version version)
        {
            StringBuilder builder = new StringBuilder(8192);

            // User defined JVM arguments
            if (!string.IsNullOrWhiteSpace(profile.JvmArgs))
            {
                builder.Append(profile.JvmArgs).Append(' ');
            }
            else
            {
                // Configure GC
                builder.Append("-XX:+UnlockExperimentalVMOptions ");
                builder.Append("-XX:+UseG1GC ");
                builder.Append("-XX:G1NewSizePercent=20 ");
                builder.Append("-XX:G1ReservePercent=20 ");
                builder.Append("-XX:MaxGCPauseMillis=48 ");
                builder.Append("-XX:+ParallelRefProcEnabled ");
                builder.Append("-XX:G1HeapRegionSize=32M ");
                builder.Append("-XX:-UseAdaptiveSizePolicy ");
                builder.Append("-XX:-OmitStackTraceInFastThrow ");
            }

            // Max Memory
            builder.Append($"-Xmx{profile.MaxMemory}M ");

            // Arguments for Forge
            builder.Append("-Dfml.ignoreInvalidMinecraftCertificates=true ");
            builder.Append("-Dfml.ignorePatchDiscrepancies=true ");

            // WHAT THE HELL is this ???
            builder.Append("-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump ");

            // Natives
            builder.Append($"-Djava.library.path=\"{_gamePathService.NativeDir}\" ");

            // Launcher Identifier
            builder.Append($"-Dminecraft.launcher.brand={AssemblyUtil.Title} ");
            builder.Append($"-Dminecraft.launcher.version={AssemblyUtil.Version} ");

            // Libraries
            builder.Append("-cp ");
            foreach (var lib in version.Libraries)
            {
                if (lib.Type == LibraryType.Native) continue;
                builder.Append($"{_gamePathService.LibDir}/{lib.Path};");
            }

            // Main Jar
            builder.Append($"{_gamePathService.VersionDir}/{version.JarID}/{version.JarID}.jar ");

            // Main Class
            builder.Append(version.MainClass).Append(' ');

            // Minecraft Arguments
            var argsDict = version.MinecarftArgsDict;

            argsDict["--username"] = profile.Username;
            argsDict["--version"] = version.ID;
            argsDict["--gameDir"] = _gamePathService.WorkingDir;

            if (version.AssetsInfo.IsLegacy)
            {
                argsDict["--assetsDir"] = _gamePathService.AssetDir + "/virtual/legacy";
            }
            else
            {
                argsDict["--assetsDir"] = _gamePathService.AssetDir;
                argsDict["--assetIndex"] = version.AssetsInfo.ID;
            }

            if (argsDict.ContainsKey("--uuid"))  argsDict["--uuid"] = profile.UUID;
            if (argsDict.ContainsKey("--accessToken")) argsDict["--accessToken"] = profile.AccessToken;
            if (argsDict.ContainsKey("--session")) argsDict["--session"] = profile.AccessToken;
            if (argsDict.ContainsKey("--userType")) argsDict["--userType"] = profile.UserType;
            if (argsDict.ContainsKey("--versionType")) argsDict["--versionType"] = profile.VersionType;
            if (argsDict.ContainsKey("--userProperties")) argsDict["--userProperties"] = "{}";

            var args = string.Join(" ", argsDict.Select(pair => pair.Key + ' ' + pair.Value));
            builder.Append(args).Append(' ');

            // Server Login
            if (!string.IsNullOrWhiteSpace(profile.ServerAddress))
            {
                string[] temp = profile.ServerAddress.Split(':');
                builder.Append("--server " + temp[0]).Append(' ');

                if (temp.Length == 2)
                {
                    builder.Append("--port " + temp[1]).Append(' ');
                }
            }

            // Full Screen
            if (profile.IsFullScreen)
            {
                builder.Append("--fullscreen ");
            }

            // Window Size
            if (profile.WinWidth != 0 && profile.WinHeight != 0)
            {
                builder.Append($"--width {profile.WinWidth} --height {profile.WinHeight}");
            }

            // Additional Arguments
            if (!string.IsNullOrWhiteSpace(profile.ExtraArgs))
            {
                builder.Append(' ').Append(profile.ExtraArgs);
            }

            Debug.WriteLine(builder.ToString());

            // Build Complete
            return builder.ToString();
        }

        #endregion
    }
}
