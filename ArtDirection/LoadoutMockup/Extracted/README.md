# 출정 준비 목업 분리 세트

기준 원본: ../Loadout_Mockup_v1.png (1672×941). UI를 개별 재디자인하지 않고 같은 좌표에서 분리했다. 씬/프리팹 배선은 아직 하지 않았다.

## 비교와 조립

- Reassembled_Source.png: 글자 포함 원본 6조각 재조립. 픽셀 차이는 layout.json의 sourceReassemblyDifferentPixels.
- Loadout_Textless_Master.png / Reassembled_Textless.png: 원본에서 글자 영역만 복원한 무문자 화면. 글자 영역 외 픽셀은 원본에서 유지한다.
- UI_Base_WithControlHoles.png + layout.json의 controlLayers: 버튼을 개별로 교체할 수 있는 정확한 무문자 화면 조립 방식. 원점은 좌상단, Unity 좌상단 앵커/피벗, 위치=(x,-y), 크기=(width,height).
- Reassembled_AllHover.png: 동작 가능한 버튼의 호버를 동시에 보여주는 비교본. 잠금 무기/특전에는 호버를 만들지 않았다.

## 파일 구분

- *_Composite: 해당 영역의 그림·프레임·아이콘이 함께 있는 무문자 크롭. 하위 조각들과 중복 배치하지 않는다.
- *_Normal / *_Normal_Hover: 로비, 무기고 연결, 권총/SMG 선택, 좌우 캐러셀, 출정 버튼. 호버는 동일 원본 위에 시안 면과 얇은 테두리를 추가한다. 위치/크기 동일.
- *_Locked: AR/산탄총 선택칸, 특전 슬롯의 잠금 상태.
- SelectedTag, WeaponStats, DeckRow_*, Divider_*: 선택 표시, 수치 영역, 시작 덱 3행과 구분선. 실제 수치/글자는 별도 TMP로 표시한다.
- Icon_* 및 Decoration_*: 명도에서 알파를 추출한 흑연색 아이콘. 원본 색과 안티앨리어싱은 재해석된다.
- *_Crop: 종이/어두운 면이 포함된 그림 크롭. 투명 아이콘이 아니다.
- WeaponArt_Pistol: 원본 권총 그림의 수동 다각형 컷. 외곽 투명 처리했지만 방아쇠 안쪽/윤곽 주변의 옅은 배경은 남을 수 있다. 확정 무기 스프라이트로 교체하는 것을 권장한다.
- Backing_*_Restored: 무기/아이콘 아래 가려진 면을 이미지 모델로 복원한 빈 패널 및 슬롯. 원본과 픽셀 일치하는 조각이 아니며 교체 가능한 무기/덱 UI를 구성할 때 사용한다. 복원된 큰 패널에도 슬롯 경계는 포함되어 있다.
- Source_*: 글자 포함 정확한 원본 구역. 비교/복구용이다.
- Paper_Texture: 원본 종이 질감 샘플.

## 글자 복원 과정

내장 이미지 모델로 전체 화면의 글자를 지운 뒤, layout.json의 textEraseRectangles에 해당하는 영역만 원본 위에 복사했다. 기하학/아이콘/무기는 그 영역 밖에서 원본 그대로다. 제거 영역 경계에 미세한 질감 차이가 있을 수 있다. 무문자 이미지의 픽셀 일치는 원본이 아니라 이 합성 기준본에 대해 검사한다.

프롬프트: Exact text-removal edit. Preserve exact UI layout, panel shapes, artwork and icons. Remove all words and numbers only; inpaint with continuous paper, cyan tag or graphite face. No repeated stamped texture, no new geometry.

빈 면 복원 프롬프트: Preserve exact screen geometry, paper panel shapes, dividers, slots and controls; remove weapon art, icons, locks, compass and arrows; reconstruct only underlying paper/graphite surfaces. No new text, artwork or layout.

배경은 목업의 좌우 가장자리에만 보인다. Source_LeftMargin / Source_RightMargin은 보이는 원본 배경 조각이며, 숨겨진 환경 전체를 복원한 배경은 아니다. 별도 환경 이미지로 교체할 때는 큰 종이 패널 외곽 마스크를 추가로 맞춰야 한다.

재현: ../Split-Loadout.ps1. 원본, 모델 복원본 2개가 필요하며 Extracted 산출물만 갱신한다. 세부 좌표와 검증 결과는 layout.json.
