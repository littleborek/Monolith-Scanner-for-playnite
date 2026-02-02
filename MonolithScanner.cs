using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;

namespace Monolith
{
    public class MonolithScanner
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly SignatureMatcher _matcher;

        public MonolithScanner()
        {
            _matcher = new SignatureMatcher();
        }

        public async Task<List<DiscoveredGame>> ScanDirectoryAsync(string rootPath)
        {
            var results = new ConcurrentBag<DiscoveredGame>();

            if (!Directory.Exists(rootPath))
            {
                Logger.Warn($"Directory not found: {rootPath}");
                return results.ToList();
            }

            Logger.Info($"Starting scan of {rootPath}...");

            await Task.Run(() =>
            {
                try
                {
                    // We will iterate ONE level deep for "game folders" typically found in a library folder.
                    // Assuming rootPath is like "D:\Games", we check "D:\Games\GameA", "D:\Games\GameB".
                    
                    // .NET Framework 4.6.2 compatible directory enumeration
                    var directories = Directory.EnumerateDirectories(rootPath, "*", SearchOption.TopDirectoryOnly);

                    Parallel.ForEach(directories, (dir) =>
                    {
                        try
                        {
                            // Try processing the top-level directory
                            bool found = ProcessDirectory(dir, results);

                            // If not found, try scanning one level deeper (for nested structures like Game/Game/...)
                            if (!found)
                            {
                                try
                                {
                                    var subDirectories = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly);
                                    foreach (var subDir in subDirectories)
                                    {
                                        ProcessDirectory(subDir, results);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Warn(ex, $"Error scanning sub-level of: {dir}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, $"Error scanning subdirectory: {dir}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Error iterating directory: {rootPath}");
                }
            });

            Logger.Info($"Scan complete. Found {results.Count} games.");
            return results.ToList();
        }

        private bool ProcessDirectory(string dirPath, ConcurrentBag<DiscoveredGame> results)
        {
            if (_matcher.IsGameFolder(dirPath, out int score))
            {
                var exePath = _matcher.FindMainExecutable(dirPath);
                if (!string.IsNullOrEmpty(exePath))
                {
                    // Strategy: Prefer Exe Name over Folder Name for cleaner metadata matches (e.g. Subnautica.exe vs Subnautica.v1.0...)
                    var exeName = Path.GetFileNameWithoutExtension(exePath);
                    var folderName = Path.GetFileName(dirPath);
                    
                    var cleanExeName = NameCleaner.Clean(exeName);
                    var cleanFolderName = NameCleaner.Clean(folderName);
                    
                    string finalName = cleanExeName;

                    // Fallback to Folder Name if Exe Name is too generic OR potentially an abbreviation/bad split (Smart Fallback)
                    var genericNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                    { 
                        "game", "launcher", "client", "app", "start", "setup", "main", "play", "shipping", "bootstrapper" 
                    };

                    // Heuristics for "Bad" Exe Name:
                    // 1. Is Generic? (Game.exe)
                    // 2. Is Too Short? (< 5 chars, e.g. "cs2", "aoe4")
                    // 3. Is Cryptic/Abbreviated? (Significantly shorter than folder name, e.g. "tlou i l" (8) vs "The Last of Us Part I" (21))
                    bool isGeneric = genericNames.Contains(cleanExeName);
                    bool isTooShort = cleanExeName.Length < 5; 
                    bool isLikelyAbbreviation = (!string.IsNullOrWhiteSpace(cleanFolderName) && cleanFolderName.Length > cleanExeName.Length + 7);

                    if (string.IsNullOrWhiteSpace(cleanExeName) || isGeneric || isTooShort || isLikelyAbbreviation)
                    {
                        // Use folder name ONLY if it's better (not generic itself, though folder names rarely are simply "Game")
                        if (!string.IsNullOrWhiteSpace(cleanFolderName) && cleanFolderName.Length >= 3)
                        {
                            finalName = cleanFolderName;
                        }
                    }
                    
                    var game = new DiscoveredGame
                    {
                        Name = finalName,
                        InstallDirectory = dirPath,
                        ExecutablePath = exePath,
                        Score = score
                    };
                    results.Add(game);
                    Logger.Info($"Found game: {finalName} (Score: {score}) in {dirPath}");
                    return true;
                }
                else
                {
                    Logger.Trace($"Skipped {dirPath}: Score {score} passed, but no valid executable found.");
                }
            }
            else
            {
                Logger.Trace($"Skipped {dirPath}: Score {score} is below threshold.");
            }
            return false;
        }
    }

    public class DiscoveredGame
    {
        public string Name { get; set; }
        public string InstallDirectory { get; set; }
        public string ExecutablePath { get; set; }
        public int Score { get; set; }
    }
}
