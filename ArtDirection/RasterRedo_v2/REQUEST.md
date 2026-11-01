# 전투 UI 조각 다시 그리기 (이미지 모델 생성본) — 스킬 배너 · 전투 종료 · 컷인 대사 태그

아래 세 UI의 조각은 지난번에 **SVG / 도형으로 다시 그린 단색 조각**으로 들어갔다 (각 README에 "geometric redraw"라고 적혀 있다). 목업의 종이 질감 · 잉크 질감 · 인쇄 느낌이 빠져서 게임 화면에서 납작해 보인다.
대화 선택지 v1 때(`../DialogueMockup/Choice_v1/Extracted_v2/`)와 같은 방식으로 **표면만 이미지 모델로 다시 그린다.** 이 방식이 잘 됐으니 그대로 따른다.

## 공통 규칙 (안 지키면 게임에서 깨진다)

1. **파일 이름 · 픽셀 크기 · 알파(외곽선)는 지금 조각과 완전히 같게.** 게임은 이미 이 조각으로 붙어 있고, 파일만 바꿔 끼운다. 생성한 질감을 지금 PNG의 알파로 잘라 내는 방식 권장. 투명 창(컷인 프레임의 카드 아트 창 등)도 그대로.
2. **9-slice 조각은 가운데 · 늘어나는 변이 좌우(또는 상하) 반복 가능하게.** 눈에 띄는 무늬는 고정 모서리 칸 안에만. 경계값은 각 `layout.json`의 `nine_slice_px` 그대로.
3. 글자 · 숫자 · 화살표 · 캐릭터 · 카드 아트를 그림에 넣지 않는다 (지금과 같음).
4. 질감은 **약하게** — 그 위에 글자가 얹혀 바로 읽혀야 한다.
5. **1~2px 선 · 마름모 · 눈금 · 섬광 같은 단색 장식은 지금 픽셀 그대로 둔다** (다시 그릴 필요 없음, 복사만). 다시 그리는 것은 아래 표의 "다시 그림" 조각뿐.
6. 질감 · 색 기준은 각 UI의 **목업 PNG** (아래 표). 지금 조각이 목업과 색이 다르면 목업을 따른다.

## 1. 스킬 사용 배너 (유니크 이상 · 파워 카드 사용 시 화면 위쪽에 촤르륵 나오는 띠)

- 지금 조각: `../BattleFxMockup/Extracted/CutIn/` (게임: `Assets/Resources/BattleFx/CutIn/`)
- 목업: `../BattleFxMockup/BattleFx_CutIn_Mockup.png` (`전술 재장전` / `저격` 띠)

| 다시 그림 | 크기 (2배) | 표면 |
|---|---|---|
| `Frame_Power.png` · `Frame_Unique.png` | 2280×400 | 먹색 잉크 판. 카드 아트 창(투명 다각형)은 그대로 비워 둔다. Unique 이중 테두리 유지 |
| `Label_Plate_Power.png` · `Label_Plate_Unique.png` | 848×64 | 목업의 라벨 판 질감 |
| `Chip_Power.png` · `Chip_Unique.png` | 272×48 | 먹색 / 청록 칩 — 인쇄된 잉크 느낌 |

그대로: `Line` · `Diamond` · `Ticks_Unique`

## 2. 전투 종료 (승리 / 패배 띠 + 결과 판)

- 지금 조각: `../BattleFxMockup/Extracted/Victory/` · `Defeat/` · `Result/` (게임: `Assets/Resources/BattleFx/` 같은 이름 폴더)
- 목업: `BattleFx_Victory_Mockup.png` (`작전 종료` 띠), `BattleFx_Defeat_Mockup.png`, `Result_Clear_Mockup.png` · `Result_Defeat_Mockup.png` (`런 클리어` 판)

| 다시 그림 | 크기 (2배) | 표면 |
|---|---|---|
| `Victory/Band.png` | 3840×440, 9-slice 좌우 192 | 흰 종이 띠 |
| `Victory/Tag.png` · `Defeat/Tag.png` | 784×56, 9-slice 좌우 48 | 먹색 잉크 |
| `Defeat/Band.png` | 3840×440, 9-slice 좌우 192 | 먹색 잉크 띠 |
| `Result/Panel.png` | 2228×1312, 9-slice 168/160/168/160 | 흰 종이 판 (오프셋 그림자 · 모서리 십자 장식은 지금 위치 그대로) |
| `Result/Button_Return_Normal` · `_Hover` · `_Pressed.png` | 780×172, 9-slice 좌 60 / 우 64 | 먹색 잉크 버튼, 상태별 밝기 차이 유지 · 청록 아래 선 유지 |

그대로: 선(`Line_*`, `Label_Line`, `Title_Line`, `Rule_*`, `Stat_Divider`), `Slash*`, `Diamond`, `Flash`, `Accent_*`

## 3. 동료 컷인 대사 태그 (컷인 때 사선 따라 붙는 이름 · 대사 띠)

- 지금 조각: `../CompanionSkillMockup/Extracted/` (게임: `Assets/Resources/CompanionSkill/`)
- 목업: `../CompanionSkillMockup/CompanionSkill_CutIn_Mockup.png` · `_Enhanced_Mockup.png`

| 다시 그림 | 크기 (2배) | 표면 |
|---|---|---|
| `Tag_Back.png` | 1140×88, 9-slice 52/6/52/6 | 먹색 잉크 띠 (왼쪽 청록 띠 유지) |
| `Tag_Back_Enhanced.png` | 1140×88, 같은 경계 | 위 + 청록 헤어라인 유지 |

그대로: `Tag_Divider`, 컷인 마스크 · 테두리 · 눈매 등 나머지 전부

## 결과물

UI마다 `Extracted_v2/` 폴더를 지금 폴더 옆에 만든다:
`../BattleFxMockup/Extracted_v2/{CutIn,Victory,Defeat,Result}/`, `../CompanionSkillMockup/Extracted_v2/`
- 폴더마다 **지금 폴더의 PNG 전부** (다시 그린 것 + 그대로 복사한 것, 같은 이름) — 폴더째 바꿔 끼울 수 있게
- `Contact_Sheet.jpg`: 다시 그린 조각 전/후 나란히 + 조립 화면 (9-slice 조각은 늘린 모습 포함)
- `README.md`: 어느 조각을 다시 그렸고 어느 것을 복사했는지, 알파 일치 검사 결과
- `layout.json`은 다시 만들지 않는다 (지금 것을 그대로 쓴다)
