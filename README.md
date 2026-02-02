Monolith 🌌

Monolith is a smart library provider for Playnite designed to find the games other scanners simply ignore. Whether it’s a DRM-free title, a standalone indie gem, or a backup from your "personal archives" with a messy folder name, Monolith digs deep to identify and import them with clean, metadata-ready names. It’s built to turn a chaotic directory of standalone copies into a polished, console-like collection without the manual headache.
Key Features

    Finds Everything: Scans sub-directories to detect standalone games and archives that official launchers won't track.

    Clean Names, Every Time: Uses custom Regex to strip away noise like "Repack," "v1.0," "Shipping," or "GOG Edition," turning messy names into clean titles like Subnautica or Cyberpunk 2077.

    Heuristic Engine: Prioritizes .exe names over directory names to ensure a 100% metadata match on IGDB.

    Smart Fallback: If an executable name is a cryptic abbreviation (like tlou_i.exe), it automatically pulls the title from the parent folder instead.

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

<img width="1363" height="888" alt="2" src="https://github.com/user-attachments/assets/0fbb26ca-48d9-485b-9c8d-782f14ac8c7d" />

<img width="2560" height="1439" alt="3" src="https://github.com/user-attachments/assets/19cb5263-5eb2-401b-852b-09ce209751a9" />

<img width="2382" height="420" alt="4" src="https://github.com/user-attachments/assets/2f39ff3b-5dfb-42af-ad6f-34fc0755ea2e" />



Disclaimer

Monolith is a tool designed for organizing and managing games you legally own. The developer is not responsible for how the tool is used or the nature of the files being scanned.
License

MIT License. Feel free to fork and improve!
