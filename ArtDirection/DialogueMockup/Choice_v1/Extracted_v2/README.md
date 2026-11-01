# Choice v1 — raster texture v2

교체용 PNG 10개. 기존 Extracted_Draft와 파일 이름, 픽셀 크기, 알파 채널이 완전히 동일하다. 기존 layout.json과 9-slice 설정(판 사방 68px)을 그대로 사용한다. 새 SVG나 layout.json은 생성하지 않았다.

Normal의 종이 및 Hover/Selected의 무광 잉크 표면은 내장 imagegen으로 생성한 래스터 질감을 사용한다. 원본은 ../Generated_Texture_v2/Paper.png 및 Ink.png, 프롬프트는 같은 폴더의 PROMPTS.txt에 보관했다. 생성 질감의 조명 편차를 제거하고 대비를 낮춘 뒤 기존 PNG의 채움 영역에 적용했다. 테두리, 알파, 선 두께는 보존했다.

Shadow, Ghost, SelectBand 및 흰색 tint용 네 조각은 규격상 단색·정확한 알파를 유지해야 하므로 기존 픽셀을 유지했다. 이 일곱 조각을 이미지 모델로 새로 그렸다고 주장하지 않는다.

중앙 질감은 334px 주기로 좌우 반전 연결하여 반복 가능하게 구성했다. 중앙 1336px는 정확히 네 주기이며 68px 경계에 맞춰 정렬했다. 큰 얼룩이나 글자는 없다. Contact_Sheet.jpg는 개별 10조각, 실제 9-slice의 736/960px 확장, 28px 글자 샘플을 담는다. 글자는 확인 시트에만 있으며 조각에는 없다.

validation.json: 모든 PNG 크기 및 전체 알파의 원본 일치 검증 결과. 재현: 저장소 루트에서 python ArtDirection/DialogueMockup/Choice_v1/build_texture_v2.py 실행. 기존 파일 및 게임 프로젝트 적용 상태는 변경하지 않았다.
