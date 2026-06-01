# Scene Serialized Field Viewer

[한국어](README.ko.md) | English

Unity EditorWindow utility for inspecting and editing serialized fields on MonoBehaviour components in the currently active scene.

The window defaults to showing project scripts whose script files live under `Assets/`. Package and Unity built-in MonoBehaviours are hidden by default, but can be shown with the `All MonoBehaviours` option.

## Features

- Inspect serialized fields in the active scene.
- Edit fields through Unity's `SerializedObject` and `SerializedProperty` APIs.
- Show only project scripts by default.
- Optionally include all MonoBehaviour components.
- Search by GameObject path or script name.
- Sort by hierarchy, GameObject name, or script name.
- Hide and restore individual result groups.
- Select and ping the source GameObject.
- Undo field edits with Unity's normal Undo system.
- Switch tool labels between Korean and English.

## Installation

Copy the editor script into any Unity project:

```text
Assets/Editor/SceneSerializedFieldViewer.cs
```

Unity compiles scripts inside `Assets/Editor` as editor-only code, so this tool is not included in player builds.

## Usage

Open the tool from:

```text
Tools > Scene Serialized Field Viewer
```

Use `Refresh` after changing scenes or scripts. Use `Include Inactive` to include inactive GameObjects. Use `All MonoBehaviours` to include package and Unity UI components such as `Button`, `PlayerInput`, and `TextMeshProUGUI`.

## Requirements

- Unity 2021.3 or newer recommended.
- Tested in Unity 6000.3.10f1.

## Limitations

- Searches only the currently active scene.
- Does not search prefab assets outside the scene.
- Does not search multiple loaded scenes at once.
- Does not export results.
- Hidden and folded results are remembered for the current Unity editor session.

## License

MIT License. See `LICENSE`.
