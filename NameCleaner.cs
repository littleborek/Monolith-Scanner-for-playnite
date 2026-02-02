using System;
using System.Text.RegularExpressions;

namespace Monolith
{
    public static class NameCleaner
    {
        // 1. Remove Content in Brackets/Parentheses [FitGirl Repack] or (v1.0)
        private static readonly Regex BracketsRegex = new Regex(@"\[.*?\]|\(.*?\)", RegexOptions.Compiled);

        // 2. Remove Version Patterns like v1.0.3, v1.0, Build 456
        private static readonly Regex VersionRegex = new Regex(@"\bv?\.?\d+(\.\d+)+([a-z])?\b|\bBuild\s*\d+\b|\bv\d+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 3. Remove Known Groups, Release Types, and Garbage Words
        // Added "DE" as requested, along with common scene terms.
        private static readonly Regex GarbageRegex = new Regex(
            @"\b(REPACK|KaOs|DODI|FitGirl|Empress|RUNE|TENOKE|SKIDROW|CODEX|PLAZA|PROPHET|GOG|ElAmigos|Goldberg|Portable|MULTi\d+|DLC|Update|Setup|DE|GOTY|Digital Deluxe|Edition|Remastered|Part I)\b", 
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
            
        private static readonly Regex SafeGarbageRegex = new Regex(
            @"\b(REPACK|KaOs|DODI|FitGirl|Empress|RUNE|TENOKE|SKIDROW|CODEX|PLAZA|PROPHET|GOG|ElAmigos|Goldberg|Portable|MULTi\d+|DLC|Update|Setup|DE)\b", 
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 4. Clean Symbols (dots, underscores, dashes becomes spaces)
        private static readonly Regex SymbolRegex = new Regex(@"[_\.\-]", RegexOptions.Compiled);

        // 5. Clean Spaces
        private static readonly Regex SpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        public static string Clean(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string name = input;

            // Step 1: Remove brackets first (contains mostly metadata/noise)
            name = BracketsRegex.Replace(name, "");
            
            // Step 2: Remove Version numbers (MUST BE DONE BEFORE SYMBOL REPLACEMENT)
            // Otherwise "v1.0.2" becomes "v1 0 2" and checking for dots fails.
            name = VersionRegex.Replace(name, "");

            // Step 3: Remove Specific Garbage Words (Repack, Groups, etc.)
            name = SafeGarbageRegex.Replace(name, "");

            // Step 4: Replace separators with spaces to isolate words
            name = SymbolRegex.Replace(name, " ");
            
            // Step 5: Intelligent Spacing for CamelCase and AttachedNumbers
            // "TheLastOfUs" -> "The Last Of Us"
            name = Regex.Replace(name, @"([a-z])([A-Z])", "$1 $2");
            // "Cyberpunk2077" -> "Cyberpunk 2077"
            name = Regex.Replace(name, @"([a-zA-Z])(\d)", "$1 $2");

             // Step 6: Remove Bit-ness and executable suffixes (32, 64, Shipping) ONLY at the end
            // Handles "Subnautica 32" -> "Subnautica"
            name = Regex.Replace(name, @"\s+(32|64|86|Shipping|Launcher|Client|Mod|Tool)$", "", RegexOptions.IgnoreCase);

            // Step 7: Final Polish
            return SpaceRegex.Replace(name, " ").Trim();
        }
    }
}
