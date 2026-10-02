# Lobby UI v2 — 목업 유지 기준본

내장 이미지 모델로 승인된 목업 자체를 편집했다. 기존 v1은 개별 재생성 과정에서 패널 비율과 프레임이 달라졌으므로 적용 기준으로 사용하지 않는다. v1 파일은 비교용으로 보존한다.

`Lobby_Textless_Master.png`: 원래 화면의 배치, 패널 경계, 컴퍼스, 모닥불 그림, 시설 아이콘을 유지하고 UI 글자를 제거한 비교 기준본. 배경과 캐릭터가 포함된 전체 이미지이며 개별 스프라이트나 투명 UI 레이어가 아니다. 대사 영역의 바탕도 편집 과정에서 사라졌으므로 원본 배치를 참고해야 한다.

생성 프롬프트 요지: Precise text removal edit of the exact approved lobby mockup. Preserve composition, panel boundaries, spacing, scale, icons, artwork and UI geometry. Remove all UI typography and inpaint with underlying paper/graphite texture. Keep compass, gear, gauge, arrows, campfire, facility icons, corner marks, cyan rule and diagonal planes. No redesign, new outlines, inset frames, thicker borders or enlarged icons.

후속 부품 제작은 이 기준본의 동일 좌표/실루엣을 기준으로 진행해야 하며, v1처럼 각 패널을 독립적으로 재해석하지 않는다. 아직 개별 부품과 호버 수정 및 Unity 적용은 완료되지 않았다.
