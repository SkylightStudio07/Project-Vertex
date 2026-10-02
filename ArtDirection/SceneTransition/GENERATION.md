# 씬 전환 디자인 시안

최종 검토본: SceneTransition_ComponentStoryboard_v2.png. 열림 예시를 상단 왼쪽/하단 오른쪽 수평 슬라이드로 수정했다.

## 열림 수정 프롬프트

Precise-object-edit. Edit target is this approved scene transition component/storyboard sheet. Change ONLY storyboard C OPENING, bottom-left thumbnail. Currently it incorrectly shows a pale HORIZONTAL band from curtains moving vertically. Correct to HORIZONTAL SIDEWAYS sliding: top curtain still occupies TOP HALF (same y bounds) but is shifted to LEFT, exposing a pale neutral rectangle at RIGHT of top half; bottom curtain still occupies BOTTOM HALF but is shifted to RIGHT, exposing a pale neutral rectangle at LEFT of bottom half. Two pale openings at OPPOSITE CORNERS, a stepped staggered aperture, absolutely NO full-width horizontal pale middle band. Top dark panel extends from left edge to60% width; bottom dark panel extends from40% width to right edge. Exposed areas plain pale with subtle registration cross. Add small left arrow within top dark panel, right arrow in bottom. Retain matching leading diagonal edge accent where appropriate. All other panels, type, components, four thumbnail frames and sheet geometry unchanged. Same exact output dimensions.

내장 이미지 생성 모델(imagegen)로 생성. 스타일 참조: ../QuestBoardMockup/QuestBoard_Posted_Mockup_v1.png. 요청서: LAYOUT_BRIEF.md.
SceneTransition_ComponentStoryboard_v1.png는 구성 조각 및 닫힘/로딩/열림/귀환을 함께 검토하는 시트다. 시트에 표기한 조각 크기는 실제 게임에서 사용할 목표 규격이며, 시트 안의 그림 자체가 그 픽셀 크기로 제작된 것은 아니다. 개별 투명 PNG 및 layout.json은 후속 분절에서 작성한다. 막 면은 불투명 먹색, TitleFrame과 Emblem 주변은 분절 시 알파 처리한다.
움직임은 이미지의 개념 예시이며 게임 코드 변경 없음. 실제 조립 시 큰 제목과 y540 이음새의 충돌 여부를 픽셀 기준으로 조정해야 한다.

## 프롬프트

Use case: ui-mockup. Generate a polished SCENE TRANSITION COMPONENT + STORYBOARD SHEET for VERTEX. Landscape presentation board 2048x1536 if possible. Attached Quest Board is ONLY reference for precise clipped-corner tactical graphics, restrained cyan, thin registration lines; this new transition design uses OPAQUE near-black #090A0D to #16181B surfaces, white linework, cyan #0DB8F2 exclusively. NOT white-paper menus, not another quest board, no characters, no weapons. All black curtains are fully opaque, NOT transparent smoky panels. Extremely faint charcoal grid/hatching, minimal engineering finish.

Upper HALF: isolated labeled component samples with generous spacing on neutral graphite board background, organized practically:
1. two long broad curtain samples labeled UPPER CURTAIN 2320x548 and LOWER CURTAIN 2320x548. Both share coherent near-black paper/material texture. Upper leading LEFT edge has a 70px diagonal cut with TWO cyan diagonal edge stripes of thickness6 and2 separated10 and tiny white ruler ticks. Lower leading RIGHT edge mirrors this treatment. Opposite ends plain, no symmetric embellishment there. Seam-facing upper BOTTOM edge and lower TOP edge have faint dark-gray hairline inset12 and tiny spaced ticks. Tiny technical text VERTEX // FIELD OPS 01 in peripheral corner. Upper curtain only: very faint large V-outline watermark at lower RIGHT, barely 6% lighter than surface. Show both entire cut ends clearly.
2. seam samples: long base strip labeled Seam_Base 2320x24, muted DARK teal central6px line, 1px near-white upper/lower lines and tiny diamond ticks every200. Separate bright cyan Seam_Fill 2320x6 with a40px white leading highlight at RIGHT. Tiny optional head: cyan diamond with white center dot.
3. blank TitleFrame1200x300 sample: ONLY sparse white/cyan corner brackets, EMPTY center. No filled panel. Separate 160x160 operation emblem: balanced nearly rotationally symmetric white linework segmented compass diamond, restrained cyan tips. No letterform logo. LoadingChip280x44 opaque near-black with thin cyan outline, three cyan dots on left, empty label surface for runtime text. Actual alpha is later extraction; represent blank transparent portions against consistent board color, not checkered art.

