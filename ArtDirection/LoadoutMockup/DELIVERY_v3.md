# 출정 준비 v3 납품 — 2026-10-23

SPLIT_REQUEST_v2.md 반영. Loadout_Mockup_v3.png는 1920×1080 기준으로 SVG 조각과 WeaponSVG 원본 PNG를 직접 조립한 검토본이다. 이미지 모델로 무기를 다시 그리지 않았다.

- Loadout_v3_Package.zip: 목업·상태 미리보기·조각·layout.json·재현 스크립트.
- Extracted_Draft/: PNG 33개, SVG 32개. 배경은 1920×1080, UI는 2배. Icon_Artifact는 제공 PNG 원본으로 SVG가 없다.
- 무기는 권총·스나이퍼·레일건·빈 칸. 잠긴 무기에 아티팩트 1 표시.
- WeaponSVG 파일은 패키지와 조각에 넣지 않았다. layout.json의 external_weapons와 각 art_rect로 외부 참조한다. 원본의 알파 여백만 배치 계산에 제외하고 그림 비율·방향·색은 유지한다.
- 원본 권총이 오른쪽을 향하므로 큰 그림과 소개 판이 겹치지 않도록 소개 판을 조금 아래로 이동했다. 수치는 탄창 3 / 사격 추가 피해 +0 / 강화 0/2 예시. 무기고 이동은 별도 버튼.
- 시작 덱 Row_Deck 템플릿과 110픽셀 줄 간격, 카드 아이콘·이름·효과·개수·강화 + 표시를 정의했다.
- 의뢰 Row 템플릿은 최대 2개, 0개면 진행 중 의뢰 없음 한 줄. 두 줄일 때 덱 머리와 겹치지 않는다.
- 특전 잠김/해금, 출정 보통/호버/비활성 상태 포함. 해금 예시의 특전 썸네일·효과는 자리표시자다.
- Mark_Upgraded와 Icon_Artifact는 기존 파일과 해시 일치 확인. 무기 원본도 변경 없음 확인.
- PNG/SVG에는 UI 글자를 굽지 않았다. layout.json의 TMP 텍스트 상자로 분리했다. 9-slice 경계는 원본 PNG 픽셀의 left/bottom/right/top 순서.
- 기하 배경/판은 벡터 재구성이므로 기존 생성 목업의 질감·그림자와 세부 차이가 있다. Unity 적용 없음.

검증: 필수 27종, PNG 크기/알파, 텍스트 범위, 9-slice 경계, 의뢰 간격, ZIP 무결성, 무기 파일 미포함. 결과는 Extracted_Draft/validation.json.
