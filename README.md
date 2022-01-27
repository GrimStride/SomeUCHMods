## Installing
- Download BepInEx from https://github.com/BepInEx/BepInEx/releases
- Extract all the files to the `Ultimate Chicken Horse` folder
- Your game folder should now look something like this:
  ```
  └── Ultimate Chicken Horse/
    ├── BepInEx
    ├── MonoBleedingEdge
    ├── UltimateChickenHorse_Data
    ├── changelog.txt
    ├── debug.log
    ├── doorstop_config.ini
    ├── UltimateChickenHorse.exe
    ├── UnityCrashHandler64.exe
    ├── UnityPlayer.dll
    ├── Version.txt
    └── winhttp.dll
    ```
- If you are not running Windows follow this guide for some extra steps: https://docs.bepinex.dev/articles/user_guide/installation/index.html
- Download a mod from the [Releases](https://github.com/GrimStride/SomeUCHMods/releases) and extract it
- Once extracted the mod should consist of a `.dll` inside a folder, move that folder to `BepInEx/plugins`

## Building
- Download and install [.NET Framework 4.8  Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-developer-pack-offline-installer)
- Download and install one of the following:
  * [Visual Studio](https://visualstudio.microsoft.com/downloads)
  * [Build Tools for Visual Studio](https://visualstudio.microsoft.com/downloads)
  * [Mono](https://www.mono-project.com/download/stable/)
- **Optional:** Create an environment variable called `UCHPATH` that contains the path to the Ultimate Chicken Horse game folder.
- Open a `Developer Command Prompt` if you installed Visual Studio or the Build Tools, open a `Mono Command Prompt` if you installed Mono
- `cd` to the folder of the mod you want to build
- Run the following command:
  ```
  dotnet msbuild <Mod Name>.csproj -p:UCHPATH=<Path to Ultimate Chicken Horse game folder>;Configuration=Release`
  ```
  * If you installed the game in the default Steam location and you are going to build the RoundCounter mod, the command should look like this:
    ```
    dotnet msbuild RoundCounter.csproj -p:UCHPATH="C:\Program Files (x86)\Steam\steamapps\common\Ultimate Chicken Horse";Configuration=Release
    ```
    * Always put double quotes `"` if your path contains spaces
    * If you set the environment variable of the optional step above, you can shorten the last part of the command to:
      ```
      -p:Configuration=Release
      ```
    * Change `Release` to `Debug` if you want to build the debug version of the mod
- The compiled `.dll` will be inside the `build` folder
