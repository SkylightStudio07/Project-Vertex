# Project-Vertex Agent Instructions

## Unity tasks
- Before doing Unity-related work, inspect `.agents/skills` and read the relevant `SKILL.md`.
- Prefer the official Unity Technologies skills already installed in this repository.
- For Unity CLI, Unity Editor, scene, prefab, GameObject, asset, build, test, package, or project operations, start with `.agents/skills/unity-cli/SKILL.md` unless a more specific installed skill is clearly applicable.
- For UI work, also inspect the relevant UI skill such as `ui-ugui` or `ui-uitk`.
- For sprite/import/slicing work, inspect `sprite-editor` and related sprite skills.
- For package changes, inspect `unity-package-management`.

## Live Editor workflow
- Before editing scenes, GameObjects, prefabs, or Unity assets, run:
  `unity status --format json --project-path "C:\\Users\\SKYLIGHT\\Documents\\Graduation\\Project-Vertex"`
- If the Editor reports `state: ready`, use `unity command`, `unity list`, and Pipeline/Editor commands against the live Editor instead of hand-editing `.unity`, `.prefab`, or `.asset` YAML.
- Discover the commands exposed by the current Editor; do not assume command names.
- Only fall back to direct file edits when the relevant skill allows it and no usable live Editor path is available.
- If the Editor cannot be reached, check for Safe Mode / compile errors before concluding that Unity is closed.

## Local shell
- This project runs on Windows.
- Unity CLI is available as `unity`.
- Node is installed.
- In PowerShell use `npx.cmd` rather than `npx` because the local execution policy can block `npx.ps1`.
- Use machine-readable Unity output (`--format json`) when parsing results programmatically.

## Safety
- Preserve unrelated user changes already present in the working tree.
- Do not reset, discard, overwrite, or clean unrelated modified/untracked files.
- Prefer targeted changes and verify them after execution.

