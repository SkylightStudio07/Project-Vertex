# 작전 개시 띠 v7 · 조각 요청

정본: `Operation_Start_Mockup_v7.png` (화면 위쪽 먹색 띠 `작전 개시` + 왼쪽 흰 일련번호 판 `06` + 아래 태그 `일반 조우 · 적 01`).
띠만 자른다. 전장 · 캐릭터 · 카드 · HUD · 의뢰 추적 패널은 자르지 않는다.

## 공통 규칙

- 표면은 이미지 모델로 그린 그림. 정본의 무광 먹색 판, 아주 옅은 톤 차이, 흰 판의 종이 결을 살린다. SVG · 도형 재구성으로 단색 판을 만들지 않는다. 정본을 같은 프롬프트(`PROMPT_Start_v7.txt`)로 고해상도(또는 띠 단독)로 재생성해서 잘라 내는 방식 권장.
- 1~2px 선 · 눈금 같은 단색 장식만 깨끗한 단색으로 그려도 된다.
- **글자 · 숫자를 그림에 넣지 않는다.** `작전 개시` · `OPERATION START` · `06` · `OPERATION SERIAL // 06` · 오른쪽의 큰 흐린 `06` · 태그 문구 전부 게임이 쓴다(층 번호 · 적 수가 바뀐다).
- 2배 PNG(화면 1px = PNG 2px), 투명 배경, 회전 0°, 이진 알파.

## 조각 `Extracted_Start_v7/`

| 파일 | 내용 |
|---|---|
| `Band.png` | 긴 먹색 띠: 오른쪽 끝 잘린 모서리, 오른쪽 끝 옅은 톤 면. 글자 자리는 조용하게 |
| `Band_Shadow.png` | 띠 뒤 얇은 먹색 오프셋 받침 |
| `Serial_Block.png` | 왼쪽 흰 일련번호 판(잘린 모서리 포함) |
| `Serial_Ticks.png` | 흰 판 왼쪽 아래 짧은 가로 눈금 묶음 |
| `Serial_Rule.png` | 흰 판 안 번호 아래 짧은 선 |
| `Serial_Divider.png` | 흰 판 오른쪽의 세로 얇은 선 |
| `Line_Top.png` | 띠 위로 길게 지나가는 얇은 선 (가로로 늘림, 9-slice 좌우 2) |
| `Title_Rule.png` | 제목 양옆 회색 가로선 (좌우에 같은 조각을 쓴다) |
| `Ticks_Right.png` | 띠 오른쪽 끝 세로 눈금 |
| `Tag.png` | 띠 아래 가운데 먹색 사다리꼴 태그 (가로로 늘림, 9-slice 경계는 사선 끝 포함) |
| `Tag_Bar.png` | 태그 안 양끝 짧은 세로 막대 (좌우에 같은 조각) |
| `Tag_EliteMark.png` | **정예 조우용**: 태그 왼쪽 안에 얹는 작은 칩(뼈흰색 또는 청록 3px 점 정도로 절제). 정본에 없으니 같은 장면에 태그 문구 `정예 조우 · 적 02`로 바꾼 확인용 목업 `Operation_Start_Mockup_v7_Elite.png`도 같이 |

## `layout.json`

- 기준 1920×1080, 왼쪽 위 원점, `[x,y,w,h]` 화면 px (정본 1685×934에서 1920 기준으로 환산).
- 조각마다 `file` · `png_size_px` · `screen_rect` · 9-slice면 `nine_slice_px`.
- **`text_slots`**: 글자 자리마다 `[x,y,w,h]` · 정렬 · 글자 크기(px) · 굵기 · 색(hex) · 자간 · 불투명도. `serial`(06), `serial_caption`(OPERATION SERIAL // 06), `eng`(OPERATION START), `title`(작전 개시), `ghost`(오른쪽 큰 흐린 06 — 띠 밖으로 잘리는 부분이 있으면 잘리는 범위도), `tag`(일반 조우 · 적 01), 정예일 때 `tag_elite`(칩 안 글자가 있으면).

## 결과물

`Extracted_Start_v7/` — PNG + `layout.json` + `Contact_Sheet.jpg`(조각 단독 + 정본 좌표로 다시 조립해 정본 옆에 나란히, 일반 · 정예) + `README.md`(SVG 미사용 확인, 생성 프롬프트).
