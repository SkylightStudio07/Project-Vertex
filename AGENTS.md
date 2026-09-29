# Project-Vertex Agent Instructions

## Unity tasks
- Before doing Unity-related work, inspect `.claude/skills` and read the relevant `SKILL.md`.
- Skills are installed per developer and `.claude/skills/` is git-ignored, so they are NOT shared through this repository. If the directory is missing or empty, install the Unity CLI skill first: `unity skill install claude-code --local`.
- For Unity CLI, Unity Editor, scene, prefab, GameObject, asset, build, test, package, or project operations, start with `.claude/skills/unity-cli/SKILL.md` unless a more specific installed skill is clearly applicable.
- The wider Unity Technologies skill set (`ui-ugui`, `ui-uitk`, `sprite-editor`, `unity-package-management`) is not installed here. Install it with `npx.cmd skills add Unity-Technologies/skills` when UI, sprite, or package work comes up, then follow the matching `SKILL.md`.

## Live Editor workflow
- Before editing scenes, GameObjects, prefabs, or Unity assets, run `unity status --format json` from the repository root (or pass `--project-path <repo root>`; the checkout path differs per machine).
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

