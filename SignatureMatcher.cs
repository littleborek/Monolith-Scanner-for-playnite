using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Monolith
{
    public class SignatureMatcher
    {
        // High Score (+50)
        private static readonly HashSet<string> HighScoreFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "steam_api64.dll", "steam_api.dll", "Galaxy64.dll", "EOSSDK-Win64-Shipping.dll", "tier0.dll"
        };

        // Medium Score (+20)
        private static readonly HashSet<string> MediumScoreFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "UnityPlayer.dll", "unins000.exe"
        };

        // Engine Indicators (+30) - Files
        private static readonly HashSet<string> EngineFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "UE4Game.exe", "UE5Game.exe" // Added UE5
        };
        
        // Blacklisted Executables
        private static readonly HashSet<string> BlacklistExes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "uninstall.exe", "config.exe", "setup.exe", "dxsetup.exe", "crashreporter.exe",
            "vcredist_x64.exe", "vcredist_x86.exe", "unitycrashhandler64.exe", "unitycrashhandler32.exe",
            "redprelauncher.exe", "HardwareUpdater.exe", "creadist.exe"
        };

        // Blacklisted Folders
        private static readonly HashSet<string> BlacklistFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CommonRedist", "DirectX", "DotNet", "VCRedist", "Support", "Tools", "Bonus", 
            "Soundtrack", "Artbook", "HardwareSupport", "HardwareUpdater", "Launcher", 
            "CrashReporter", "Engine", "Prerequisites", "Installers", "Redist", "$Recycle.Bin",
            "System Volume Information", "Windows", "Program Files", "Program Files (x86)"
        };

        public bool IsGameFolder(string directoryPath, out int score)
        {
            score = 0;
            if (!Directory.Exists(directoryPath)) return false;

            try
            {
                var files = Directory.GetFiles(directoryPath);
                var dirs = Directory.GetDirectories(directoryPath);
                var folderName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar));

                // Immediate blacklist check
                if (BlacklistFolders.Contains(folderName)) return false;

                // 1. Check Root Files
                score += ScoreFiles(files, folderName);

                // 2. Check Root Directories
                foreach (var dir in dirs)
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName.EndsWith("_Data", StringComparison.OrdinalIgnoreCase)) score += 30;
                    if (dirName.Equals("Binaries", StringComparison.OrdinalIgnoreCase)) score += 10;
                    if (dirName.Equals("bin", StringComparison.OrdinalIgnoreCase)) score += 10; // Added bin
                    if (dirName.Equals("Engine", StringComparison.OrdinalIgnoreCase)) score += 10;
                    if (dirName.Equals("Content", StringComparison.OrdinalIgnoreCase)) score += 10;
                    if (dirName.Equals("game", StringComparison.OrdinalIgnoreCase)) score += 10; // CS2 structure
                }

                // 3. Deep Scan for signatures if score is suspicious but not confirmed
                // Often games like CS2 have no DLLs in root, but have them in bin/win64 or game/bin/win64
                if (score < 50) 
                {
                    // Check specific subfolders for high score files
                    var subPathsToCheck = new[] { "bin", "Binaries", "game/bin" };
                    foreach (var subPath in subPathsToCheck)
                    {
                        var fullSubPath = Path.Combine(directoryPath, subPath);
                        if (Directory.Exists(fullSubPath))
                        {
                            // We do a recursive search only for DLLs in these specific valid subtrees to save time
                            // limiting depth implicitly by only checking these start points? 
                            // Actually, let's just check the immediate files in these likely bin folders.
                             // Many games have bin/x64/steam_api64.dll so we might need one more level.
                            try 
                            { 
                                var deepFiles = Directory.GetFiles(fullSubPath, "*.dll", SearchOption.AllDirectories);
                                // Limit deep check to avoid massive scans
                                int checkedCount = 0;
                                foreach (var file in deepFiles)
                                {
                                    if(checkedCount++ > 50) break;
                                    var fileName = Path.GetFileName(file);
                                    if (HighScoreFiles.Contains(fileName)) 
                                    {
                                        score += 50;
                                        break; // Found a strong indicator, no need to keep scanning
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch 
            {
                // Access errors etc
            }

            // Lowered threshold to 25 to catch more valid games
            return score >= 25;
        }

        private int ScoreFiles(string[] filePaths, string folderName)
        {
            int score = 0;
            foreach (var file in filePaths)
            {
                var fileName = Path.GetFileName(file);
                if (HighScoreFiles.Contains(fileName)) score += 50;
                else if (MediumScoreFiles.Contains(fileName)) score += 20;
                else if (EngineFiles.Contains(fileName)) score += 30;

                if (fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    var fi = new FileInfo(file);
                    if (fi.Length > 5 * 1024 * 1024) score += 10;

                    var exeNameNoExt = Path.GetFileNameWithoutExtension(fileName);
                    if (string.Equals(exeNameNoExt, folderName, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 30;
                    }
                }
            }
            return score;
        }

        public string FindMainExecutable(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return null;

            var folderName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar));
            
            // Recursive search for executables to find games like Cyberpunk (bin/x64/Cyberpunk2077.exe)
            // or CS2 (game/bin/win64/cs2.exe)
            List<string> exes = new List<string>();
            try
            {
                exes = Directory.GetFiles(directoryPath, "*.exe", SearchOption.AllDirectories).ToList();
            }
            catch 
            {
                // Should we try non-recursive if recursive fails? 
                // Usually fails due to AccessDenied on some protected subfolder.
                // Fallback to top directory
                try { exes = Directory.GetFiles(directoryPath, "*.exe", SearchOption.TopDirectoryOnly).ToList(); } catch { }
            }

            var candidates = exes
                .Select(path => new FileInfo(path))
                .Where(fi => !BlacklistExes.Contains(fi.Name) && !fi.Name.StartsWith("unins", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!candidates.Any()) return null;

            // Improved Scoring for executables: 
            // 1. Name match with folder name is VERY strong (Levenshtein)
            // 2. "Shipping" or "Game" in name often indicates the real exe for UE games
            // 3. File size is a good tie breaker
            
            var bestMatch = candidates
                .OrderBy(fi => 
                {
                    // Prioritize exact name match or very close match
                    // This handles valid launchers or root exes effectively
                    string nameNoExt = Path.GetFileNameWithoutExtension(fi.Name);
                    return LevenshteinDistance(nameNoExt, folderName);
                })
                .ThenByDescending(fi => 
                {
                    // Prioritize files with 'Shipping' in name if it's an Unreal game?
                    // Actually, sometimes the 'Shipping' exe is the real one, but usually we want the wrapper if exists?
                    // For now, size is the best generic heuristic.
                    return fi.Length;
                })
                .First();

            return bestMatch.FullName;
        }

        private static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
            if (string.IsNullOrEmpty(t)) return s.Length;

            s = s.ToLowerInvariant();
            t = t.ToLowerInvariant();

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}
