# Repository Guidelines

## Project Structure & Module Organization
- `Assets/`: Source of truth. Scripts in `Assets/Scripts` (DialogueSystem, Interactions, Inventory), scenes in `Assets/Scenes` (e.g., `Main Scene.unity`), plus `Prefabs/`, `Materials/`, `UI/`, `DialogueData/`, and the `ArticyImporter/` plugin.
- `ProjectSettings/` and `Packages/`: Unity configuration (Unity 6000.0.31f1).
- Generated: `Library/`, `Temp/`, `Logs/`, `obj/` — never commit.

## Build, Test, and Development Commands
- Open: Unity Hub → Open → this folder (Unity 6000.0.31f1).
- Run locally: Open `Assets/Scenes/Main Scene.unity` and press Play.
- Build (Editor): File → Build Settings → select target and Build.
- Build (CLI example): `Unity -batchmode -quit -projectPath . -buildWindows64Player Build/TimeLoop.exe` (adjust path/target).
- Tests (setup): Use Unity Test Framework. Place tests under `Assets/Tests/EditMode` or `Assets/Tests/PlayMode`.
- Tests (run CLI): `Unity -batchmode -projectPath . -runTests -testResults Logs/test-results.xml -testPlatform PlayMode`.

## Coding Style & Naming Conventions
- C#: 4-space indent; LF endings (see `.gitattributes`).
- Naming: PascalCase for classes/methods/properties; camelCase for fields. Prefer `[SerializeField] private` for Inspector fields.
- Structure: One `MonoBehaviour` per file; keep methods focused; use `Awake/Start/Update` idiomatically; cache `FindObjectOfType` results.
- UI: Use `TMP_Text` (TextMesh Pro). Null-check serialized references.

## Testing Guidelines
- Framework: Unity Test Framework (EditMode/PlayMode). Aim for coverage of dialogue flow, interaction triggers, loop reset logic, and Articy sync.
- Naming: Mirror folder under `Assets/Tests/...` and name files `<Feature>Tests.cs`.
- Execute: Test Runner window or the CLI command above.

## Commit & Pull Request Guidelines
- Branches: `feature/<name>`, `fix/<name>` (history also uses `codex/<name>`).
- Commits: Imperative, concise, scoped when helpful (e.g., `feat(dialogue): start node validation`).
- PRs: Clear description, linked issues, screenshots/GIFs for UI, test plan (Play Mode steps), and note the target Unity version.

## Assets, LFS & Merging
- LFS: Large binaries (`*.png`, `*.wav`, `*.fbx`, etc.) are tracked via Git LFS.
- Smart merge: UnityYAMLMerge configured via `.gitattributes`. If needed, configure locally:
  `git config --global merge.unityyamlmerge.driver "unityyamlmerge merge -p \"$BASE\" \"$REMOTE\" \"$LOCAL\" \"$MERGED\""`.
- Always commit `.meta` files; never commit `Library/` or `Temp/`.

