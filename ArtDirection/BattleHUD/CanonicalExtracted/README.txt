Battle HUD / ★ v3 정본 분절

기준: BattleHUD_Mockup_v3_Canonical.png (1672×941).
47개 개별 PNG. Example_PlayerTurn.png / Example_EnemyTurn.png는 1920×1080 조립본.
Example_TurnBanner.png는 정본 HUD에 Oratio 턴 배너의 선/글자 배치만 추가한 참고 예시.

정본 우선 적용
- 지도만 Oratio_TurnBanner의 접힌 지도 선화에서 추출. 정본 별 아이콘은 사용하지 않는다.
- Res_Panel은 판 하나(264×225). Res_Divider와 Res_Slash는 별도 투명 조각.
- 작은 에너지 최대 숫자 위 청록 선 제거.
- PNG에는 UI 문자/수치가 없다. 모든 글자는 런타임 TMP로 올린다.
- 기호는 판과 별도 투명 32×32. 방패는 배지 크기.
- HP 틀/채움/트랙 분리. 예시 적 HP 숫자는 먹색.

버튼 조립
Button_Map_Normal/Hover/Pressed + Icon_Map
Button_Deck_Normal/Hover/Pressed + Icon_Deck
Button_EndTurn_Normal/Hover/Pressed/Disabled + EndTurn_CornerSlash + Icon_Arrow
비활성에서는 Icon_Arrow_Disabled, Icon_Energy_Disabled 사용.
판 PNG는 기호나 라벨을 포함하지 않는다. 상태를 바꿀 때 기호 레이어도 함께 바꾼다.
ItemSlot_Empty와 Icon_SlotPlus를 겹친다. ItemSlot_Quest는 작은 회수 배지를 포함.

좌표
layout.json의 canonicalRect는 정본 원본 좌표. placement1920/displaySize1920은 같은 화면을 1920×1080으로 환산한 배치다.
size는 요청서의 PNG 출력 규격. 정본 그림의 실제 크기 비율은 요청서 표와 조금 다르므로 원본 재현용 displaySize1920과 구분했다.
TopBar 1300×72, 아이템 56×56, MAP/DECK 64×64, END TURN 280×70.
HP 360×39 / 340×27 / 340×27, 적 HP 240×26 / 227×18 / 227×18.
방패 56×68 / 40×48. TextBand 120×40, border 가이드 [8,4,8,4].
resourceLayers는 판/두 가로 구분선/에너지 사선의 개별 배치 목록이다.

이미지 생성 및 분절 방식
빈 판은 내장 imagegen으로 정본의 글자와 기호를 지운 뒤, 해당 HUD 영역만 크롭했다.
게임 배경/캐릭터/카드는 조립본에서 정본 픽셀을 재사용했다. 생성된 전체 화면으로 교체하지 않았다.
판 내부는 알파255, 판 바깥/기호 바탕/HP 구멍은 투명. 기호는 밝기 기반 알파 추출이라 가장자리에 약간의 원본 래스터 흔적이 있을 수 있다.
권총 기호는 정본에 존재하지 않아 내장 이미지 모델로 별도 생성했다.
헤어라인, 구분선, 채움/트랙 및 상태 변형은 규격에 맞춰 결정적으로 출력했다. 목업 전체를 다시 디자인하지 않았다.
큰 조각은 작은 원본 크롭에서 요청 규격으로 확대한 결과다. 모든 조각이 네이티브 고해상도 신규 생성물인 것은 아니다.

Assembly_*_TextlessHUD는 분리한 UI 텍스트를 얹기 전 예시. 카드 글자, 적 인텐트 숫자, 기존 가장자리 장식은 게임 원본 영역으로 남아 있다.
Example_*의 글꼴은 로컬 미리보기용 Malgun Gothic. 게임에서는 TMP Pretendard 적용.
Unity 임포트/프리팹/배선은 이번 작업 범위에 포함되지 않았다.
재생성: ../Split-Canonical.ps1. 프롬프트: ../Canonical_ExtractionPrompts.txt.
