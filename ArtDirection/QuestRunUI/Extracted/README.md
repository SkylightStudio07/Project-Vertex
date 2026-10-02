# 런 중 의뢰 UI 분절

승인 시트의 실제 픽셀을 크롭하고, 글자 부분만 내장 이미지 모델 복원 결과로 교체했다. 원본 시트는 1672×941이며 출력 PNG는 브리프 사용 크기로 조정했다.

- Toast_Acquired / Progress / Completed / Deferred_Normal: 420×76. 1920×1080 기준 (1470,130), 최대 3개, 간격 8. 호버 없음.
- Chip_Recovery / Delivery / Chip_Normal: 150×30. Chip_Normal_Hover 동일 크기. 필요시 동일 호버 프레임에 유형 아이콘을 별도 배치한다.
- Summary_Header: 310×24, 칩 포함 조립 310×56. 출정 준비 기준 (1100,26). Summary_Empty는 글자 없는 빈 상태 바탕이다.
- Tooltip_Normal: 300×110, 목표 2줄과 획득 계기를 TMP로 배치한다.
- MapTag_Recovery/Delivery/Elimination/Rescue_Normal: 28×28. Highlight: 40×40, 기준 위치에서 (-6,-6). 태그 바깥 투명 알파 포함.
- MapTooltip_Normal: 220×40, 포인터 포함. 한 줄 목표 문구를 별도 TMP로 배치한다.
- ItemSlot_Normal / ItemSlot_Normal_Hover: 실제 기존 ItemSlot.prefab 크기인 70×70. 중앙 체크무늬는 제거되어 실제 투명 알파다. 일반 슬롯 위에 오버레이로 사용한다.
- Icon_*은 의뢰 게시판 기존 조각을 복사했다. 유형 기호를 바꾸려면 해당 아이콘으로 교체한다.
- Example_*은 글자 포함 조립 참고이며 런타임 텍스트 없는 파일과 구분한다. Example_Summary는 310×56 조립 예시다.

layout.json: 원본 크롭 좌표와 출력 크기, 화면 배치 좌표. 이미지 모델의 글자 제거 결과에 따라 복원 면 질감이 원본과 조금 달라질 수 있다. 종이 패널의 배경은 불투명이며 슬롯 중앙/맵 태그 외부만 투명하다. Unity 배선은 포함하지 않았다.

기본 토스트 왼쪽 기호 칸은 시트 비율대로 축소되어 약 64px보다 작다. 텍스트 배치 시 실제 출력 PNG 기준으로 여백을 맞춘다. 생성 프롬프트는 ../ExtractionPrompts.md, 재생성은 ../Split-QuestRunUI.ps1.
