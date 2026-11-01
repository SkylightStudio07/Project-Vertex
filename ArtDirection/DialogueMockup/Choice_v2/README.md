# Dialogue Choice v2 — image model revision

최신 REQUEST.md 기준 1920×1080 세 상태. 토냐는 원래 자리·크기·밝기로 유지하고 주인공은 원본 PNG를 배치했다. 상점과 HUD는 65% 먹색, 하단 대사창은 약하게만 어둡게 했다.

v1 Extracted_v2 PNG와 원본 캐릭터로 래스터 가이드 작성 → 내장 imagegen으로 세 상태 각각 생성(꼬리, 청록 사선, NPC 가장자리 정리) → 원본 캐릭터와 판·문자를 재합성하여 변형 보정. SVG 생성이나 렌더링은 사용하지 않았다. NPC 외곽 접합부에는 모델 보정이 남으며 양쪽 얼굴 내부는 원본 픽셀 일치를 검증했다.

ImageModel_v2/Generated_*.png는 모델 원출력, Guide_*.png는 입력, PROMPTS.txt는 실제 프롬프트. composition_manifest.json에 검증 및 배치를 기록했다. Previous_Composition/에 이전 결과 보관.

이전 build_choice_mockups.py는 사용하지 않는다. 최신 스크립트는 build_image_model_guides.py 및 finish_image_model_v2.py. 이번 단계는 목업이며 꼬리 분절은 포함하지 않는다.
