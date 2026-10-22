# 동료 탭 v2 · 조각 요청

`Companion_List_Mockup_v2.png`, `Companion_Detail_Mockup_v2.png` 확정 (회백 기하 면 + 그림자, 명일방주 결). 아래만 고쳐서 조각을 나눈다. 글자는 굽지 않는다(게임 TMP). SVG 원본, 2배 PNG, 판은 9-slice.

## 고칠 것

| # | 화면 | 고칠 것 |
|---|---|---|
| 1 | 목록 | 오른쪽 아래 페이지 표시 `‹ 01 / 02 ›` 삭제 (한 장에 다 들어간다, 넘치면 세로 스크롤) |
| 2 | 상세 | 대사 아래 `▶ VOICE / 01` → 음성이 없다. 작은 `‹ ›`로 대사 넘기기 표시로 바꾼다 (`LINE 01 / 03`) |
| 3 | 상세 | 단계별 해금: **매 레벨마다 기록 1편**이 열린다. 줄마다 `기록 n` + 그 레벨의 추가 해금(카드·스킬 강화)을 같이 적는 형식 — 예 `Lv.3  동료 기록 III · 동료 카드` |

## 조각 `Extracted_Draft/`

| 파일 | 내용 |
|---|---|
| `List_BG.png` (1920×1080) | 목록 바탕: 회백 기하 면·그림자·십자 표식만. 카드·글자·제목 판 없이 |
| `Detail_BG.png` (1920×1080) | 상세 바탕: 뒤쪽 회색·먹색 기하 판(캐릭터 뒤 큰 판들)까지. **캐릭터·정보 판·글자 없이** |
| `Card_Frame`, `Card_Frame_Hover`(청록 테두리), `Card_Frame_Locked`, `Card_Frame_Empty`(빈 칸, 가운데 +) | 목록 카드 (약 225×335) |
| `Card_NameBand` | 카드 아래 먹색 띠 |
| `Pip_Empty`, `Pip_Filled` | 호감도 마름모 |
| `SortTab_Underline` | 정렬 탭 선택 밑줄 (청록) |
| `Panel_Title`, `Button_Lobby` | 제목 판(04 동료), ← 로비 |
| `Panel_Section` | 오른쪽 정보 판 (먹색 머리 띠 + 회백 몸통, 왼쪽 청록 표식) — 9-slice, 머리 띠 높이 고정 |
| `Bond_Seg_Empty`, `Bond_Seg_Filled` | 호감도 막대 칸 (사선 평행사변형) |
| `Skill_Slot` | 스킬 아이콘 칸 |
| `Tag_Small` | `최대 호감도 시 강화` 작은 태그 판 |
| `CardThumb_Frame`, `CardThumb_Locked` | 동료 카드 썸네일 칸 |
| `Unlock_Mark`, `Unlock_Lock` | 해금 줄 앞 청록 마름모 / 자물쇠 |
| `Panel_NameBlock` | 왼쪽 아래 이름 판 (청록 밑줄 포함) |
| `Arrow_Prev`, `Arrow_Next` | 좌우 화살표 |
| `Button_Record`, `Button_Camp` | 기록 보기(회백) / 베이스캠프에서 만나기(먹색) |
| `layout.json` | 두 화면의 모든 rect, 캐릭터 전신 rect·피벗(발끝 기준), 글자 상자(크기·색·정렬·자간) |

- 캐릭터 전신은 게임에서 동료마다 넣는다. 목업의 치하라 쇼 전신 그림 원본이 따로 있으면 `Chara_ChiharaSho_Full.png`(투명 배경)으로 같이.
