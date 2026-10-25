# 축복 A안 · 분절 v2

`SPLIT_REQUEST_A.md` 및 `Blessing_A_Canonical.png` 기준. 첫 음절 강조를 사용하지 않는다.

- `Blessing_A_Mockup_v2.png`: 1920×1080 기본 검수본. 네 줄 모두 Normal, 제목 흰색42px, 초상 칸 없음.
- `Extracted_Draft/`: 신규 SVG/2배 PNG 10쌍, v1 및 게임 아이콘 외부 참조 8종, `layout.json`, `README.txt`, `validation.json`, `Asset_Overview.png`, `States_Preview.png`.
- `Blessing_A_v2_Package.zip`: 수정 목업과 위 폴더 전체. 외부 자산은 중복 포함하지 않으므로 원래 폴더 구조를 유지한다.
- 대사 판·선택지 판·이름판은 면 전체 알파0.90. 의도 sRGB 및 Linear RGB, Image tint 설정을 layout에 명시했다. PNG RGB에 색이 포함되므로 Image는 백색/알파1.
- 선택지 모양은 오른쪽 아래 사선 컷 한 종류, 동일 크기. Hover는2px 청록 테두리와 밝은 바탕 및 줄 전체 -8px 이동. glow/bloom/그림자 없음.
- 대사 본문은 고정90px에 line-height29, 최대3줄. 판174px 고정이며 4줄부터 페이지 분리.
- 대사 진행 중 선택지 숨김, Disabled 글자/아이콘 흐리게 처리.
- `layout.json`은 v1의 assets 배열과 output_size, placement_1920_xy, nine_slice_lbrt, option_instances를 유지하고 모든 TMP rect 및 상태 전이를 추가했다. nine_slice_lbrt는 **2배 PNG 원본 픽셀** 기준, 논리 border도 별도 기재.

게임 캡처가 몸과 배경을 구 UI로 가리고 있으므로 최종 목업은 캡처 배치를 기준으로 프로젝트 배경·리깅 첫 프레임을 이어 붙이지 않고 전체 조립했다. 캡처의 HUD는 그대로 사용했다. 당시 애니메이션 캡처와 픽셀 단위로 같지는 않으며, 실제 적용 시 기존 `characterImage/backgroundImage/BlessingAmbienceFx`를 그대로 유지한다. 생성 모델로 아트를 새로 그리지 않았다.

PNG 크기·투명도·균일 알파, 글자·임베디드 그림·필터 미포함, 9-slice 경계, TMP 폭, 외부 경로, 기본 네 줄 Normal을 검사했다. 기본/상태/조각 시트를 시각 검수했다. Unity 씬·임포터·meta 파일은 변경하지 않았다.

재생성: `python ArtDirection/BlessingMockup/Redesign_A/build_blessing_a.py` (Pillow, NumPy, Node/resvg).
