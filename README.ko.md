# Scene Serialized Field Viewer

한국어 | [English](README.md)

현재 활성 씬에 있는 MonoBehaviour 컴포넌트의 직렬화 필드를 한 곳에서 확인하고 수정할 수 있는 Unity EditorWindow 도구입니다.

기본 상태에서는 스크립트 파일이 `Assets/` 폴더 아래에 있는 프로젝트 스크립트만 보여줍니다. Unity 기본 컴포넌트나 패키지 MonoBehaviour는 기본적으로 숨겨지며, `모든 MonoBehaviour` 옵션을 켜면 함께 볼 수 있습니다.

## 기능

- 현재 활성 씬의 직렬화 필드 확인
- Unity의 `SerializedObject`, `SerializedProperty` API를 통한 필드 수정
- 프로젝트 스크립트만 기본 표시
- 옵션으로 모든 MonoBehaviour 컴포넌트 표시
- GameObject 경로 또는 스크립트 이름으로 검색
- 계층순, GameObject 이름순, 스크립트 이름순 정렬
- 개별 결과 숨김 및 다시 표시
- 원본 GameObject 선택 및 ping
- Unity Undo 시스템을 통한 필드 수정 되돌리기
- 한국어/영어 UI 전환

## 설치

다음 파일을 Unity 프로젝트에 복사합니다.

```text
Assets/Editor/SceneSerializedFieldViewer.cs
```

Unity는 `Assets/Editor` 안의 스크립트를 에디터 전용 코드로 컴파일합니다. 따라서 이 도구는 플레이어 빌드에 포함되지 않습니다.

## 사용법

Unity 메뉴에서 도구를 엽니다.

```text
Tools > Scene Serialized Field Viewer
```

씬이나 스크립트를 변경한 뒤에는 `새로고침`을 누릅니다. 비활성 GameObject까지 보고 싶다면 `비활성 포함`을 켭니다. `Button`, `PlayerInput`, `TextMeshProUGUI` 같은 패키지/Unity UI 컴포넌트까지 보고 싶다면 `모든 MonoBehaviour`를 켭니다.

## 요구 사항

- Unity 2021.3 이상 권장
- Unity 6000.3.10f1에서 테스트됨

## 제한 사항

- 현재 활성 씬만 검색합니다.
- 씬 밖의 프리팹 에셋은 검색하지 않습니다.
- 여러 씬을 동시에 검색하지 않습니다.
- 결과 내보내기 기능은 없습니다.
- 숨김 및 접힘 상태는 현재 Unity 에디터 세션 동안만 유지됩니다.

## 라이선스

MIT License. `LICENSE` 파일을 참고하세요.
