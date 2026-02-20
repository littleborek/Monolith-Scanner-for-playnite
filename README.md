Monolith 🌌

Monolith is a smart library provider for Playnite designed to find the games other scanners simply ignore. Whether it’s a DRM-free title, a standalone indie gem, or a backup from your "personal archives" with a messy folder name, Monolith digs deep to identify and import them with clean, metadata-ready names. It’s built to turn a chaotic directory of standalone copies into a polished, console-like collection without the manual headache.
Key Features

    Finds Everything: Scans sub-directories to detect standalone games and archives that official launchers won't track.

    Clean Names, Every Time: Uses custom Regex to strip away noise like "Repack," "v1.0," "Shipping," or "GOG Edition," turning messy names into clean titles like Subnautica or Cyberpunk 2077.

    Heuristic Engine: Prioritizes .exe names over directory names to ensure a 100% metadata match on IGDB.

    Smart Fallback: If an executable name is a cryptic abbreviation (like tlou_i.exe), it automatically pulls the title from the parent folder instead.

    Auto-Scan: Optionally scans your directories for new games every time Playnite starts.

    Custom Skip List: Peace of mind with a user-definable blacklist for specific files, folders, or extensions you want to ignore.

    Built-in Blacklist: Automatically skips system tools, DirectX folders, and updaters to keep your library clutter-free.

Usage

    Install the Monolith plugin in Playnite.

    Go to Add-ons -> Extension Settings -> Monolith.

    Add your game root folders (e.g., D:\Games or E:\Backups).

    Update your library and let the engine do the cleaning.

Technical Details

    Language: C#

    Framework: .NET Framework 4.6.2 (Playnite SDK Standard)

    Logic:

        Implements an Order of Operations fix: Cleans version/group tags before removing punctuation to maintain regex accuracy.

        PascalCase Splitting: Automatically turns TheLastOfUs into The Last Of Us for better API matching.

        Heuristic Scoring: Validates game folders based on file signatures and excludes known non-game executables.

Disclaimer

Monolith is a tool designed for organizing and managing games you legally own. The developer is not responsible for how the tool is used or the nature of the files being scanned. This project has been developed with AI assistance from Antigravity.
License

MIT License. Feel free to fork and improve!

---
**Author:** Berk Kocabörek ([littleborek](https://github.com/littleborek))