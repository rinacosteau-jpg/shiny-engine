# Unity Git Setup

This repository is configured for Unity development.

- `.gitignore` excludes Unity-generated folders (`Library/`, `Temp/`, `Logs/`, `obj/`) and IDE files.
- `.gitattributes` normalizes text files to LF and uses Git LFS for large binaries.
- Unity YAML assets (`.unity`, `.prefab`, `.asset`, etc.) use UnityYAMLMerge to reduce merge conflicts.

## Quick Start
```bash
git lfs install
git add -A
git commit -m "Initialize Unity repo"
```

## Open & Run
- Open the project in Unity 6000.0.31f1 via Unity Hub.
- Open `Assets/Scenes/Main Scene.unity` and press Play.

