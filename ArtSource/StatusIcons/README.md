# 상태(버프/디버프) 아이콘 원본

`Assets/Art/UI/StatusIcons/*.png`의 **생성 원본**입니다.
Unity는 SVG를 기본 지원하지 않으므로(Vector Graphics 패키지 필요) SVG는 `Assets/` 바깥에 두고 PNG만 넣습니다.

## 구성

| 파일 | 설명 |
|---|---|
| `*.svg` | 아이콘 원본 (64×64 viewBox) |
| `gen-icons.js` | SVG 작성 + PNG(128px) 변환 스크립트 |

## 디자인 규칙

- **흰 글리프 + 어두운 외곽선(#1a1a20), 투명 배경.**
  칩 배경색이 뒤에서 비치는 구조라 아이콘 자체에는 색을 넣지 않습니다.
  (배경색은 `StatusChipView`가 `StatusDefinition.GetDisposition()` 값에 따라 칠합니다 — 버프 초록 / 디버프 빨강 / 중립 회색)
- 실제 표시 크기가 34px이므로 **단순하고 굵은 실루엣**으로 유지합니다.
- 128px로 렌더해 Unity에서 축소되며 안티에일리어싱이 먹도록 합니다.

## 파일명 규칙

**PNG 이름 == StatusDefinition 에셋 이름**이어야 자동 연결됩니다.

```
ArtSource/StatusIcons/Burn.svg
  -> Assets/Art/UI/StatusIcons/Burn.png
  -> Assets/Data/Status/Burn.asset 의 icon 필드
```

이름이 어긋나면 `Tools/Vertex/Assign Status Icons` 실행 시 "대응 아이콘 없음" 목록에 나옵니다.

## 재생성

```bash
npm install @resvg/resvg-js
node gen-icons.js <출력폴더>
cp <출력폴더>/*.png ../../Assets/Art/UI/StatusIcons/
```

그다음 Unity에서 **`Tools/Vertex/Assign Status Icons`** 실행 — PNG를 Sprite로 임포트 설정하고
같은 이름의 StatusDefinition에 연결합니다.

## 관련 메뉴

| 메뉴 | 역할 |
|---|---|
| `Tools/Vertex/Setup Status (Buff/Debuff) UI` | 칩 프리팹 생성 + 플레이어/적 StatusListView 구성 |
| `Tools/Vertex/Assign Status Icons` | PNG 임포트 설정 + StatusDefinition에 아이콘 연결 |
