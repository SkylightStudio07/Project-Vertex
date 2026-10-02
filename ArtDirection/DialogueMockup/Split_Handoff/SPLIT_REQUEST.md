# 대화 화면 분절 요청

정본: `../Dialogue_Story_Mockup.png` (스토리 대화), `../Dialogue_Overlay_Mockup.png` (하단 대사창). 둘 다 1920×1080. 보기용: `Preview_Story.jpg`, `Preview_Overlay.jpg` (960×540).

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 위 미리보기 2장까지만. 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 확인은 1200폭 이하 확인용 시트 1장(`../Extracted/Contact_Sheet.jpg`)으로.

## 분절 규칙

- **글자 전부 제외:** 이름, 소속("VERTEX / 협력자"), 대사, 선택지 번호·문장, 답변 문장.
- **캐릭터 스탠딩·초상·배경·상점 화면은 넣지 않는다.** 대화창·칸·기호만.
- **대화창 위쪽 그라데이션은 알파로 굽는다:** 위 120px 구간 맨 위 알파 0 → 아래 255, 계단 없이 부드럽게. 그 아래는 알파 255 종이색. 원본에서 배경이 비친 부분은 종이 질감으로 복원한다.
- 판·칸은 그라데이션 구간 외에 알파 0/255. 기호는 판과 따로 투명 PNG.
- 이름 아래 고유색 선은 **흰색**으로 뽑는다 (게임에서 캐릭터 색으로 tint).

## 조각 목록

| 조각 | 설명 | 상태 |
|---|---|---|
| `Dialogue_Box` | 대화창 1920×300 (위 그라데이션 포함, 왼쪽 위 마름모·모서리 장식 포함, 워터마크 제외). 가로로 늘려도 되게 양 끝 장식 영역을 9-slice 경계로 | — |
| `Dialogue_Watermark` | 오른쪽 나침반 선화 | — |
| `Name_AccentLine` | 이름 아래 선 (흰색) | — |
| `Advance_Diamond` · `Advance_Line` | 넘기기 표시 | — |
| `Portrait_Cell` | 하단 대사창 초상 칸 96×96 (테두리·모서리 장식) | — |
| `Choice_Panel` | 선택지 판 760×72 (9-slice) | Normal / Hover(청록 테두리) / Disabled |
| `Answer_Panel` | 답변 판 640×60 (9-slice) | Normal / Hover |
| `Icon_Arrow` · `NumberUnderline` | 선택지 기호 | — |

## 결과물

- `../Extracted/` 조각 PNG, `layout.json` (원본 크롭 좌표 · 1920×1080 배치 좌표 · 9-slice), `README.md`, `Contact_Sheet.jpg`
