# Dialogue Choice v2

일반 NPC 대화용 선택지 목업. 세 장 모두 1920×1080이며 Hover와 Selected는 02번 선택지를 기준으로 한다.

- `Choice_Mockup_v2_Default.png`: 등장 완료.
- `Choice_Mockup_v2_Hover.png`: 먹색 반전, 청록 테두리, 오른쪽 이동.
- `Choice_Mockup_v2_Selected.png`: 1.2배 확대, 청록 띠, 다른 줄 잔상과 주인공 퇴장 시작.

주인공은 `Reference_PlayerPose.png`를 그대로 확대·배치했다. 재생성이나 색 보정은 하지 않았다. 상점의 큰 NPC 스탠딩 제거에만 생성형 이미지의 국소 패치를 사용했다. 하단 NPC 초상·이름·대사는 유지하며, y=900 이후는 원본 스크린샷과 픽셀 단위로 동일하다.

전체 디자인 사양은 `COMPOSITION_PROMPT.txt`, 실제 배경 보정 생성 프롬프트는 `GENERATION_PROMPT_CleanPlate.txt`, 연출 사양은 `MOTION_SPEC.txt`에 있다. `composition_manifest.json`에는 배치와 원본 주인공 SHA256을 기록했다.

`build_choice_mockups.py`는 원본 이미지와 `Composition_Source`의 SVG를 사용해 세 상태를 합성한다. Pillow, NumPy, Node 및 스크립트에 명시된 로컬 resvg 경로가 필요하다. 저장소 루트에서 `python ArtDirection/DialogueMockup/Choice_v2/build_choice_mockups.py`로 실행한다.

이번 결과물은 목업과 재현용 소스다. 게임 적용용 분절 패키지 및 Unity UI 구현은 포함하지 않는다. Choice v1은 변경하지 않았다.
