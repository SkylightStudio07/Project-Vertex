# UI 목업 → 분절 → 게임 적용 작업 방식

GPT 이미지 에이전트(아스트라)에게 요청서를 주고 **완성 화면 목업**을 받은 뒤, 같은 그림에서 **게임용 조각**을 잘라 받아 Unity에 붙이는 흐름이다.
전투 결과 v3(`RasterRedo_v2/Direction_Mockups` → `Direction_Split`)가 이 방식으로 잘 된 사례다.

## 한눈에 보기

```
1. 현행 캡처 · 참조 모으기
2. 목업 요청서 (REQUEST.md)          → 아스트라: 완성 화면 목업 PNG
3. 목업 검토 · 확정                    (마음에 들 때까지 2로)
4. 분절 요청서 (SPLIT_REQUEST.md)    → 아스트라: 조각 PNG + layout.json + Contact_Sheet
5. 조각 검수
6. Resources로 복사 · 임포트 설정
7. 코드로 조립 (layout.json 좌표 그대로)
8. 플레이 모드 캡처로 확인 → 테스트 → 커밋
```

목업과 분절은 **반드시 두 단계로 나눈다.** 한 번에 "디자인하고 잘라 줘"라고 하면 목업 품질이 떨어지거나, 기존 조각에 질감만 다시 입혀 온다.

## 폴더

```
ArtDirection/<화면>Mockup/
  REQUEST.md              목업 요청서
  <이름>_Mockup.png       확정 목업 (정본)
  PROMPTS.txt             아스트라가 실제로 쓴 생성 프롬프트
  SPLIT_REQUEST.md        분절 요청서
  Extracted/              조각 납품 (PNG + layout.json + Contact_Sheet.jpg + README.md)
Assets/Resources/<UI>/    게임이 읽는 조각 (Extracted에서 PNG만 복사)
```

새 디자인으로 갈아엎을 때는 기존 조각을 덮어쓰지 말고 새 폴더(`Resources/BattleFx/V3/` 등)에 넣는다. 예전 판으로 되돌리기 쉽다.

## 1~3. 목업 요청서

### 넣을 것

- **무엇을 바꾸고 무엇을 그대로 두는지.** 예: "평소 대화창은 지금 것 그대로, 선택지 순간만 새로."
- **기준 그림**: 디자인 언어를 따올 목업(로비, 결과 판 등)과 지금 게임 캡처. 경로로 적는다.
- **장면 구성**: 배경 · 캐릭터 · 어두운 덮개 · 위치. 지금 게임 동작과 같게.
- **가져올 디자인 요소**를 구체적으로. 예: 옅은 회청 기하 면, 사선 제도선, 작은 정합 십자, 잘린 먹색 모서리, 딱딱한 오프셋 그림자, 짧은 청록 막대, 작은 번호 + 넓게 띄운 영문 소제목 + 굵은 한글.
- **금지 목록**: 네온 글로 · 금색 · 그런지 · 빽빽한 회로무늬 · SVG · 도형 조립 · 에셋 시트.
- **상태가 있으면 장마다 같은 장면**으로: Default / Hover / Selected는 배치 · 어둡기 · 판 모양이 같고 상태만 달라야 한다.
- 결과물: 1920×1080 완성 화면 PNG + `PROMPTS.txt` + `README.md`.

### 검토할 것

- 판 안이 "빈 흰 사각형 + 노이즈"가 아니라 안쪽 구성이 있는가.
- 장마다 어둡기 · 판 디테일이 흔들리지 않는가.
- 캐릭터 가장자리 흰 테두리(프린지), 글자 겹침.
- 아스트라가 **고치기 전 요청서**를 보고 작업하지 않았는가 (요청서를 고쳤으면 고쳤다고 따로 알린다).

## 4. 분절 요청서

### 핵심 규칙 (요청서에 그대로 적는다)

1. **표면은 이미지 모델로 그린 그림.** 목업의 종이 · 잉크 질감, 기하 면, 문양을 살린다. 정본을 같은 프롬프트로 고해상도(또는 조각 단독)로 재생성해 잘라 내는 방식 권장.
2. **SVG를 요구하지 않는다.** "SVG 원본 + PNG"를 달라고 하면 질감 없는 단색 도형으로 다시 그려 온다. 1~2px 선 · 작은 십자 · 짧은 막대 같은 단색 장식만 단색으로 그려도 된다고 허락한다.
3. **글자 · 숫자 · 화살표 글리프 · 캐릭터 · 카드 아트 · 배경을 조각에 넣지 않는다.** 바뀌는 값(층 번호, 적 수, 이름)은 전부 게임이 쓴다.
4. **2배 PNG**(화면 1px = PNG 2px), 투명 배경, **회전 0°**(목업에서 기울어 있어도 게임이 돌린다).
5. 외곽은 **이진 알파**(0/255). 문양 · 그라데이션 덮개만 부드러운 알파 허용 — 표에 `soft`로 표시.
6. 판 · 그림자 · 문양 · 덮개 · 테두리를 **나눠** 받는다. 각각 따로 움직이게 하려고(판이 밀려 들어오고 문양이 늦게 떠오르는 식).
7. 늘어나야 하는 조각만 **9-slice**. 눈에 띄는 무늬는 고정 모서리 칸에만, 가운데는 반복 가능한 질감만. 나머지는 **고정 크기**.
8. 기존 조각을 바꿔 끼우는 경우 **파일 이름 · 크기 · 알파를 그대로** 맞추게 한다. 새 디자인이면 새로 잡아도 된다고 명시.
9. 목업에 없는 상태(Hover, 정예, 패배 등)는 **같은 장면의 확인용 목업을 먼저 한 장** 뽑게 한 뒤 그걸로 자르게 한다.

