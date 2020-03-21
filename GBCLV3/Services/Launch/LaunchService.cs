using GBCLV3.Models.Launch;
using GBCLV3.Utils;
using StyletIoC;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBCLV3.Models.Authentication;
using Version = GBCLV3.Models.Launch.Version;

namespace GBCLV3.Services.Launch
{
    public class LaunchService
    {
        #region Events

        public event Action<string> LogReceived;

        public event Action<string> ErrorReceived;

        public event Action<int> Exited;

        #endregion

        #region Private Fields

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

        public async Task<bool> LaunchGameAsync(LaunchProfile profile, Version version)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = profile.IsDebugMode ? _gamePathService.JavaPath : _gamePathService.JavawPath,
                WorkingDirectory = _gamePathService.WorkingDir,
                Arguments = BuildArguments(profile, version),
                UseShellExecute = profile.IsDebugMode,
                RedirectStandardOutput = !profile.IsDebugMode,
                RedirectStandardError = !profile.IsDebugMode,
            };

            _gameProcess = Process.Start(startInfo);

            _gameProcess.EnableRaisingEvents = true;
            _gameProcess.Exited += (s, e) => Exited?.Invoke(_gameProcess.ExitCode);

            if (!profile.IsDebugMode)
            {
                _gameProcess.ErrorDataReceived += (s, e) => ErrorReceived?.Invoke(e.Data);
                _gameProcess.BeginErrorReadLine();

                _gameProcess.OutputDataReceived += (s, e) => LogReceived?.Invoke(e.Data);
                _gameProcess.BeginOutputReadLine();

                if (!_gameProcess.HasExited)
                {
                    await Task.Run(() => _gameProcess.WaitForInputIdle());
                }
            }

            return !_gameProcess.HasExited;
        }

        #endregion

        #region Private Methods

        private string BuildArguments(LaunchProfile profile, Version version)
        {
            var builder = new StringBuilder(8192);

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
            builder.Append($"-Djava.library.path=\"{_gamePathService.NativesDir}\" ");

            // Launcher Identifier
            builder.Append($"-Dminecraft.launcher.brand={AssemblyUtil.Title} ");
            builder.Append($"-Dminecraft.launcher.version={AssemblyUtil.Version} ");

            // Libraries
            builder.Append("-cp \"");
            foreach (var lib in version.Libraries)
            {
                if (lib.Type == LibraryType.Native) continue;
                builder.Append($"{_gamePathService.LibrariesDir}/{lib.Path};");
            }

            // Main Jar
            builder.Append($"{_gamePathService.VersionsDir}/{version.JarID}/{version.JarID}.jar\" ");

            // Authlib-Injector
            if (profile.Account.AuthMode == AuthMode.AuthLibInjector)
            {
                builder.Append(
                    $"-javaagent:\"{_gamePathService.RootDir}/authlib-injector.jar\"={profile.Account.AuthServerBase} ");
                builder.Append("-Dauthlibinjector.side=client ");
                builder.Append("-Dauthlibinjector.yggdrasil.prefetched=");
                builder.Append(profile.Account.PrefetchedAuthServerInfo);
                builder.Append(' ');
            }

            // Main Class
            builder.Append(version.MainClass).Append(' ');

            // Minecraft Arguments
            var argsDict = version.MinecraftArgsDict;

            argsDict["--username"] = '\"' + profile.Account.Username + '\"';
            argsDict["--version"] = '\"' + version.ID + '\"';
            argsDict["--gameDir"] = '\"' + _gamePathService.WorkingDir + '\"';

            if (version.AssetsInfo.IsLegacy)
            {
                argsDict["--assetsDir"] = _gamePathService.AssetsDir + "/virtual/legacy";
            }
            else
            {
                argsDict["--assetsDir"] = '\"' + _gamePathService.AssetsDir + '\"';
                argsDict["--assetIndex"] = version.AssetsInfo.ID;
            }

            if (argsDict.ContainsKey("--uuid")) argsDict["--uuid"] = profile.Account.UUID;
            if (argsDict.ContainsKey("--accessToken")) argsDict["--accessToken"] = profile.Account.AccessToken;
            if (argsDict.ContainsKey("--session")) argsDict["--session"] = profile.Account.AccessToken;
            if (argsDict.ContainsKey("--userType")) argsDict["--userType"] = "mojang";
            if (argsDict.ContainsKey("--versionType")) argsDict["--versionType"] = profile.VersionType;
            if (argsDict.ContainsKey("--userProperties")) argsDict["--userProperties"] = "{}";

            string args = string.Join(" ", argsDict.Select(pair => pair.Key + ' ' + pair.Value));
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