LOWER HALF: FOUR equally-sized 16:9 storyboard thumbnails in TWO columns by TWO rows, clearly labeled outside frames:
A CLOSED / 출정: curtains fully cover view; central seam runs at EXACT relative y540 on1920x1080, overlapping curtain16px. Sparse corner TitleFrame spanning x360..1560,y390..690. Small cyan "OPERATION // DEPLOY" above large white Korean "출정", light gray "작전 지역으로 이동합니다" BELOW seam. Main large typography wholly ABOVE seam (bottom no lower than y528), no overlap with progress strip y528..552. White/cyan emblem SMALL, to left of title and not intersecting it. LoadingChip lower-right (1600,1000) with "NOW LOADING".
B LOADING: same closed layout, cyan progress fill about60% along seam with white leading diamond. Text stable. Subtle lower right three-dot activity.
C OPENING: upper curtain shifted LEFT and lower curtain shifted RIGHT (they move in opposite horizontal directions), prominent cyan leading diagonal edges visible. Revealed central area is plain pale neutral PLACEHOLDER with simple registration cross, no scenic illustration. Title sliding right and fading in concept only, curtains stay opaque. Do NOT separate curtains vertically; show horizontal sideways slide.
D RETURN / 귀환: same CLOSED curtains and cyan point color, replace text with "RETURN // BASE", large "귀환", small "기지로 복귀합니다". No new palette.

Practical crisp Korean Pretendard-like typography. Accurate fine lines. No red/yellow, no smoke, no neon sci-fi glow everywhere, no ornate Celtic shapes. Seam band must remain clear except its own fill/ticks. Peripheral engineering markings minimal. This sheet is a design concept with separate assets and sequence examples; retain consistent curtain design across all thumbnails. 
Detailed source brief:
# 씬 전환 막 · 조각 요청서

로비에서 **출정**을 누르면 화면을 덮었다가 전투 씬에서 다시 열리는 전환 막이다. 지금 게임에는 단색 도형으로 만든 임시 버전이 들어가 있다. 그 **배치와 움직임은 그대로 두고 겉모습만** 바꾸는 조각 시트를 요청한다.
스타일은 명일방주식 작전 화면을 따른다. 기준 시리즈는 `LobbyKit_v2`, `QuestBoardMockup`이다.

## 0. 공통

- 기준 화면은 **1920×1080**이다. 좌표는 모두 왼쪽 위 원점, 픽셀 단위다.
- 색은 세 가지만 쓴다.
  - 막 면: 먹색 **#090A0D ~ #16181B**
  - 선·글자: 흰색
  - 포인트: 청록 **#0DB8F2**
- 빨강·노랑은 쓰지 않는다. 반투명 검정도 쓰지 않는다. 막은 **완전 불투명**이어야 한다.
- 큰 글자는 게임에서 TMP(Pretendard)로 얹는다. 목업의 글자는 참고용이니, 분리할 때 지울 수 있게 단색 면 위에 둔다.
- 결과물:
  - 각 조각의 PNG(투명 배경)
  - 조립 예시 3장: 닫힌 상태, 로딩 중, 열리는 중
  - 좌표가 든 `layout.json`

## 1. 움직임 (참고용 — 코드가 처리한다)

```
닫힘 0.45초   위 막: 오른쪽 → 가운데      아래 막: 왼쪽 → 가운데 (0.06초 늦게)
              이음새 선이 가운데서 좌우로 그어짐 → 글자 페이드 인 (자간 넓게 → 좁게)
로딩 0.9초+   이음새 선 위로 청록 진행 막대가 왼쪽부터 차오름
열림 0.5초    글자가 오른쪽으로 빠지며 사라짐
              위 막: 가운데 → 왼쪽      아래 막: 가운데 → 오른쪽
              흰 섬광 한 번
```

이 움직임 때문에 **막의 좌우 끝(앞장서는 모서리)이 이동 중에 크게 보인다.** 이 끝부분 디자인이 제일 중요하다.

## 2. 위 막 · 아래 막

```
x=-200                                                       x=2120
┌───────────────────────────────────────────────────────────────┐ y=0
│                          위 막 (2320 × 548)                    │
│   · 먹색 면 + 아주 옅은 격자/사선 해칭 (알파 없이 색 차이로만)      │
│   · 한쪽 구석에 작은 기술 표기: "VERTEX // FIELD OPS 01" 류      │
│══════════════════════ 이음새 쪽 가장자리 ══════════════════════│ y=548
└───────────────────────────────────────────────────────────────┘
      (아래 막은 y=532~1080, 위아래를 뒤집은 짝. 가운데 16px 겹침)
```

