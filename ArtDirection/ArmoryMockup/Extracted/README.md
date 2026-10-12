# 무기고 장비 강화 — 분절 키트

기준: `../Armory_Equipment_Mockup_v2_Sniper.png` (1672 × 941). 세 번째 무기는 **저격총**이다.

## 구성

- `Armory_Textless_Master.png`: 원본에서 글자 영역만 이미지 모델 복원 결과로 교체한 마스터.
- `*_Composite`: 검토용 묶음. 하위 조각들과 동시에 겹쳐 쓰지 않는다.
- `Back_*`, `Tab_*`, `Weapon_*`, `CardAttack_Upgrade_*`, `CardDefense_Upgrade_*`, `Upgrade_*`: 개별 컨트롤. `_Hover`는 동일 좌표·크기에 청록 틴트와 테두리를 더한 상태다. 잠금/비활성 컨트롤에는 호버가 없다.
- `WeaponArt_Pistol`, `*_Thumbnail`, `Icon_*`, `UpgradeNode_*`, `ExperienceGauge_*`, `Divider_*`, `Decoration_*`: 무기 아트와 기호, 트리, 게이지, 구분선, 장식.
- `Backing_*_Restored`: 이미지 모델이 콘텐츠를 지우고 복원한 빈 패널. 원본 픽셀과 동일한 레이어가 아니다. 전체 또는 개별 패널 중 한 방식을 골라 쓴다.
- `Source_*`: 글자 포함 원본의 직사각형 조각. 디자인 보존용이며 실제 UI용이 아니다.
- `UI_Base_WithControlHoles.png`: 위 컨트롤만 투명하게 뺀 고정 레이어. 무기 아트·아이콘 등의 고정 콘텐츠는 남아 있다.
- `Reassembled_Textless.png`, `Reassembled_AllHover.png`: 기본/호버 조립 미리보기.
- `layout.json`: PNG별 원점(좌상단), 크기, 종류, 호버 연결 및 조립 순서.

## 사용

1. 목업 재현: `UI_Base_WithControlHoles`를 놓고 `layout.json`의 `controlLayers`만 순서대로 배치한다.
2. 동적 화면 제작: `Backing_*_Restored`의 빈 패널에 무기·아이콘·컨트롤을 개별 배치한다. Composite와 하위 조각을 중복 배치하지 않는다.
3. 텍스트/수치는 별도 Pretendard UI 텍스트로 배치한다. 목업의 비용·수치는 예시이지 게임 데이터가 아니다.
4. 원본 좌표는 1672×941 좌상단 기준. Unity의 좌상단 기준 앵커/피벗으로 옮길 경우 anchoredPosition은 `(x, -y)`다.

## 한계와 검증 범위

- 이미지 모델에는 전체 텍스트 제거를 요청했지만, 최종 마스터는 디자인 변형을 줄이기 위해 지정된 글자 사각형만 복원 결과를 사용한다. 경계의 종이 질감 차이가 있을 수 있다.
- 총기 본체는 원본 윤곽을 수동 마스킹한 추출물이다. 방아쇠 내부나 미세 윤곽의 바탕 잔여는 완전한 자동 누끼가 아니다.
- `Sniper_LockedThumbnail_Composite`와 `Shotgun_LockedThumbnail_Composite`는 자물쇠가 겹친 원본이며, 가려진 총기 부분이 복구된 해금 아트가 아니다.
- 업그레이드 노드 원형은 사각형 배경 포함 크롭이다. 아이콘 luminance-alpha는 원본 밝기를 알파로 바꾼 것으로 완전한 원본 레이어 복원이 아니다.
- 조립 픽셀 검사는 텍스트 없는 마스터와 고정 베이스+컨트롤이 일치하는지만 확인한다. 독립 배경 복원의 정확성이나 Unity 배선을 보장하지 않는다.
- Unity 임포트·프리팹·런타임 연결은 이번 작업에 포함하지 않았다. 카드 목록 탭의 본문은 별도 디자인이 필요하다.

재생성: `../Split-Armory.ps1`. 생성 프롬프트는 `../ExtractionPrompts.md` 참고.
