# 마테리얼(적·동료 기록) 어두운 판 · 조각 교체 요청서

아카이브(기록관) 입구에서 **적 기록 / 동료 기록**을 고르면 이 화면이 열린다. 지금은 로비 시설과 같은 흰 종이 판이라 아카이브의 어두운 서재·진홍 분위기와 끊긴다. **배치·크기는 그대로 두고, 그림 조각만 아카이브 테마로 다시 그린다.** 게임에서는 같은 이름의 파일을 1:1로 바꿔 끼운다.

- 참고
  - `Reference_Material_Enemy.png`, `Reference_Material_Companion.png`: 지금 화면 (적 탭 / 동료 탭). **배치 기준.**
  - `Reference_Archive_Hub.png`: 아카이브 입구(B안). **색·판 기준.**
  - `Reference_Archive_Panel_Menu.png`, `Reference_Archive_Panel_Dialogue.png`: 아카이브 판 조각 (어두운 반투명 먹색 + 가는 진홍 테두리 + 절단 모서리).
  - `Current_Sprites/`: 지금 화면이 쓰는 조각 36장. **이것을 하나하나 어두운 판으로 다시 그린다.**
  - 배경 재료: `../ArchiveHubMockup/Source_BG.png` (보라 서재).

## 0. 색

| 쓰임 | 색 |
|---|---|
| 판 | 먹색 #16181B, 80~88% 불투명 (아카이브 메뉴 판과 같게) |
| 판 테두리 | 회보라 #77717E 가는 선 / 강조 진홍 #B3263A |
| 글자(게임 TMP) | 회백 #EEF0F2, 보조 #B8B1BF |
| 강조 (선택·채움·호버) | 진홍 #B3263A, 호버 #D43A50 |

- 지금 청록(#0DB8F2)으로 된 곳은 **전부 진홍**으로 바꾼다 (선택 탭, 기록 진행 칸 채움, 격퇴 마름모 채움, 선택 칸 테두리).
- 종이 질감 대신 아주 옅은 먼지·결 정도.

## 1. 조각 목록 — 파일명·픽셀 크기·9-slice 테두리 **그대로**

| 파일 | 지금 | 새 판 |
|---|---|---|
| `Base_Material.png` (1672×941) | 흰 종이 바탕 + 칸 나눔 | `Source_BG`를 1672×941로 깔고 어둡게(40~50%) + 칸 나눔 자리에 어두운 반투명 판. 왼쪽 위 제목 판도 아카이브 제목 판 모양 |
| `Back_Normal`, `Back_Normal_Hover` | 흰 버튼 | 아카이브 `← 로비` 버튼 모양 (호버는 진홍 테두리) |
| `CategoryTab_Normal / _Hover / _Selected` | 적·동료 분류 탭 | 보통: 어두운 판 / 호버: 테두리 밝게 / 선택: 진홍 판 |
| `Checkbox_Unchecked / _Hover / _Selected / _Selected_Hover` | 필터 체크 | 회보라 테두리 / 선택은 진홍 채움 + 흰 체크 |
| `SubjectCell_Normal / _Hover / _Selected / _Unknown` | 대상 칸 | 어두운 판 / 호버 밝게 / 선택 진홍 테두리 / 미확인은 더 어둡게 |
| `Icon_Lock`, `Icon_LockTab`, `Icon_LockLarge` | 자물쇠 | 회백 선화 |
| `DefeatPip_Empty / _Filled` | 격퇴 마름모 | 빈 것 회보라 선 / 채운 것 진홍 |
| `Scrollbar_Track`, `Scrollbar_Handle` | 스크롤바 | 어두운 선 / 회보라 손잡이 |
| `SubjectHeader` | 대상 머리 판 | 어두운 판 |
| `TypeBadge_Normal / _Elite / _Boss` | 등급 배지 | 일반 회보라 / 엘리트 진홍 테두리 / 보스 진홍 채움 |
| `Badge_Affiliation` | 소속 배지 | 진홍 채움 |
| `ProgressCell_Empty / _Filled` | 기록 진행 칸 | 빈 것 어두운 칸 / 채운 것 진홍 |
| `EntryTab_Normal / _Hover / _Selected / _Locked / _LockedSelected` | 01~05 색인 탭 | 어두운 판 / 선택 진홍 밑줄·테두리 / 잠김 더 어둡게 |
| `Document_Paper`, `Document_Rule` | 서류 한 장 | 오래된 어두운 양피지 느낌 판(먹색 위 아주 옅은 갈보라) + 회보라 줄 |
| `Document_Clip` | 클립 | 진홍 리본 끈 또는 붉은 나비 핀 |

- 결과물: `Extracted_Dark/` 에 위 파일명 그대로 + 목업 `Material_Dark_Enemy.png`, `Material_Dark_Companion.png` (지금 화면 두 장에 새 조각을 끼운 모습).
- 글자는 굽지 않는다. 제목 글자(`05 마테리얼`)는 게임에서 `적 기록` / `동료 기록`으로 바꾼다.
- 나비 장식을 넣고 싶으면 `Base_Material` 배경 구석에 한두 마리(작게)까지만.
