# 목업 픽셀 분리 세트

기준: ../Lobby_Textless_Master.png, 1672×941. UI는 새로 생성하지 않고 원본 픽셀을 자른다. 호버만 동일 이미지에 시안 색과 테두리를 덧입혔다. 외곽 알파는 다각형 마스크로 처리했다.

## 조립

- 전체 비교: Reassembled_Normal.png / Reassembled_AllHover.png. AllHover는 모든 버튼의 호버를 한 번에 보여주는 비교용이다.
- 원본 재조립: Scene_WithCharacter_ReferenceOnly.png → UI_Static_FullCanvas.png → layout.json 좌표에 Normal 버튼 7개와 Progress_Composite.png.
- reassembly-check.json에서 원본과 다른 픽셀 수를 확인한다. 첫 검증 결과 0 / 1,573,352.
- UI_Static_FullCanvas는 버튼 영역이 뚫린 1672×941 투명 레이어다. LeftPanel_Static은 (0,0), RightPanel_Static은 (599,0)에 놓는 대체 분리본이다. 전체 레이어와 중복 배치하지 않는다.
- RightPanel_Composite와 Header_Right_Composite는 버튼/아이콘까지 포함된 비교용 크롭이다. 분리 버튼과 중복 사용하지 않는다.
- 좌표는 좌상단 원점 픽셀 단위다. UI 앵커와 피벗을 좌상단으로 설정하고 anchoredPosition=(x,-y), sizeDelta=(width,height)를 사용한다. 실제 게임 해상도에 맞춰 전체를 같은 비율로 스케일한다.

## 파일 역할

- Expedition, Bonfire, Armory, QuestBoard, InformationBroker, TrainingGround: Normal 및 Normal_Hover. 모서리 밖 투명, 아이콘/화살표 포함.
- Settings_Button_Normal 및 Hover: 원래 종이 면을 포함한 클릭 영역. Settings_Icon은 독립 투명 아이콘.
- *_Icon, Brand_Compass, Arrow_Dark, CornerMark: 원본 명도에서 알파를 계산한 흑연색 심볼. 원본 크롭과 달리 색/안티앨리어싱이 재해석된다.
- *_Crop: 배경까지 포함된 그림 조각, 독립 투명 아이콘이 아니다.
- Progress_Composite: 원래 게이지. Track_Segment / Fill_Segment는 재사용할 수 있는 작은 면 조각이다. 런타임 수치는 별도 텍스트, 채움 비율은 별도 제어한다.
- Footer_Dark, LeftHeader, LeftLower_Composite: 원본 장식 조각.
- Background_Restored: 내장 이미지 모델로 UI/캐릭터를 지우고 가려진 캠프를 복원한 교체용 배경. 원본과 픽셀 일치하지 않는다.
- Scene_WithCharacter_ReferenceOnly: 원본에서 UI 영역만 제외한 정확한 재조립 레이어. 캐릭터와 배경이 합쳐져 있어 교체용 배경으로 쓰지 않는다.

## 범위와 한계

현재는 아트 분리 결과이며 Unity 임포트/씬 배선은 하지 않았다. 원본 대사 텍스트는 이전 무문자 편집에서 제거되어 새로운 대사 패널을 임의로 생성하지 않았다. 캐릭터는 원본과 배경에 합성되어 있다. 교체 가능한 캐릭터에는 별도 투명 스플래시 원본을 사용해야 한다. 다각형 경계는 픽셀 단위 수동 지정이며 배경 교체 시 경계에 남은 원래 배경색을 추가로 다듬을 수 있다.

## 배경 생성 프롬프트

Precise background restoration of the approved lobby. Remove character and all UI. Preserve visible tent, crates, ground, mountain and observation tower, reconstruct occluded areas in the same cool low-saturation anime environment style. No character, text, logos or interface. Built-in image model; source image only reference.

분리 재현: ../Split-Lobby.ps1. 원본은 보존하며 Extracted 산출물만 다시 만든다.