### 조각 표 형식

| 파일 | 내용 | 알파 |
|---|---|---|
| `Panel.png` | 흰 종이 판: 잘린 모서리, 안쪽 회청 기하 면, 제도선, 십자 | 이진 |
| `Emblem_Compass.png` | 크고 옅은 나침반 — **판 외곽으로 이미 잘라 둔 상태** | soft |
| `Strip_Mask.png` | 사진 띠의 **모양만**(흰색) — 게임이 실제 배경을 잘라 넣는다 | 이진 |

판 위에 겹치는 문양 · 덮개는 **판과 같은 캔버스 크기 · 같은 위치**로 받으면 조립이 쉽다(위치 값 하나로 겹친다).

### `layout.json`에 받을 것

- 기준 1920×1080, 왼쪽 위 원점, `[x,y,w,h]` 화면 px (목업 해상도에서 환산).
- 조각마다 `file` · `png_size_px` · `screen_rect` · `alpha` · `nine_slice_px` · 그림자 불투명도.
- **`text_slots`**: 글자 자리마다 `[x,y,w,h]` · 정렬 · 크기(px) · 굵기 · 색(hex) · 자간(px).
- 판 너비가 바뀌는 경우 따라붙는 조각(꼬리 등)은 **판 모서리 기준 오프셋**으로.
- 아트 창 · 마스크가 있으면 다각형 꼭짓점(PNG px, 화면 px 둘 다).

### 결과물

`Extracted/` — PNG + `layout.json` + `Contact_Sheet.jpg`(조각 단독 + 정본 좌표로 다시 조립해 정본 옆에 나란히 + 9-slice는 늘린 모습) + `README.md`(SVG 미사용 확인, 생성 프롬프트, 검증 결과).

## 5. 검수

- `README.md`에 "geometric redraw", SVG 언급이 있으면 다시 요청한다.
- `Contact_Sheet` / `Reassembled` 이미지로 정본과 나란히 비교한다. **전/후 차이가 거의 없으면** 질감만 다시 입힌 것이다(새 디자인 요청이었다면 실패).
- 조각 단독 이미지에 글자가 들어가 있지 않은지.

## 6. Unity 임포트

`unity command eval_file`로 한 번에 설정한다 (일회성 편집용 .cs 파일을 프로젝트에 만들지 않는다).

- Sprite / Single / **PPU 200** / FullRect / Bilinear / 압축 없음 / 밉맵 없음 / NPOT None.
- 9-slice 경계는 `spriteBorder`에 **PNG px**(L, B, R, T)로.
- 캔버스 참조 PPU가 100이라, 2배 PNG를 PPU 200으로 넣으면 화면 크기가 맞는다.

## 7. 코드 조립

- layout.json 좌표를 그대로 쓴다. 앵커 화면 위 · 가운데, 피벗 왼쪽 위, `anchoredPosition = (x - 960, -y)` — 넓은 화면에서도 가운데 정렬. (`BattleOperationFx.Pic` · `ScreenPos` 참고)
- 자간: TMP `characterSpacing`은 em의 1/100 단위 → `자간px / 글자크기 × 100`.
- 문양을 가운데 기준으로 키우려면 피벗만 문양 중심으로 옮긴다(`PivotAt`).
- 아트 창 · 사진 띠는 `Mask`(showMaskGraphic 끔) 안에 그림을 넣고, 덮개 조각을 그 위에 얹는다.
- 씬에 깔린 예전 판을 바꿀 때는 코드로 새 판을 만들고 예전 자식을 끄는 방식 + 인스펙터 토글(`useV3`)로 되돌릴 수 있게.
- 함정: 코드로 `Button`을 붙이면 기본 색조 전환(비활성 = 반투명 회색)이 먼저 칠해진다. SpriteSwap으로 바꾼 뒤 `canvasRenderer.SetColor(Color.white)`.

## 8. 확인

- 플레이 모드에서 eval 스크립트로 연출을 직접 호출하고(`HandleVictory` 등, 리플렉션), `ScreenCapture`로 찍어 정본 목업과 비교한다.
- 사용자가 직접 플레이 중이면 멈추지 않는다. 씬을 바꾸기 전 dirty 여부를 확인한다.
- 사용자 테스트 후 커밋. Astra 작업 파일(.py, .cjs, .zip)은 커밋에서 뺀다.

## 지금까지 사례

| 화면 | 목업 | 조각 | 게임 |
|---|---|---|---|
| 승리 · 패배 · 결과 · 컷인 v3 | `RasterRedo_v2/Direction_Mockups/` · `BattleFxMockup/CutIn_Category_v2/` | `RasterRedo_v2/Direction_Split/` | `Resources/BattleFx/V3/` |
| 대화 선택지 v2 | `DialogueMockup/Choice_v2/` | `Choice_v1/Extracted_v2/` + `Choice_v2/Extracted/` | `Resources/DialogueChoice/` |
| 대화 선택지 v3 | `DialogueMockup/Redesign_v3/Choice_v3.png` | 요청 중 (`SPLIT_REQUEST.md`) | — |
| 작전 개시 v7 | `BattleFxMockup/StartEnd_Redesign_v2/Operation_Start_Mockup_v7.png` | 요청 중 (`SPLIT_REQUEST_Start.md`) | — |
| (실패 사례) 질감만 다시 입히기 | — | `BattleFxMockup/Extracted_v2/` | 쓰지 않음 |
