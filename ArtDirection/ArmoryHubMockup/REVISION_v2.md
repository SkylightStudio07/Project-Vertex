# 무기고 v2 · 수정 + 조각 요청

입구(`Armory_Hub_Mockup_v1`)는 확정. 안쪽 두 장(`Armory_Equipment_Mockup_v1`, `Armory_Card_Mockup_v1`)은 **재화 체계가 바뀌어** 아래를 고쳐 v2로 다시 뽑는다. 재화 아이콘은 `../LobbyShopMockup/Currency_Icons/`(탈린·파츠·아티팩트) 그대로 쓴다.

## 1. 공통

| # | 고칠 것 |
|---|---|
| 1 | 머리: 세 장 모두 시설 번호 **03**. 입구 `03 │ 무기고`, 안쪽은 경로 `03 무기고 › 장비 강화` / `03 무기고 › 카드 강화` (같은 모양) |
| 2 | 오른쪽 위 `로비 경험치 50/100` 삭제 → 로비 재화 바처럼 **탈린 · 파츠 · 아티팩트** 보유 수 (아이콘 + 숫자) |
| 3 | 입구 캐릭터는 목업용. 게임에서는 `Source_Mechanic.png` 원본을 쓰므로 조각에 캐릭터 넣지 않는다 |

## 2. 장비 강화

| # | 고칠 것 |
|---|---|
| 1 | 강화 비용: `30 EXP` → **파츠 아이콘 + 10** (Lv.2), **파츠 20** (Lv.3). 보유 표시 `보유 36 → 강화 후 26` |
| 2 | 강화 단계 Lv.3 줄: `시설 Lv.2 필요` 유지 (마지막 단계 조건) |
| 3 | 기본 카드 강화 버튼 `강화 20 EXP` → **탈린 80 + 파츠 3** (카드 강화와 같은 값, 버튼 안에 아이콘 두 개) |
| 4 | 잠긴 무기: `해금 조건` → **아티팩트 아이콘 + 1** (새 무기는 아티팩트로 연다) |
| 5 | 비용이 모자랄 때 상태: 숫자 회색 + `강화` 버튼 회색 판 |

## 3. 카드 강화

| # | 고칠 것 |
|---|---|
| 1 | 상세 판 `무기고 강화 Lv.1 → Lv.2` 삭제 → **강화 비용** 줄: 탈린 80 + 파츠 3, 아래 `보유 탈린 1,240 · 파츠 36` |
| 2 | 이미 강화한 카드: 카드 칸 오른쪽 위 작은 청록 `+` 표식, 상세 버튼 자리 `강화 완료` (회색) |
| 3 | 강화할 수 없는 카드(상태이상 등): 버튼 `강화 불가` 회색 |
| 4 | 머리 번호 `03`은 1절대로 경로 표시로 |

## 4. 결과물

1. `Armory_Equipment_Mockup_v2.png`, `Armory_Card_Mockup_v2.png`
2. 세 장 조각 `Extracted_Draft/` + `layout.json` (SVG 원본, 2배 PNG, 9-slice, 글자 굽지 않음)
   - 공통: `Workshop_BG`(작업실 배경, 캐릭터·판 없이), `Panel_Title`, `Panel_TitlePath`, `Button_Back`, `Tab_Normal`, `Tab_Selected`, `Panel_Dark`(먹색 반투명 판), `Panel_Paper`(가운데 회백 판)
   - 입구: `Menu_Panel`, `Menu_Panel_Hover`, `Menu_Diamond`, `Menu_Diamond_Hover`, `Icon_Equipment`, `Icon_CardUpgrade`, `Panel_Dialogue`
   - 장비: `WeaponSlot`, `WeaponSlot_Selected`, `WeaponSlot_Locked`, `Step_Done`, `Step_Current`, `Step_Locked`, `Step_Line`, `Button_Upgrade`, `Button_Upgrade_Disabled`, `Button_Small`, `Row_BaseCard`
   - 카드: `Checkbox_Off`, `Checkbox_On`, `CardCell`, `CardCell_Selected`, `Mark_Upgraded`, `Panel_CardDetail`
