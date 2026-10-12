# 훈련장 동료 편성 v2 분절

이번 결과는 Extracted_v2에만 저장했습니다. 1차 Extracted는 수정하거나 재생성하지 않았습니다. 판은 원본의 깨끗한 종이 여백 질감으로 복원하고 윤곽·상태를 재구성했습니다. 글자·초상·실루엣·카드 그림은 제외했습니다. 모든 알파는0/255입니다.

## 새 조각

Base_Companion은 오른쪽 선택/동행 구역과 필터 구성이 달라 새로 만들었습니다. 바깥 로비 배경은 원본을 유지하고 내부는 무문자 종이로 복원했습니다.

PortraitCell4상태는 초상 창이 투명하고 이름/상태 띠는 종이입니다. PartyBadge는 오른쪽 위에 얹는 글자 없는 청록 칸입니다. 큰 초상은 DetailPortrait_Frame과 기존 그림을 합칩니다. DetailCardSlot은 카드 아트 창이 투명하며 카드 이름/에너지 표시는 게임에서 얹습니다.

PartySlot Filled/Hover는 초상 창이 투명, Empty는 빈 종이+점선입니다. Button_Remove3상태는 요청대로×기호를 포함합니다. Icon_Plus는 별도입니다.

메인 CompanionSlot_Empty2상태는 기존 CompanionCard와 정확히 같은162×221입니다. +와 동료 편성 글자는 별도로 넣습니다. Button_Formation3상태에는 글자/화살표가 없습니다.

## 재사용 — 새로 출력하지 않음

- Extracted/Base_Main: 메인 동료 구역 경계가 동일하므로 재사용. Base_Main_v2 없음.
- Extracted/CompanionCard_* + CompanionTag: 메인의 두 동료 모두 왼쪽 아래 동일 위치에 동행 태그를 놓습니다. 목업의 이카루스 오른쪽 위 태그는 사용하지 않습니다.
- Extracted/Button_Done_*, Scrollbar_*, Icon_Lock, DefeatPip_Filled, Icon_Arrow.
- ArmoryMockup/CardCatalogExtracted/Checkbox_*.
- LoadoutMockup/Extracted/Back_Normal, Back_Normal_Hover, Icon_BackArrow.
- SanctuaryMockup/Extracted/Detail_AffinityCell_Empty/Filled: 같은 직사각 문법이라 재사용, 화면상33×26으로 표시합니다.
- SanctuaryMockup/Extracted/Name_AccentLine: 흰색을 캐릭터 색으로tint합니다.

layout.json의 reuse 경로는 이 Extracted_v2 폴더 기준이며 모두 존재를 확인했습니다. 기존 완료 버튼은 편성 화면 완료 위치(약1158,789)에 맞춰 배치하며 실제 목업과 작은 폭 차이는9-slice로 조절합니다.

## 조립·좌표

모든 배치는1672×941 좌상단 기준입니다. source_crop_xywh는 참고한 원본 영역, placement_1672_xy는 정본 배치, output_size는 출력 PNG 규격입니다. repeats에는6열14칸의 격자 간격과3개 동행 칸 간격을 기록했습니다. 초상 격자는14칸만 생성하고 나머지4자리는 빈 배경입니다.

portrait_window_xywh/art_window_xywh는 조각 내부의 투명 창입니다. text_slots_local는 이름/상태/동행 이름/버튼 글자 위치이며 text_screen_companion/main에는 섹션 제목·수치·상세 정보·완료 위치가 있습니다. 글자는 모두 런타임입니다. 큰 초상과 카드 창의 화면 영역도 별도로 기록했습니다.

nine_slice_lbrt는Left,Bottom,Right,Top입니다. 내부 투명 창이 있는 판은지정 크기 사용을 권장합니다. 9-slice로 전체를 늘리면 초상 창도 변하므로 필요하면 그림과 창 좌표를 함께 조절하십시오. 기호·배경은Simple로 사용합니다.

## 확인·재생성

python ArtDirection/TrainingMockup/Split_Handoff/split_training_v2.py

Contact_Sheet.jpg는1200px 폭 한 장입니다. 크기·크롭 범위·알파·투명 창·상태 동일 크기·재사용 파일 존재를 자동 확인합니다. Unity importer/.meta는 수정하지 않았습니다.
