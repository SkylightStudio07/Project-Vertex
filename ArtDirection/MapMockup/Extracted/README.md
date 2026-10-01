# 맵 v1 분절 + 1막 지형

Map_Mockup_v1.png / Map_Mockup_v1_Boss.png를 기준으로 UI를 복원했습니다. 원본 전체 PNG를 화면에 띄우지 않고 미리보기2장과 스크립트로 작업했습니다.

## 제작 방식

판은 깨끗한 제목 줄 여백의 종이 질감을 추출해 반전 반복하고 #EEF0F2로 보정한 복원판입니다. 단순 사각 크롭이 아니며 글자·노드 아이콘·지형 흔적은 없습니다. 원본의 윤곽과 표시를 기준으로 노드 상태·선·작은 기호를 규격에 맞게 재구성했습니다. 판/노드 알파는0/255이며 바깥 그림자는 불투명입니다.

Terrain_Act1은 내장 imagegen으로 새로 생성했습니다. **3320×600, 불투명 종이색 #EEF0F2 배경**을 선택했습니다. 생성 결과의 본문을 가로형으로 편집하고, 오른쪽 원형 광장은 종횡비를 보존해 보스 구역에 배치했습니다. x0~160은 비어 있고, 보스 광장은x3060~3320에 있습니다. 본문 명도차 최대8%, 보스 구역 최대12%로 보정했으며 물색은 #DCE6EE로 제한했습니다. 노드·연결선·글자는 없습니다. 게임에서는 가로로 늘리지 말고 원래 크기로 스크롤 영역에 클리핑합니다.

## 조립

- Map_Panel: 제목 글자 없는 판, 가로선·모서리+ 포함. 지도 영역은 빈 종이입니다.
- FloorTick: 200×28, Current만 가운데 청록 칸. 번호는 게임에서 올립니다. FloorRuler는 반복 가로선입니다.
- LegendBar: 아이콘·글자 없이 구분선7개. 공통 폭1760 기준8개 항목입니다. 심하게 늘리면 구분선 위치도 움직이므로 지정 폭을 권장합니다.
- ScrollBar_Track/Handle: 분리된 트랙과 손잡이. 스크롤에 맞춰 손잡이를 이동합니다.
- Normal76×76: Locked/Accessible/Visited/Current. Rest76×76 및 Elite84×84: Locked/Accessible/Visited. Boss는128×128 칸+24px 아래 이름띠(파일128×152)입니다.
- Accessible은 청록 꺾쇠입니다. Node_AccessibleRing을 추가 레이어로 맥동할 수 있습니다. Rest 아래 청록 띠는 상태와 관계없이 안전 지점을 나타냅니다.
- Visited에는 Node_VisitedCheck를 별도로 얹습니다. Current에는 Node_HereMarker와 런타임 HERE 글자를 얹습니다. Node_QuestTag는 글자 없는 먹색 꼬리표입니다.
- 기존 Assets/Art/Maps/Nodes의 아이콘은 프레임 안에 게임에서 배치합니다. 이번 작업에서 해당 자산을 수정하지 않았습니다.
- Line_Solid는2px 실선입니다. Line_Dashed는10px 대시+6px 공백을 반복하며 두 번째 픽셀 행을50% 알파로 하여1.5px를 근사합니다. 점선은 늘리지 말고 타일링합니다. 지나온 길은 게임에서 청록으로 tint합니다.
- Button_Close 판과 Icon_Close를 따로 제공합니다. Icon_Triangle은 남은 층 라벨 옆에 사용합니다.

## 좌표·9-slice

layout.json의 source_crop_xywh는1920×1080 정본의 참고 영역, placement_1920_xy는 화면 배치, output_size는 실제 파일 크기입니다. 정본과 요청 규격 사이 차이가 있어 출력은 요청 규격으로 정규화했습니다. Map_Panel은(36,120), 지형 뷰포트는(36,236)1848×600입니다. 개별 프레임의 source_crop은 형태 참고이며 아이콘을 실제로 복사한 영역은 아닙니다.

nine_slice_lbrt 순서는 Left, Bottom, Right, Top(출력 픽셀)입니다. 패널 내부 가로선·범례 구분선 때문에 정해진 크기 사용을 권장합니다. 프레임·기호·층 눈금·지형은 지정 크기를 사용하고 크기 변경 시 모서리/선 두께를 확인하십시오. 지형 source_crop은null이며 출처는 생성 파일입니다.

## 재생성·확인

python ArtDirection/MapMockup/Split_Handoff/split_map.py

Contact_Sheet.jpg는1200px 폭 한 장입니다. 규격·알파·크롭 범위·지형 시작 여백을 검증했습니다. 원본 목업 및 Unity importer/.meta는 변경하지 않았습니다.
