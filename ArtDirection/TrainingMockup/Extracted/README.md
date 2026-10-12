# 훈련장 분절

정본 두 장1672×941 기준입니다. 글자·숫자·썸네일·큰 적 그림·초상·실루엣·카드 아트·덱 아이콘은 제외했습니다. 판 내부는 원본의 깨끗한 머리 여백 종이 질감으로 복원했으며, 상태/기호/테두리는 원본을 참고해 재구성했습니다. 단순 크롭이 아닙니다. Base는 바깥 로비 배경을 보존하고 내부 판을 복원했습니다. 모든 PNG 알파는0/255입니다.

## 재사용 (이번에 출력하지 않음)

- 뒤로 버튼: ArtDirection/LoadoutMockup/Extracted/Back_Normal.png, Back_Normal_Hover.png, Icon_BackArrow.png.
- 필터 체크박스: ArtDirection/ArmoryMockup/CardCatalogExtracted/Checkbox_Unchecked, Checkbox_Selected 및 각_Hover.png.
- 카드 틀·잠긴 카드·에너지: 같은 CardCatalogExtracted의 Card_Attack_NoArt.png, Card_RewardLocked.png, Lock_Crop.png, Energy_Cyan.png, Energy_Gray.png.

파일 존재를 확인했습니다. 목업 카드에는 위 에너지 표기보다 오른쪽 수량 배지가 강조되어 있으며 기존 카드 틀과 세부 테두리/크기 차이가 있습니다. 기존 틀을 재사용하고 CountBadge를 추가합니다. 카드4열 배치 간격과 출력 칸 크기는 JSON library_grid에 기록했습니다. 그림은 기존 카드 아트입니다.

## 조각

Base_Main/Deck에는 구역 판·머리 장식·구분선만 있고 목록/버튼/초상/카드는 없습니다. Target_Backdrop은 분리된 과녁이며 전체 알파0.18 정도부터 조절합니다.

TargetRow는 빈 썸네일 바탕 포함4상태, TypeBadge는3종류, DefeatPip은Filled/Empty입니다. 상태별 크기와 로컬 좌표는 동일합니다. Selected는청록 테두리+왼쪽 띠입니다. 자물쇠와 스크롤은 별도입니다. AnalysisBadge는 원본에서 별도 판 없이 글자뿐이므로 생략했습니다. 옆 보스 칸은 TypeBadge_Boss를 사용합니다.

CompanionCard는4상태이며 초상 창은 완전 투명입니다. 선택 시 아래 청록 선을 포함하고 CompanionTag는 별도로 올립니다. 잠긴 실루엣은 게임에서 실제 그림을 회색으로 표시합니다.

DeckRow/EditRow는 카드 그림을 넣을 빈 아이콘 바탕 포함2상태입니다. EditRow의−/+기호·버튼은 포함하지 않았으며 Button_Minus/Plus를 별도로 올립니다. 이 작은 버튼만 기호를 포함하고 각각4상태입니다.

Button_DeckEdit3상태, Sortie4상태, Done3상태입니다. Sortie/Done은먹색+아래 청록 선입니다. Icon_Arrow는**흰색**이며 종이 버튼 위에서는 먹색 tint합니다. CostChip3상태와 CountBadge는숫자 없는 칸입니다.

## 좌표·크기

layout.json은 **1672×941, 좌상단 기준**입니다. source_crop_xywh는 원본 참고 영역, placement_1672_xy는 배치, output_size는 PNG크기입니다. text_slots_local는각 조각 안 글자/배지/점/수치/버튼 위치이며 sections_main/deck에는섹션 제목·개수, repeats에는목록·동료·카드 격자·코스트 칩 반복 간격이 있습니다. 줄의 실제 게임 텍스트 크기에 맞춰 마지막 정렬을 조정하십시오.

nine_slice_lbrt 순서는Left,Bottom,Right,Top입니다. 내부 썸네일/초상 창이 있는 줄·카드는지정 크기 사용을 권장합니다. 틀을 크게 늘리면 내부 창도 늘어날 수 있습니다. 과녁·기호·Base는Simple입니다. 원본 목업 치수에 맞춘 출력이며 초안 레이아웃 문서 치수와 다를 수 있습니다.

## 검증·재생성

python ArtDirection/TrainingMockup/Split_Handoff/split_training.py

Contact_Sheet.jpg 한 장(1200px 폭)에 모든 새 조각이 있습니다. 규격·알파·원본참고범위·초상투명·상태별크기를 검사합니다. Unity importer/.meta 및원본은 변경하지 않았습니다.
