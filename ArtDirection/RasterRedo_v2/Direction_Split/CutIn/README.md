# CutIn / Direction Split

2배 RGBA PNG 11개. 좌표는 정본 1672×941에서 1920×1080으로 환산했다. layout.json의 screen_rect는 화면 좌표이며 png_size_px는 정확히 두 배다. 판 고정 크기; nine_slice_px가 있는 Accent_Bar/Tag만 가로 크기 변경 가능. PNG는 회전 0도.

SVG 미사용. Generated의 이미지 모델 clean plate를 표면으로 사용하고 외곽/투명 창은 래스터 알파 마스크로 자른다. 표면을 단색 도형으로 재구성하지 않았다. 나침반은 모델로 별도 추출한 문양, 컷인 카테고리 문양은 정본의 래스터를 밝기 기반으로 분리했다. 그림자·작은 선·십자·막대·마스크·덮개만 규격에 맞게 후처리한다. 패배 표면은 생성 패배 목업을 기준으로 같은 clean plate를 먹색으로 매핑해 알파와 기하 구조를 유지했다.

실제 프롬프트: ../Generated/PROMPTS.txt. 생성 원출력도 같은 폴더. 정본과 분절판의 세부 종이결/제도선은 재생성으로 차이가 있으며 픽셀 단위 복원은 아니다. 구조·텍스트 배치·주요 기하 면과 문양 위치를 맞춘 게임 적용용 분리안이다.

조립 순서: 그림자 → 판 → 사진/카드 아트(지정 마스크) → 덮개 → 문양 → 테두리/태그/막대 → 런타임 글자. CutIn 아트는 Frame 뒤에 두며 Art_Fade는 그 위. Result Strip_Mask는 표시용이 아닌 마스킹 전용. Result 사진은 게임의 전장 이미지를 사용한다. 그림자 runtime_opacity는 PNG 알파 외에 한 번만 적용한다. 문양 농도는 soft 알파에 이미 포함되어 있다.

Result의 Defeat 접미사 파일은 해당 clear 파일과 교체. 그 외 레이어 공유. CutIn 문양은 세 종류 중 하나만, Unique_Border는 유니크일 때만. 모든 텍스트는 text_slots로 분리; PNG에 글자/숫자/화살표/캐릭터/카드 아트/전장 사진 없음. 확인용 Reassembled JPG에는 글자와 아트가 들어 있다. 카드 창 꼭짓점은 CutIn layout의 art_window 참조.

검증: 모든 파일 크기/알파 모드 검사 통과. 이진 레이어 알파는 0/255만 사용. 정본 옆 재조립 비교와 조각 단독 미리보기는 Contact_Sheet.jpg. 게임 Assets나 기존 조각은 변경하지 않았다.
