# 헌터킬러 땅 파임 VFX

`../GROUND_IMPACT_REQUEST.md`에 따른 아트 제작. 2026-10-24.

- 최종 PNG와 layout: `../Extracted_Draft/`.
- Crater_A/B/C 380×190, Dust_Puff 2048×256 (256×256 8프레임, 8열1행, 여백0), Debris_01~06 (16~32px), Shock_Ring 600×300.
- Dust_Frames/에 개별8프레임도 제공. 모든 피벗은 가운데, 흰 먼지/고리는 런타임 tint용. 어두운 구덩이/돌은 중성 회색 RGB를 유지한다.
- `GroundImpact_Package.zip`: 최종 조각+layout/검증/인계문서 및 미리보기4개.
- `GroundImpact_Asset_Overview.png`: 투명도와 크기 검수 시트.
- `GroundImpact_Bunker_Preview.png`, `GroundImpact_Sequence_Preview.gif`: 실제 벙커 배경에서 24fps 타격프레임15·21·27·33·42·45에 따른 6회 시연.
- `Dust_Puff_Preview.gif`: 먼지8프레임 별도 재생.

내장 image_gen 사용. 투명 배경 옵션은 현재 도구가 `background='transparent' is not supported by gpt-image-2` 오류를 반환했다. 이후 동일한 내장 도구에서 마젠타/검정 키 원화를 생성해 알파 추출·디스필·중성화·리사이즈·시트 분리를 수행했다. CLI/API fallback이나 다른 이미지 모델로 전환하지 않았다.

최초 요청 프롬프트 `GENERATION_PROMPTS.json`, 실제 생성 프롬프트 `GENERATION_PROMPTS_KEYED.json`, 원화 `Sources/`, 원본 경로 `sources.json`, 후처리/검수/미리보기 스크립트 `build_ground_impact.py` 보존.

크기·투명 가장자리·중성색(키색 없음)·고리 중심투명·먼지 끝프레임 거의 투명 검증 통과. 실제 게임 배경 정지 프리뷰와 조각 시트 시각 검수. 자동검증 결과 `../Extracted_Draft/validation.json`.

게임 코드·타이밍·위치·Unity asset/meta를 변경하지 않았다. 현행 코드는 단일 Soft를 움직이고 사각 링을 Y축으로 압축한다. 본 시트의 재생 연결, 이미 납작한 고리의 비율 매핑, 어두운 RGB와 기존 tint의 곱은 적용 담당자가 확인할 항목으로 layout.runtime_handoff에 명시했다. 게임 내 적용 또는 플레이 검증 완료를 의미하지 않는다.

옵션인 색별 먼지는 흰 원본에 tint를 곱해 사용. 선택사항인 Crater_Glow는 포함하지 않았다.
