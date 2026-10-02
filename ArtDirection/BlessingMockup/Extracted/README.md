# 축복 v1 분절 결과

정본 Blessing_Mockup_v1_Canonical.png에서 제작했습니다. 원본을 화면에 열지 않고 축소본과 스크립트로 작업했습니다.

## 조각 및 조립

- Panel_Dialogue: 초상·대사·라벨·워터마크 없는 종이 판. 위쪽 가는 선 포함. Portrait_Cell과 Watermark_Compass를 별도 배치합니다.
- Panel_Option_A/B: 오른쪽 위/아래 사선 컷만 다른 공통 판. 각각 Normal/Hover/Disabled. 번호·제목·설명·선택지 아이콘·구분선·화살표는 없습니다.
- Option_IconCell: 56×56 빈 칸, Normal/Hover(먹색)/Disabled. 기존 게임 아이콘을 위에 올립니다. 대사 초상도 기존 게임 자산을 사용합니다.
- Option_Divider/NumberUnderline/Icon_Arrow: 별도 레이어. 상태에 따른 기호/글자 색은 게임에서 처리합니다.
- Panel_Nameplate: 먹색 판과 회색 그림자, 조각 목록에서 요구한 + 표시 포함. 이름·소개·라벨·붉은 선은 제외했습니다. Icon_Plus는 별도 활용용이며 이미 포함된 자리에 중복 배치하지 마십시오.
- Nameplate_AccentLine: 60×2 붉은 선. 원본 선보다 짧게 정규화했습니다.
- Corner_Marks: 1920×1080 투명 오버레이, 좌상단·우하단 선과 +만 포함. 마이크로 카피는 게임 글자입니다.
- HUD 아이템 칸, MAP/DECK, 캐릭터, 배경, 선택지 아이콘, 대사 초상은 결과에 포함하지 않았습니다.

## 복원 방식

단순 사각 크롭이 아닌 글자 없는 복원판입니다. 대사 판의 깨끗한 여백에서 추출한 종이 질감을 반전 반복해 글자·아이콘·장식 영역을 채우고 원본 윤곽에 맞춰 판과 불투명 그림자 띠를 복원했습니다. 종이는 #EEF0F2, 비활성은 #D9DCDF입니다. 판 알파는 0/255입니다. 상태 파생판과 작은 기호는 규격에 맞춰 재구성했습니다.

Watermark_Compass는 원본 나침반을 밝기 마스크로 추출했습니다. 종이 바탕을 제거하고 선화만 남겼으며 가변 알파입니다. 게임에서 전체 알파0.18부터 조절하십시오. 나침반 주변의 미세한 질감이 일부 남을 수 있습니다.

## 좌표·9-slice

layout.json: source_crop_xywh는 원본1672×941 픽셀 기준 참고/추출 영역, output_size는 PNG 크기, placement_1920_xy는 좌상단 원점 배치입니다. 옵션 판은1158×70으로 통일했습니다. 대사 판1158×140과 이름판376×160은 정본 외형에 맞춰 이전 레이아웃 문서와 치수가 다릅니다.

nine_slice_lbrt는 Left, Bottom, Right, Top 순서이며 출력 픽셀 단위입니다. 사선 컷·그림자·+를 보존하도록 경계를 잡았습니다. 칸은2px 경계이며, 기호·워터마크·오버레이는 Simple로 사용합니다. 옵션 Hover는 게임에서 x=-8px 이동합니다. JSON option_instances는 정본 기준이며 초안 레이아웃의 y값과 다를 수 있습니다.

## 검증·재생성

python ArtDirection/BlessingMockup/Split_Handoff/split_blessing.py

PNG 크기·원본 크롭 범위·판 알파를 자동 확인했습니다. Contact_Sheet.jpg 한 장(1200px 폭)에 전 조각이 있습니다. Unity importer/.meta 및 원본 파일은 수정하지 않았습니다.
