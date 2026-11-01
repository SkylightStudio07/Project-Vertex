# 대화 선택지 v1 · 상태 목업

2026-11-01. `REQUEST.md`에 따른 세 장의 디자인 검토본.

- `Choice_Mockup_v1_Default.png`: 흰 종이 판3장, 사선 컷·계단식 배치·한 줄의 얇은 착지 윤곽 잔상.
- `Choice_Mockup_v1_Hover.png`: 01 데려간다만 먹색/백색 반전, 청록 외곽선, 오른쪽 이동.
- `Choice_Mockup_v1_Selected.png`: 01 선택 확대와 청록 하단 띠, 02/03은 반투명으로 왼쪽 퇴장하는 순간.
- 세 장 모두 최종1920×1080. 내장 image_gen 생성 원본1672×941에서 Lanczos 리사이즈했다. 생성 원본은 `Generated_Originals/`에 보존.
- 프롬프트: `GENERATION_PROMPT_Default.txt`, `GENERATION_PROMPT_Hover.txt`, `GENERATION_PROMPT_Selected.txt`.
- 참조: 기본은 `Reference_CurrentDialogue.png`와 `Reference_Lobby.png`, 상태 변형은 생성된 Default를 입력으로 사용.
- `MOTION_SPEC.txt`: 등장0.06초 간격·착지 반동·Hover·선택1.2배 및0.25초 후 퇴장을 설명. 구현은 수행하지 않았다.

페르소나 계열에서 사선 구도와 등장/퇴장 동작만 참고하고, VERTEX의 회백·먹색·청록, 얇은 선과 하드 섀도우를 사용했다. 기본 세 문구와 번호·태그를 유지했다.

세 장을 시각 비교해 상태 위계·제목 가독성·얼굴/하단 대사 영역 침범 여부를 확인했다. 출력 크기도 확인했다. 생성 모델이 화면 전체를 재현한 목업이므로 인물·배경·기존 대사창의 픽셀 보존 합성본은 아니다. 정확한 각도/1.2배 확대·그림자 수치와 원본 고정 합성은 확정 후 조각/구현 단계에서 적용할 사항이다.

이번 범위는 목업과 프롬프트/연출 설명까지다. 분절·layout.json·Unity 적용은 수행하지 않았다.

## SPLIT_REQUEST · 분절 완료

- `Extracted_Draft/`: 신규 SVG/2배 PNG 10쌍, `layout.json`, `README.md`, `validation.json`, `Contact_Sheet.jpg`(1200px 폭).
- `Choice_v1_Package.zip`: 위 폴더 전체. 외부 자산 의존성 없이 조각 포함.
- 모든 조각은0°·꼬리 없음·텍스트 없음. 명시된 왼쪽 아래/오른쪽 위 사선 컷으로 통일했다. Normal/Hover/Selected, 별도 하드 섀도우와 빈 윤곽 Ghost, tint용 선/칸막이/화살표, 선택띠 제공.
- 1920×1080 회전 전 행 rect 및 로컬 글자 rect, 화면 회전/Unity 회전 변환, 상태별 이동·확대·불투명도, 1/2/3개 배치와 긴 문구 정책을 기록했다.
- 1개/2개는 위쪽 행부터 사용. 최대960px까지 오른쪽 끝 고정으로 확장, 22자 초과 말줄임. 그림자 화면 방향과2배PNG의9-slice/PPU 설정도 명시했다.
- 검수: 크기, 글자·임베디드 그림·필터 미포함, 모서리 방향, Ghost 내부 투명, 실제9-slice 확장 전후 모서리 픽셀 일치, 긴 한글/태그 간격, 확인 시트 시각 검수 및ZIP 무결성.
- SVG 재구성본이며 종이 질감의 픽셀 단위 추출은 아니다. Unity 적용은 수행하지 않았다.
- 재생성: `python ArtDirection/DialogueMockup/Choice_v1/build_choice_delivery.py` (Pillow, NumPy, Node/resvg).
