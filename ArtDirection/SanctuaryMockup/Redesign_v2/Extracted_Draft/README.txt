성소 후보 선택 v3 / SVG 분절 패키지

범위: 후보 선택 화면만. 상세 화면은 포함하지 않음.
좌표: 1920×1080, 좌상단 원점, rect = x,y,width,height.
PNG: UI 2배, 배경/전체 가이드 1배. SVG는 논리 크기 viewBox.
SVG/PNG에는 글자, 캐릭터, 잠긴 실루엣, HUD를 포함하지 않음.
TMP 글자: layout.json의 text_boxes. Pretendard 기준 크기, 색, 정렬, 자간.
자물쇠: 기존 LoadoutMockup/Extracted_Draft/Icon_Lock 재사용. 외부 참조.

배경 사용법:
  A. 고정: Sanctuary_BG 한 장. Hover 때 해당 면만 교체/겹침.
  B. 동적: Sanctuary_Base → 후보별 Panel → Overlay_Guides.
  A와 B를 중복으로 그리지 않음. 선택 경계선과 캐릭터는 그 위에 배치.
Panel_Locked_Right는 화면 오른쪽에서 잘린 모양을 맞춘 별도 조각.
Line_ActiveEdge는 원본 목업의 활성 면 오른쪽 경계(02 면 왼쪽 경계).

캐릭터: 프로젝트 원본 외부 참조, preserve aspect / contain / no mask.
세 후보 rect 높이 930px 동일. 넓은 무기는 contain 결과 표시 높이가 작아질 수 있음.
02는 현재 알레시아_Standing.png 실루엣으로 미리보기. 요청서의 1.5배 방패/짧은 창과 현재 원본의 무기 비율은 다르므로 원본을 변경하지 않았다.
잠금 문구 70%, 조건 55%, 조건은 두 줄까지. 조건 문자열은 런타임 바인딩.
Rule_Name은 흰색 tint용. 이름 선/소속에는 후보 테마색 적용.
동행 가득 상태는 상태 문구 변경 및 확인 버튼 disabled.

그림자: Shadow_Button을 버튼보다 오른쪽 아래 12px에 배치. 먹색 35%, blur 없음.
Unity Shadow를 쓰면 그림자 PNG는 끈다. 둘을 중복하지 않음.
상단 106px는 HUD 자리이며 배경 단색만 둠.
9-slice 없음. 면 클릭 판정은 bounding rect 대신 polygon 사용.

Sanctuary_Select_Mockup_v3.png는 제공 조각과 원본 아트로 조립한 검수 이미지.
Asset_Overview.png는 체크무늬 검수 시트로 게임에 넣지 않음.
validation.json: PNG 크기, 외부 경로, 텍스트 박스, 글자/원본 그림 미포함 검증.
Unity 씬, 임포터, meta 파일은 변경하지 않음.