- **크기:** 각 **2320 × 548**. 화면보다 좌우로 200씩 넓다.
- **좌우 끝:** 70px 기울어진 사선 컷이다. 위 막은 들어올 때 **왼쪽 끝**이 앞장서고, 아래 막은 **오른쪽 끝**이 앞장선다.
  - 앞장서는 끝에만 장식을 넣는다: 청록 사선 띠 2줄(굵기 6·2px, 간격 10), 흰 눈금 몇 개.
  - 반대쪽 끝은 민짜로 둔다.
- **이음새 쪽 가장자리:** 흰 선 1px(알파 없이 #2A2D31 정도)과 짧은 눈금 반복(40px 간격)을 넣는다. 가운데 이음새 선과 겹치지 않게, 가장자리에서 **12px 안쪽**에 둔다.
- **면 질감:** 판독이 안 될 정도로 옅은 격자, 또는 사선 해칭. 로비 키트의 종이 질감을 먹색 버전으로 바꾼 느낌이면 좋다.
- **로고:** 워터마크(예: 큰 `V` 엠블럼 윤곽)를 넣는다면 위 막 오른쪽 아래 한 곳에만 넣고, 먹색에서 명도 +6% 이내로 한다.
- 위 막과 아래 막은 서로 다른 그림이어도 된다. 다만 닫혔을 때 한 장처럼 이어져 보여야 한다.

## 3. 이음새 · 로딩 막대

화면 가운데 **y=540**을 지나는 가로 띠다.

| 조각 | 크기 | 설명 |
|---|---|---|
| `Seam_Base` | 2320 × 24 | 가운데 6px 청록 선을 흐린 버전(알파 25% 느낌을 **색으로** 표현)으로. 위아래에 1px 흰 가는 선, 200px마다 작은 마름모 눈금 |
| `Seam_Fill` | 2320 × 6 | 진행 막대. 청록 단색 + 오른쪽 끝(선두)에 짧은 흰 광택 40px. 게임에서 왼쪽 기준으로 가로 스케일한다 |
| `Seam_Head` | 48 × 24 | 진행 막대 선두에 따라다니는 작은 표식 (청록 마름모 + 흰 점). 없어도 됨 |

## 4. 가운데 글자 판

```
                x=360                                  x=1560
          y=390 ┌ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐
                   OPERATION  //  DEPLOY        ← 작은 영문 22pt, 청록, 자간 넓게 (y≈448)
                          출  정                ← 큰 글자 96pt, 흰색 (y≈506)
            ════════════════ 이음새 (y=540) ═════════════════
                   작전 지역으로 이동합니다        ← 22pt, 옅은 회색 (y≈580)
          y=690 └ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┘
```

- `TitleFrame`(1200 × 300, 투명 배경)을 요청한다. 네 모서리에 청록/흰 꺾쇠를 넣고, 글자 영역은 비워 둔다.
- 큰 글자 왼쪽 옆에 들어갈 **작전 엠블럼 `Emblem`**(160 × 160, 흰 선화 + 청록 포인트)도 필요하다. 로딩 중에 천천히 돌아가니 **회전 대칭**에 가까운 모양으로 한다. 위치는 대략 (560, 450) 중심.
- 이음새 선이 큰 글자와 작은 글자 사이를 지나간다. 이음새와 글자가 부딪히지 않는지 조립 예시에서 확인할 것.

## 5. 우하단 로딩 칩

| 조각 | 크기 | 위치 | 설명 |
|---|---|---|---|
| `LoadingChip` | 280 × 44 | (1600, 1000) | 먹색 칩 + 청록 테두리 1px. 왼쪽에 점 3개(게임에서 깜빡임), 글자 "NOW LOADING"은 TMP |

## 6. 변형 — 귀환 (전투 → 로비)

같은 막을 재사용하고 글자만 "RETURN // BASE · 귀환"으로 바꾼다. 포인트 색은 청록 그대로다.
따로 그릴 것은 없고, 조립 예시만 1장 더 부탁한다.

## 하지 말 것

- 막에 캐릭터 일러스트를 넣지 않는다. 기호·선·글자만 쓴다.
- 반투명 어두운 면을 쓰지 않는다. 이 프로젝트는 Linear 색공간이라 알파 0.8 검정이 회색으로 뜬다. 어둡게 할 곳은 불투명 색으로 칠한다.
- 이음새 y=540 ±12px 안에는 장식을 넣지 않는다 (게임의 진행 막대 자리).
- 큰 글자 영역(x 560~1360, y 460~560)에는 질감 외에 아무것도 넣지 않는다.

