# 로딩 화면 A · 조각 요청

`Loading_A_Mockup.png` 확정. 게임에서는 엠블럼·진행선·글자가 움직이므로 아래처럼 나눠 준다. 글자는 굽지 않는다(게임 TMP).

| 파일 | 내용 |
|---|---|
| `Loading_BG.png` (1920×1080) | 배경만: 회백 다각 면 + 종이 결 + 모서리 십자 표식·가는 사선들. **엠블럼·글자·진행선 없이** |
| `Emblem_Star.png` (512×512, 투명) | 가운데 네 갈래 별만 (먹색). 게임에서 천천히 돌거나 뒤집힌다 |
| `Emblem_Frame.png` (512×512, 투명) | 별을 둘러싼 끊긴 마름모 틀 + 위의 작은 사각점·세로선, 좌우 가는 선. 별과 중심 맞춤 |
| `Track.png` | 진행선 회색 바탕 (9-slice 가로) |
| `Fill.png` | 진행선 청록 채움 (9-slice 가로) |
| `Head_Diamond.png` | 진행 끝 청록 마름모 |
| `layout.json` | 엠블럼 중심·크기, 진행선 rect, 글자 상자(VERTEX · 안내 · % · FIELD GUIDE · 팁 · PROJECT VERTEX / ARCHIVE)의 위치·크기·정렬·색·자간 |

- 모두 SVG 원본 같이, 2배 해상도 PNG.
