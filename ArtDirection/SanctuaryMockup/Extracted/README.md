# 성소 분절

정본 선택/상세 목업을 기준으로 글자·캐릭터·실루엣·카드 아트를 제외한 조각을 복원했습니다. 배경과 판은 깨끗한 종이 여백 질감으로 채웠으며 격자·눈금·모서리·기호는 원본 형태를 따라 재구성했습니다. 단순 사각 크롭은 아닙니다. 원본 파일은 변경하지 않았습니다.

## 후보 띠

Strip_Frame은 테두리만, Strip_Fill은 종이 면, Strip_Mask는 흰색 마스크입니다. 세 조각은816×680 동일 캔버스·꼭짓점을 사용합니다. Strip_NameBand도 같은 캔버스이며 위530px은 투명, 아래 이름 띠만 남겼습니다. 동일 위치에 겹치면 맞습니다. 프레임 테두리 때문에 Fill/Mask의 외곽과 프레임 외곽은 일치합니다. 마스크 적용은 게임에서 합니다.

생성 목업의01띠는02·03과 폭·기울기가 서로 다릅니다. 요청한 공통 조각을 위해 **02번 정본 띠**의 윤곽을 기준으로 통일했습니다. 따라서01 원본 윤곽과는 차이가 있습니다. layout.json에 원본 참고 영역과 통일한3개 배치, 번호·이름·소속/해금 조건의 로컬/화면 좌표를 모두 기록했습니다. 우측 마지막2px은 화면 경계에서 클리핑됩니다. 실제 구현에서는 끝 띠를2px 왼쪽으로 이동해도 됩니다.

Locked에도 실루엣을 넣지 않았습니다. 게임의 캐릭터 그림을 회색으로 바꿔 동일 마스크로 표시합니다. 잠금 기호는 별도 Icon_Lock입니다. Selected는 청록 테두리와 아래 굵은 선, Hover는 청록 테두리입니다.

## 상세·공통

Sanctuary_Background는1920×1080 공용 종이 격자 배경이며 상단106px은 장식 없이 비웠습니다. Detail_ArtBackdrop은 투명 원형 눈금, 게임에서 알파0.22 정도부터 조절합니다.

Detail_InfoPanel에는 라벨·내용·구분선이 없습니다. Detail_SectionRule은 글자 자리를 비운 줄표와 긴 선입니다. Detail_CardSlot은 아트 창이 투명하며 art_window_xywh에 영역을 기록했습니다. 카드 칸은 지정 크기 사용을 권장합니다. AffinityCell은 Empty/Filled 분리입니다.

Button_Detail은 아래 청록 선 포함4상태, Back3상태, Join4상태입니다. 화살표는 별도이며 먹색 합류 버튼 위에서는 흰색 tint합니다. Name_AccentLine은120×3 흰색입니다.

## 좌표·9-slice

source_crop_xywh는1920×1080 정본 참고 영역, placement_1920_xy는 좌상단 배치, output_size는 실제 크기입니다. 이번 요청의 정본 크기를 따라 상세 정보판806×604, Detail버튼416×114, Back326×94, Join332×100으로 출력했습니다. 이전 초안 문서 치수와 다릅니다.

nine_slice_lbrt 순서는Left,Bottom,Right,Top입니다. 사선 후보 띠·마스크·이름 띠는 **Simple, 비율 고정**으로 사용하고9-slice하지 않습니다. 정보판·버튼은 모서리 여백이 지정되어 있습니다. 원형 눈금과 기호도Simple로 사용합니다.

## 확인·재생성

python ArtDirection/SanctuaryMockup/Split_Handoff/split_sanctuary.py

Contact_Sheet.jpg는1200px 폭 한 장입니다. 파일 크기·원본 참고 범위·알파0/255·공통 띠 마스크 일치·아트 창 투명을 자동 검증합니다. Unity importer/.meta는 변경하지 않았습니다.
