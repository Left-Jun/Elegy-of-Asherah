# Elegy of Asherah - Workspace Handoff

Last updated: 2026-07-24

This file is the persistent handoff record for future Codex and developer
workspaces. Update it whenever repository state, active priorities, build
requirements, or known risks change.

## Project Identity

- Game name: `Elegy of Asherah`
- Chinese narrative title: `亚舍拉挽歌` (keep in narrative and art content)
- Studio / Unity company name: `Limenauts`
- Main project: `D:\Unity Project\Elegy of Asherah`
- Development copy: `D:\Unity Project\Elegy of Asherah - 副本`
- GitHub: `https://github.com/Left-Jun/Elegy-of-Asherah`
- Main branch: `main`
- Unity version: `2022.3.62f3c1`
- Solution: `Elegy of Asherah.sln`
- Windows player: `ggj\Elegy of Asherah.exe`
- Unity product GUID: `c72323ba01971364c8b8979acd543d18`

Do not rename script classes, namespaces, scenes, Unity GUIDs, the product
GUID, or the asset structure as part of branding work.

## Current Repository State

The full branding rename was committed and pushed on 2026-07-23:

- Commit: `ea1da250ce1cb1122b3b2747a6f9d4a53bc81921`
- Message: `Rename project to Elegy of Asherah`
- Remote: `https://github.com/Left-Jun/Elegy-of-Asherah.git`
- GitHub visibility: public
- GitHub description remains the Chinese game-jam description intentionally.
- The main worktree was clean when this handoff was created.

Unity metadata currently uses:

```text
companyName: Limenauts
productName: Elegy of Asherah
```

The tracked Windows x64 player was built successfully from both projects.
Its `app.info` contains `Limenauts` and `Elegy of Asherah`.

The large tracked file
`ggj/Elegy of Asherah_Data/sharedassets1.assets.resS` is managed by Git LFS.
Git LFS must be installed before cloning, pulling, or pushing this repository.

## Build Settings

The five enabled scenes are:

1. `Assets/Scenes/StartMenu.unity`
2. `Assets/Scenes/练习.unity`
3. `Assets/Scenes/练习 1.unity`
4. `Assets/Scenes/练习 2.unity`
5. `Assets/Scenes/练习 3.unity`

Treat changes to this list as gameplay/build changes, not rename cleanup.

## Development Copy Warning

`D:\Unity Project\Elegy of Asherah - 副本` contains pre-existing uncommitted
URP, scene, and project-setting work. It was intentionally not committed or
pushed during the rename.

- Do not reset, clean, overwrite, commit, or push the copy without reviewing
  every existing change.
- Do not copy its entire settings or assets into the main project.
- Compare and migrate individual intended changes only.
- Always record `git status --short` for both worktrees before shared work.

## Immediate Next Task

Open the main project once from its new path in Unity Hub or Unity Editor, then
rebuild the Windows x64 player from:

```text
D:\Unity Project\Elegy of Asherah
```

Reason: the generated Burst debug manifest still contains the old absolute
build-time path `D:\Unity Project\Tencennt_2026.3`. Do not hand-edit that
generated manifest. A successful rebuild from the new project path must remove
the stale path naturally.

After rebuilding:

1. Confirm Unity exits with code `0` and the log contains no `error CS`.
2. Confirm `ggj\Elegy of Asherah.exe` exists.
3. Confirm `ggj\Elegy of Asherah_Data\app.info` contains the correct company
   and product names.
4. Scan tracked/project files for `Tencennt_2026.3` and `Tencent_2026.3`,
   excluding `.git`, `Library`, `Temp`, and logs.
5. Review all generated player changes before committing because rebuilds can
   update large binary assets.
6. Verify Git LFS still owns `sharedassets1.assets.resS` before pushing.

## New Workspace Startup Checklist

Run these checks before making changes:

```powershell
$main = 'D:\Unity Project\Elegy of Asherah'
$copy = 'D:\Unity Project\Elegy of Asherah - 副本'

git -C $main status --short
git -C $main log -1 --oneline
git -C $main remote -v
git -C $main lfs status
git -C $copy status --short
git -C $copy remote -v
```

Then read this document, inspect the relevant scripts/assets, and verify Unity
is not already running before starting a batch build.

## Ongoing Handoff Update Rules

At the end of each substantial task, update this file with:

- Date and final commit hash, if committed.
- What behavior or assets changed.
- Verification performed and its result.
- Uncommitted changes intentionally left in either worktree.
- Known defects, generated-file caveats, and the next concrete task.
- Any change to Unity version, build target, scene list, LFS tracking, remote,
  or project identity.

Do not turn this file into a chronological dump. Keep current facts in the
sections above and add short dated entries below only when history is useful.

## Handoff Log

### 2026-07-24

- Main and copy folders now use the `Elegy of Asherah` name.
- GitHub, Unity metadata, solution, and Windows player naming are aligned.
- Main rename commit is pushed to `main`.
- The copy's unrelated uncommitted development work remains preserved.
- Next action is a main-project rebuild from the renamed path to remove the
  stale absolute path from generated Burst debug metadata.
