# 마테리얼 UI 분절

정본1672×941 두 장을 기준으로 분절·복원했습니다. 글자·숫자·초상·실루엣·적 그림은 포함하지 않았습니다. 판 내부는 원본의 깨끗한 종이 여백에서 추출한 질감으로 채웠고, 테두리·그림자·상태·기호는 정본 형태로 재구성했습니다. 단순 사각 크롭이 아닙니다. Base의 바깥 로비 배경은 원본 그대로 유지했습니다. 모든 PNG 알파는0/255입니다.

## 새 조각과 이유

- Base_Material: 분류/4열 격자/읽기 영역 구조가 훈련장과 달라 전용 바탕. 머리/구역 구분선만 포함하며 탭·칸·서류·대상 머리는 분리했습니다.
- CategoryTab3상태: 두 언어를 넣는 큰 잘린 분류 판. Selected는먹색+왼쪽청록띠입니다.
- SubjectCell4상태: 초상 창은 투명, 이름·점 자리 종이는 포함. Hover/Selected 청록 테두리이며 표시점은 별도입니다.
- SubjectHeader: 원본에 독립 테두리가 있어 별도 출력했습니다. 큰 그림 창 투명, 정보 자리 빈 종이입니다.
- Badge_Affiliation: 동료 소속 청록 칸, 가로9-slice 가능. ProgressCell Filled/Empty: 원본의 직사각 진행 칸으로 마름모 기록 점과 구분합니다.
- EntryTab5상태: 글자·잠금 기호 제외. LockedSelected는회색+청록밑줄입니다.
- Icon_LockTab18×24, Icon_LockLarge62×80: 기존 작은 격자 잠금과 용도·규격이 달라 새로 복원했습니다.
- Document_Paper: 접힌 모서리·불투명 그림자 포함. 제목/본문/괘선/클립은 없습니다. Document_Rule와 Document_Clip은별도입니다.

## 재사용 (다시 출력하지 않음)

- LoadoutMockup/Extracted/Back_Normal, Back_Normal_Hover, Icon_BackArrow.
- ArmoryMockup/CardCatalogExtracted/Checkbox_Unchecked/Selected 및_Hover.
- TrainingMockup/Extracted/TypeBadge_Normal/Elite/Boss.
- TrainingMockup/Extracted/DefeatPip_Filled/Empty: 칸 아래 기록점5개.
- TrainingMockup/Extracted/Scrollbar_Track/Handle: 격자와본문용으로길이조절.
- TrainingMockup/Extracted/Icon_Lock: 미확인격자칸용. 밝은색으로tint합니다.

기존 파일은 같은 문법이므로 재사용하며 존재를 확인했습니다. reuse 경로는 이 Extracted 폴더 기준입니다. 기존 분절본을 수정하지 않았습니다.

## 조립·좌표

layout.json 좌표계는1672×941 좌상단 원점입니다. source_crop_xywh는원본 참고 영역, placement_1672_xy는배치, output_size는PNG규격입니다. portrait_window_xywh는각 칸 내부의 완전투명영역이고 text_slots_local는글자위치입니다. 창 안에 게임 그림을 마스크해 넣습니다.

격자는4열14칸, 마지막줄2칸입니다. 두 정본 하단틀에 미세한 차이가 있어 동료 화면의 정규 간격으로 통일했습니다. repeats에는격자/편탭/진행칸/기록점/분류탭 간격을 기록했습니다. text_screen에는섹션명·개수·필터·대상정보·해금안내 자리가 있습니다.

Document_Paper의 document_regions_local와 최상위 document_regions_screen에는문서번호/제목/괘선/본문스크롤/스크롤바/출처/잠김문구 영역이 있습니다. 잠긴3편은 EntryTab_LockedSelected+Icon_LockTab을 쓰고본문 대신 Icon_LockLarge와런타임조건을 표시합니다. 기록2/5와선택3편을혼동하지 않습니다.

nine_slice_lbrt는Left,Bottom,Right,Top입니다. 초상 창을 가진 칸과대상머리는지정크기를권장합니다. 크게늘리면창도변하므로게임의그림마스크를함께맞추십시오. 서류는28px경계로접힘/그림자를보존하며클립은별도배치합니다. 기호/배경은Simple입니다.

## 확인·재생성

python ArtDirection/MaterialMockup/Split_Handoff/split_material.py

Contact_Sheet.jpg는1200px폭한장입니다. 크기·알파·크롭범위·투명창·상태동일크기·재사용파일존재를자동검증했습니다. 원본목업과Unity importer/.meta는수정하지않았습니다.
