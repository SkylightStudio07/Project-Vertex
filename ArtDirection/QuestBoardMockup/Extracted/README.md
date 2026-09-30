# 의뢰 게시판 분절 키트

기준: ../QuestBoard_Posted_Mockup_v1.png, 1672×941. 원본 픽셀 크롭에 글자 영역만 이미지 모델 복원 결과를 적용했다. 프롬프트는 ../ExtractionPrompts.md, 재현 스크립트는 ../Split-QuestBoard.ps1.

## 조립

- layout.json: 좌상단 좌표·크기·상태 매핑. 원본 픽셀 단위이며 Unity 좌상단 앵커에서는 (x, -y)로 사용.
- UI_Base_WithControlHoles + controlLayers를 순서대로 놓으면 Reassembled_Normal이 된다.
- *_Composite는 검토용 묶음이다. 하위 조각을 중복 배치하지 않는다.
- Backing_*_Restored는 동적 내용 교체용 빈 면. 숨겨진 원본 레이어가 아니라 모델이 복원한 면이다.
- Card_*은 원래 네 카드의 크기와 상태를 보존한다. 크기가 약간씩 다르므로 공통 카드 프리팹에는 Backing_Card_Normal_Restored를 기준으로 맞춘다.

## 상태

- Back: 기본 / 호버.
- Tab_Posted, Tab_Active, Tab_History: 기본 / 선택 / 호버. 추가 선택 상태는 원본 실루엣을 청록으로 재색상 처리했다.
- 카드: 기본 / 호버 / 선택 / 수주됨 / 잠금. Card_Accepted에는 원본 수주 도장이 포함된다.
- 필터: 체크됨 / 미체크 / 호버.
- Accept_Normal / Accept_Normal_Hover / Accept_Disabled / Cancel_Normal / Cancel_Normal_Hover는 **(1156,824), 431×74** 공통. 비활성에는 호버와 상호작용을 적용하지 않는다.
- 별도 TMP 문구: 수주=의뢰 수주, 취소=수주 취소, 비활성=수주 한도 도달. 진행 중 2/2 판정 및 실제 동작은 아직 구현하지 않았다.
- Stamp_Accepted_KO / Stamp_Completed_KO: 요청한 수주/완료 한글 도장, 청록 잉크를 알파로 분리한 투명 PNG. 번역 시 Stamp_Frame_Textless 위에 별도 텍스트를 얹는다. KO 도장 이미지는 텍스트 포함 예외다.
- Badge_Story_Blank / Badge_Rescue_Blank 위에 STORY / 구출을 별도 UI 텍스트로 올린다.
- 회수·배달·토벌·구출 기호는 Icon_Recovery/Delivery/Elimination/Rescue.

## 한계

카드 삽화와 엠블럼은 사각 크롭이며 원본의 글자 제거 영역에서 질감 이음새가 남을 수 있다. 작은 흑연 기호는 밝기 기반 알파로 추출했다. 모든 그림의 누끼나 숨겨진 배경을 완벽하게 복구한 파일은 아니다. 수주 도장 포함 카드 외 일반 글자는 제거 대상으로 처리했다.

재조립 검사는 마스터와 베이스+조각의 픽셀 일치만 확인한다. 모델 복원 품질이나 Unity 동작 검증은 아니다. Unity 임포트/프리팹/배선은 이번 작업에 포함하지 않았다.
