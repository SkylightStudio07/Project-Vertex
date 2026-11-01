# CompanionOrder / RasterRedo v3 분절

2배 투명 RGBA PNG 14개. 좌표 기준1920×1080, 원점 왼쪽 위. 회전0°, 모든 PNG 외곽은 이진 알파0/255. 상태별 크기와 알파 동일 검증 통과.

SVG 미사용. 내장 image_gen으로 정본을 참조한 글자 없는 원화를 생성해 사용했다. 제거/Cancel/CLOSE는 생성된 판 전체를 크로마 배경에서 분리했다. 먹색 띠·동료 목록·HUD 표면은 생성 원화를 래스터 외곽 마스크로 잘라 사용했다. 상태 차이는 동일 원화의 톤을 조절했다. 선·작은 칩·마름모·막대·외곽 마스크만 단색 후처리했다. 글자·숫자·동료 얼굴은 납품 PNG에 없다. 맵·덱 아이콘과 제거 조준 문양은 요청대로 포함했다.

실제 생성 프롬프트: ../PROMPTS.txt. 생성 원화 및 참조 크롭: ../Generated/. 재생성으로 미세 질감과 일부 판 디테일은 정본과 차이가 있다. 고해상도 원화를 최종2배 규격으로 리샘플링했으며 픽셀 단위 복원은 아니다.

List_Panel 세로9-slice. 머리42px, 줄 간격78px. 동료 얼굴은 게임이 넣고 Face_Frame만 겹친다. 충전 막대는 줄/태그보다 위에 둔다. Disabled에서 마름모·배지는 layout.disabled_tints로 회색 처리한다. Row_Hover와 탭3상태는 동일 외곽이다.

layout.json의 text_slots.rect는 parent 조각 왼쪽 위 기준 화면 픽셀이다. 상태가 바뀌어도 같은 위치를 쓴다. nine_slice_px는 PNG 기준 왼·아래·오른·위. 상태별로 같은 경계를 적용한다. 조립 순서는 layer_order 참조.

Contact_Sheet.jpg는 조각 단독·상태별 표면·실제9-slice 늘림·정본/재조립 비교를 담았다. Reassembled는 정본 장면 위에 해당 조각만 교체한 확인 화면이며 글자와 얼굴은 미리보기에만 추가했다. 주변 UI와 우측 상태 비교 박스는 정본 배경이다. 게임에 적용하거나 기존 Assets/Resources를 교체하지 않았다.

상태 확인용 생성 목업: ../../Battle_SmallUI_States.png. 이 그림은 방향 확인용이고 실제 크기·알파는 납품 PNG/layout이 기준이다.
