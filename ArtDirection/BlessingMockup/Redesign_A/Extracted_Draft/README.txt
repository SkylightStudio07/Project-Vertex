축복 A / SPLIT_REQUEST_A

1920×1080, 좌상단 원점. rect=x,y,width,height. SVG 논리 크기, PNG는2배.
신규10종. 모든 조각에서 글자·숫자·인물·배경·아이콘·빛·그림자 제외.
layout은 v1 호환 assets 배열, output_size, placement_1920_xy, nine_slice_lbrt, option_instances와 TMP text_boxes 등을 포함.

기본은 네 줄 모두 Normal. 제목은 모두 흰색 42px, 첫 글자 강조 없음.
Hover는 2px 청록선과 밝은 바탕, 줄 전체 왼쪽 8px 이동. glow/bloom/Shadow 없음.
Disabled는 판/마름모 교체 + 글자 #8C949E, 아이콘 alpha .5. Hover 이동 해제.
대사 진행 중 네 선택지 그룹 숨김. speakerAvatarImage는 꺼 둠.
대사 판은 174px 고정, 본문 90px / line-height29로 최대3줄. 4줄부터 페이지 분리, 아래 줄 위치 불변.

알파: 판 면 전체 0.90(바이트230). PNG RGB가 이미 의도색이므로 Image tint=(1,1,1,1).
Image 색에 다시 .9 알파를 곱하면 의도보다 밝아진다. Texture sRGB=true, straight alpha.
layout color_intent에 sRGB float와 linear RGB 값을 모두 제공.
미리보기는 Linear 색 공간의 source-over 계산 사용. 실제 프로젝트 머티리얼/톤매핑에 따라 차이는 남는다.

9-slice: assets[].nine_slice_lbrt는 2배 PNG SOURCE 기준 L/B/R/T. 논리값은 별도 nine_slice_logical_lbrt.
마름모/chevron은 고정 크기, sliced 금지.
외부 참조 8종: v1 Corner_Marks/Option_Divider/Nameplate_AccentLine/Icon_Plus 및 게임 흰색 아이콘4종.
외부 파일 중복 포함하지 않음. 원래 Redesign_A 폴더 아래에 풀어 상대경로 유지.
Divider/Plus는 기존 알파를 보존하면서 layout tint 적용. 마키나 이름판은 새 반투명 판이며 v1과 달라 재작성.

검수 목업은 캡처를 배치 기준으로 사용하고 상단 HUD를 캡처에서 재사용.
구 UI가 마키나 몸과 배경을 가리므로, 이음매 없이 게임 배경/리깅 첫프레임 원본으로 전체 장면을 조립.
당시 게임 애니메이션 캡처와 완전히 동일한 픽셀은 아님. 생성 모델로 원본을 새로 그리지 않았다.
States_Preview: 01 Hover / 02 Disabled 상태 확인용. 기본 목업은 전부 Normal.
Asset_Overview는 임포트용 아님. 캐릭터/배경/아이콘/모서리 원본은 패키지 제외.
Unity 적용 및 importer/meta 편집 없음.
