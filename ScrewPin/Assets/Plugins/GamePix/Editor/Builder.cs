using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GamePix.Editor
{
    public class Builder
    {
        private static string buildRoot = "html5";
        private static readonly string buildDirectory = "Build";
        private static readonly string applicationInfoJson = "appinfo.json";
        private static readonly string symbolsFileName = "MethodMap.tsv";

        private static readonly string[] symbolsPaths =
        {
            Path.Combine("Library", "Bee", "artifacts", "WebGL", "il2cppOutput", "cpp", "Symbols", symbolsFileName),
            Path.Combine("Temp", "StagingArea", "Data", "il2cppOutput", "Symbols", symbolsFileName),
            Path.Combine("Temp", "EditorBuildOutput", symbolsFileName)
        };

#if UNITY_2021_2_OR_NEWER
        private static readonly string gpxBinaryFileName = "libunity-gpx.a";
#else
        private static readonly string gpxBinaryFileName = "libunity-gpx.bc";
#endif

        private static readonly string gpxBinaryPath =
            Path.Combine("Assets", "Plugins", "GamePix", "impl", gpxBinaryFileName);

        private static readonly string logsDirectory = "Logs";
        private static readonly string editorLogFileName = "Editor.log";
        private static readonly string projectSettingsFileName = "ProjectSettings.asset";

#if UNITY_2022_2_OR_NEWER
        private static readonly string emccArgsPrefix = "Player";
        private static readonly string emccArgsExtension = ".dag.json";
        private static readonly string emccArgsPath = Path.Combine("Library", "Bee");
#elif UNITY_2021_2_OR_NEWER
        private static readonly string emccArgsPrefix = string.Empty;
        private static readonly string emccArgsExtension = ".rsp";
        private static readonly string emccArgsPath = Path.Combine("Library", "Bee", "artifacts", "rsp");
#else
        private static readonly string emccArgsPrefix = string.Empty;
        private static readonly string emccArgsExtension = ".resp";
        private static readonly string emccArgsPath = "Temp";
#endif

        private static readonly string packMetadata = "gmpx.v3";
        private static readonly int archiveAttemptCount = 3;
        private static readonly int archiveAttemptTimeout = 5000;

        public static bool BuildArchive(
            string root,
            bool changedResources = true,
            CompressOptions compressFormat = CompressOptions.Uncompressed,
            bool autoRunPlayer = true)
        {
            buildRoot = root;
            if (Directory.Exists(buildRoot))
            {
                Directory.Delete(buildRoot, true);
            }

            if (Build(compressFormat, autoRunPlayer))
            {
                try
                {
                    CopyGamepixBinary();
                    SaveApplicationInfo(changedResources);
                    CopyDebugSymbols();
                    CopyLogs();

                    for (int i = 1; i < archiveAttemptCount + 1; i++)
                    {
                        Debug.Log("Archiving attempt: " + i);
                        try
                        {
                            Archive();
                            break;
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }

                        if (i < archiveAttemptCount)
                        {
                            Thread.Sleep(archiveAttemptTimeout);
                        }
                        else
                        {
                            throw new Exception("Can't create archive");
                        }
                    }
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    EditorUtility.DisplayDialog(
                        "GamePix",
                        "Can't create game bundle (.gpx).\n More info in Unity console",
                        "Ok");
                }
            }
            
            return false;
        }

        private static bool Build(
            CompressOptions compressFormat = CompressOptions.Uncompressed,
            bool autoRunPlayer = true)
        {
            Debug.Log("Start build");
            DateTime buildStart = DateTime.Now;
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetBuildScenes().Where(s => s.enabled).Select(c => c.path).ToArray();
            buildPlayerOptions.locationPathName = buildRoot;
            Debug.LogFormat("Build directory: {0}", buildPlayerOptions.locationPathName);
            buildPlayerOptions.target = BuildTarget.WebGL;
            if (autoRunPlayer)
            {
                buildPlayerOptions.options = BuildOptions.AutoRunPlayer;
            }

            switch (compressFormat)
            {
                case CompressOptions.StandardCompression:
                    buildPlayerOptions.options = buildPlayerOptions.options | BuildOptions.CompressWithLz4;
                    break;
                case CompressOptions.HighCompression:
                    buildPlayerOptions.options = buildPlayerOptions.options | BuildOptions.CompressWithLz4HC;
                    break;
                default:
                    buildPlayerOptions.options = buildPlayerOptions.options | BuildOptions.UncompressedAssetBundle;
                    break;
            }

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                TimeSpan duration = DateTime.Now - buildStart;
                Debug.LogFormat("Build succeeded. Size: {0:0.00} Mb. Duration: {1:0.00} min.",
                    (double)summary.totalSize / 1024 / 1024,
                    duration.TotalMinutes);
                return true;
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }

            return false;
        }

        public static EditorBuildSettingsScene[] GetBuildScenes()
        {
            return EditorBuildSettings.scenes;
        }

        public static void SaveBuildScene(EditorBuildSettingsScene scene)
        {
            EditorBuildSettingsScene[] scenes = GetBuildScenes()
                .Where(s => s.guid.Equals(scene.guid) || s.path.Equals(scene.path))
                .Select(s =>
                {
                    s.enabled = scene.enabled;
                    return s;
                })
                .ToArray();
            EditorBuildSettings.scenes = scenes;
        }

        private static void SaveApplicationInfo(bool changedResources)
        {
            FileInfo jsonFile = new FileInfo(Path.Combine(buildRoot, buildDirectory, applicationInfoJson));
            using (StreamWriter stream = jsonFile.CreateText())
            {
                ApplicationInfo.resourcesFormat = changedResources ? ResourcesFormat.Gpx : ResourcesFormat.Default;
                stream.Write(ApplicationInfo.ToJson());
            }
        }

        private static void CopyDebugSymbols()
        {
            string projectPath = Directory.GetCurrentDirectory();
            foreach (string symbolsPath in symbolsPaths)
            {
                FileInfo symbolsFile = new FileInfo(Path.Combine(projectPath, symbolsPath));
                if (symbolsFile.Exists)
                {
                    symbolsFile.CopyTo(Path.Combine(buildRoot, buildDirectory, symbolsFileName), true);
                    return;
                }
            }

            throw new Exception(string.Format("Debug symbols file {0} is not exist", symbolsFileName));
        }

        private static void CopyGamepixBinary()
        {
            string projectPath = Directory.GetCurrentDirectory();
            FileInfo binaryFile = new FileInfo(Path.Combine(projectPath, gpxBinaryPath));
            if (!binaryFile.Exists)
            {
                throw new Exception(string.Format("Gamepix binary file {0} is not exist", binaryFile.FullName));
            }

            binaryFile.CopyTo(Path.Combine(buildRoot, buildDirectory, gpxBinaryFileName), true);
        }

        private static void CopyLogs()
        {
            string logsPath = Path.Combine(buildRoot, logsDirectory);
            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }

            FileInfo[] editorLogPaths =
            {
                new FileInfo(Path.Combine(Application.persistentDataPath, "..", "..",
                    editorLogFileName)),
                new FileInfo(Path.Combine(Application.persistentDataPath, "..", "..", "..",
                    "Local", "Unity", "Editor", editorLogFileName)),
                new FileInfo(Path.Combine(Application.persistentDataPath, "..", "..", "..",
                    "Logs", "Unity", "Editor", editorLogFileName))
            };
            foreach (FileInfo path in editorLogPaths)
            {
                if (path.Exists)
                {
                    path.CopyTo(Path.Combine(logsPath, editorLogFileName), true);
                    break;
                }
            }

            FileInfo projectSettings = new FileInfo(Path.Combine("ProjectSettings", projectSettingsFileName));
            if (projectSettings.Exists)
            {
                projectSettings.CopyTo(Path.Combine(logsPath, projectSettingsFileName), true);
            }

            if (!Directory.Exists(emccArgsPath))
            {
                return;
            }

            string searchPattern = emccArgsPrefix + "*" + emccArgsExtension;
#if UNITY_2022_2_OR_NEWER
            CopyLatestFile(emccArgsPath, searchPattern, SearchOption.TopDirectoryOnly, logsPath);
#else
            CopyAllFiles(emccArgsPath, searchPattern, SearchOption.TopDirectoryOnly, logsPath);
#endif
        }

        private static void CopyLatestFile(string source, string searchPattern, SearchOption searchOption,
            string target)
        {
            DirectoryInfo directory = new DirectoryInfo(source);
            FileInfo file = directory
                .GetFiles(searchPattern, searchOption)
                .OrderByDescending(item => item.LastWriteTime)
                .First();
            string targetFilePath = Path.Combine(target, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        private static void CopyAllFiles(string source, string searchPattern, SearchOption searchOption, string target)
        {
            foreach (string file in Directory.GetFiles(source, searchPattern, searchOption))
            {
                string targetFilePath = Path.Combine(target, Path.GetFileName(file));
                File.Copy(file, targetFilePath, true);
            }
        }

        private static void Archive()
        {
            Debug.Log("Archiving process started...");
            if (!Directory.Exists(buildRoot))
            {
                throw new Exception($"Directory with build {buildRoot} is not exist");
            }

            string archiveFileName = string.Format("{0}_{1}_{2}.gpx",
                ApplicationInfo.companyName,
                ApplicationInfo.productName,
                PlayerSettings.bundleVersion);
            string archivePath = Path.Combine(buildRoot, archiveFileName);

            try
            {
                Debug.Log("Archive file creating started...");
                using (FileStream archive = File.Create(archivePath))
                {
                    Debug.Log("Stream creating started...");
                    using (ZipOutputStream stream = new ZipOutputStream(archive))
                    {
                        ZipEntry zipEntry = new ZipEntry(archiveFileName);
                        stream.PutNextEntry(zipEntry);
                        Debug.Log("Writing to archive started...");
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            writer.WriteString(packMetadata);

                            int rootLength = buildRoot.Length + 1;
                            IEnumerable<string> paths =
                                Directory.EnumerateFiles(buildRoot, "*.*", SearchOption.AllDirectories);
                            StringBuilder addedPaths = new StringBuilder();
                            foreach (string path in paths)
                            {
                                if (path == archivePath)
                                {
                                    continue;
                                }

                                string filename = path.Substring(rootLength);
                                writer.WriteString(filename);

                                using (FileStream reader = File.OpenRead(path))
                                {
                                    int numBytesToRead = (int)reader.Length;
                                    byte[] bytes = new byte[numBytesToRead];
                                    int offset = 0;
                                    while (numBytesToRead > 0)
                                    {
                                        int read = reader.Read(bytes, offset, numBytesToRead);
                                        if (read == 0)
                                        {
                                            break;
                                        }

                                        offset += read;
                                        numBytesToRead -= read;
                                    }

                                    writer.WriteBytes(bytes);
                                    addedPaths.AppendLine(path);
                                }
                            }
                            
                            Debug.LogFormat("Added to archive:\n{0}", addedPaths);
                            Debug.LogFormat("Archive created successfully: {0}", archivePath);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (File.Exists(archivePath))
                {
                    try
                    {
                        File.Delete(archivePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                throw new Exception($"Can't create archive: {archivePath}. Details {exception}");
            }
        }
    }
}