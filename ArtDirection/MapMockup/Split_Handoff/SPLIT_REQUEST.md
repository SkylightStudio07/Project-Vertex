# 맵 v1 분절 + 1막 지형 바탕 요청

정본: `../Map_Mockup_v1.png` (시작 쪽), `../Map_Mockup_v1_Boss.png` (보스 쪽). 둘 다 1920×1080. 보기용: `Preview_Start.jpg`, `Preview_Boss.jpg` (960×540).

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 위 미리보기 2장까지만. 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 확인은 1200폭 이하 확인용 시트 1장(`../Extracted/Contact_Sheet.jpg`)으로.

## 1. 판·노드 분절 (정본에서 추출·복원)

- **글자·숫자 전부 제외:** "01", "1막", 막 이름, "OPERATION MAP", "BOSS · 남은 층 12", 층 번호, "HERE", "QUEST", "BOSS" 이름표, 범례 이름, 가장자리 마이크로 카피.
- **노드 아이콘 그림은 빼고 칸만.** 기존 `Assets/Art/Maps/Nodes` 아이콘을 게임에서 얹는다.
- **지형 바탕·노드·선은 서로 떼어 낸다.** 판 조각에는 지형이 남지 않게, 스크롤 영역 안쪽은 비운 판으로 복원한다.
- 판은 알파 0/255, 종이 질감 유지. 기호는 판과 따로 투명 PNG.

| 조각 | 설명 | 상태 |
|---|---|---|
| `Map_Panel` | 지도 판 전체 (제목 줄·가로 구분선·모서리 "+" 포함, 스크롤 영역은 빈 종이) 9-slice | — |
| `Map_FloorTick` | 층 눈금 한 칸 (눈금선 + 아래 짧은 꼭지) | Past / Current(청록 칸) / Future / Boss |
| `Map_FloorRuler` | 눈금 띠 가로선 (반복) | — |
| `Map_LegendBar` | 범례 띠 (구분 헤어라인 포함, 아이콘·글자 없음) 9-slice | — |
| `Map_ScrollBar` | 아래 작은 스크롤 막대 트랙 · 손잡이 | — |
| `Node_Frame_Normal` | 종이 칸 | Locked(회색) / Accessible(청록 꺾쇠) / Visited / Current |
| `Node_Frame_Rest` | 아래 청록 띠 칸 | Locked / Accessible / Visited |
| `Node_Frame_Elite` | 먹색 칸 | Locked / Accessible / Visited |
| `Node_Frame_Boss` | 큰 먹색 칸 + 이중 테두리 + 아래 이름표 띠 | Locked / Accessible |
| `Node_Frame_Blessing` | 원형 헤어라인 | — |
| `Node_HereMarker` | 청록 마름모 | — |
| `Node_VisitedCheck` | 원형 체크 | — |
| `Node_QuestTag` | 먹색 작은 꼬리표 (글자 없음) | — |
| `Line_Solid` · `Line_Dashed` | 연결선 조각 (실선 2px, 점선 1.5px/간격 6), 흰 바탕 기준 흑연 — 청록은 게임에서 색만 바꾼다 | — |
| `Button_Close` | 닫기 칸 | Normal / Hover / Pressed |
| `Icon_Triangle` | "남은 층" 옆 ▶ | — |

## 2. 1막 지형 바탕 — 새로 그린다 (추출 아님)

목업에는 지형이 일부만 보이므로 **전체 폭을 새로 생성**한다.

- `Terrain_Act1.png` **3320 × 600**, 배경 투명 또는 판 종이색 #EEF0F2 (둘 중 하나로 정하고 README에 적기).
- 정본 두 장의 지형 표현을 그대로 이어서: **위에서 내려다본 측량도**, 무너진 담장 터·회랑 외곽선, 물길과 작은 다리, 피안화 점 묶음, 원형 기둥 터. 원근 건물 금지.
- **진하기:** 정본보다 조금 더 옅게 — 종이 대비 명도 차 5~8%. 물길의 옅은 하늘색은 지금 정도 유지(#DCE6EE 수준), 그 이상 채도 금지.
- **배치:** 노드·선이 없는 상태로 그린다. x 0~160(시작 여백)은 축복 노드 자리라 비교적 비워 두고, **x 3060~3320 보스 자리에 정본 보스 컷의 원형 광장**을 두며 이 부분만 10~12%로 조금 진하게.
- 위아래 끝(y 0~20, 580~600)은 판 경계라 지형이 끊겨도 됨.
- 글자 없음.

## 결과물

- `../Extracted/` 조각 PNG + `Terrain_Act1.png`, `layout.json` (원본 크롭 좌표 · 1920×1080 배치 좌표 · 9-slice · 노드 칸 크기), `README.md`, `Contact_Sheet.jpg`
