# 전투 연출 분절 — 2배 해상도

`Split_Handoff/SPLIT_REQUEST.md`에 따른 산출물. 6개 폴더, RGBA PNG 56개, 폴더별 `layout.json`과 폭 1120px `Contact_Sheet.jpg`을 제공한다.

## 제작 및 좌표 규칙

- 미리보기 JPG를 참고해 도형을 **2배 캔버스에 새로 래스터화**했다. 원본 PNG의 픽셀을 잘라 붙이거나 확대하지 않았다. 원본은 스크립트로 크기·해시만 확인했다.
- 목업의 손그림/생성 이미지 경계를 정수 좌표로 정리한 재구성이다. 종이와 먹색의 미세 질감도 고정 시드로 새로 합성했다. 원본과 픽셀 단위로 동일한 추출물은 아니다.
- 모든 실제 조각에는 글자·숫자·화살표·배경·HUD·캐릭터·카드 아트·화면 암막이 없다. 확인용 시트의 이름과 체크무늬는 PNG에 포함되지 않는다.
- `screen_rect` 및 `text_slots`는 **1920×1080, 좌상단 원점, 오른쪽 +X / 아래쪽 +Y**, `[x,y,width,height]`이다. 각 조각의 `scale: 2`, `png_size_px`는 실제 저장 크기다.
- `nine_slice_px`는 **2배 PNG 픽셀**이며 키는 `left/bottom/right/top`이다. 가로 늘리기는 높이를 고정한다. Panel만 양축 늘리기를 허용한다.
- `assets`의 기본 배치에 `instances`의 추가 복제 배치를 더한다. 단 KillFeed는 `instances`만으로 3줄을 조립한다. 상태 변형은 하나만 선택한다. `screen_size`가 있으면 그 크기로 배치하고, 없으면 PNG 크기의 절반을 사용한다.
- `text_slots`는 글자를 별도로 얹을 영역이다. 글꼴·글자 크기는 실제 문자열과 런타임 폰트에 맞춰 설정한다. KillFeed의 `repeat_y`는 각 줄의 Y 증가량이다.
- 모든 알파는 0/255이며 Victory/Flash만 부드러운 알파다. 선의 기본 두께는 PNG 2px = 화면 1px이다. 강조 띠는 의도적으로 더 두껍다.

## Unity 임포트

Sprite / Single, **PPU 200**, Full Rect, Mip Maps Off, Compression None, Filter Bilinear, Max Size 4096 권장. Sprite Editor Border에 JSON의 2배 값을 그대로 입력한다. 여기서는 ArtDirection 인계 파일만 제작했으며 Unity 임포터·씬·프리팹 적용은 수행하지 않았다.

uGUI Canvas의 Reference Pixels Per Unit은 100 기준이다. RectTransform을 JSON의 화면 크기로 지정하고 Image의 Pixels Per Unit Multiplier는 1로 둔다. 좌상단 anchor/pivot에서는 `anchoredPosition = (x, -y)`. Sliced를 쓸 때도 JSON의 화면 크기를 유지한다. 1px 선에는 Preserve Aspect를 끄고 가로만 늘린다. 9-slice 경계 합보다 작은 크기로 줄이지 않는다.

## Start

Band, Line_Top/Bottom, Slash_Accent, Tag, Tag_EliteMark, Tick을 독립 PNG로 제공한다. Label_Line과 Title_Slash도 분리했다. 먹색 띠의 양쪽 사선은 9-slice 고정 가장자리에 포함된다. ELITE 글자는 별도로 올린다.

## KillFeed

Row, Row_Accent, BossChip, Separator. 모든 줄의 기본 크기를 590×50 화면 픽셀로 통일했다. **반복 간격 58px, 줄 사이 빈 간격 8px**. BossChip은 세 번째 줄에만 조립 예시를 넣었다. 짧은 행은 Row를 가로 Sliced로 줄이고 오른쪽 칩 위치를 함께 이동할 수 있다.

## CutIn

Frame, Label_Plate, Chip은 Power/Unique 변형끼리 각각 동일 PNG 크기다. 프레임은 2280×400이며 Unique의 짧은 오른쪽 끝 뒤는 투명 여백이다. Unique 이중 테두리는 프레임에 포함하고, 눈금은 Ticks_Unique로 분리했다. Line과 Diamond는 두 프레임에 복제한다.

`art_window`에는 PNG 내부 2배 다각형과 화면 전체 기준 1배 다각형, 각 바운딩 박스를 기록했다. 카드 아트는 프레임 뒤에 놓고 **다각형 창 모양으로 마스킹**해야 한다. 사각형 바운딩 박스만으로 자르면 사선 아래에 아트가 튀어나온다. 두 목업의 창 형태 차이는 유지했다. 프레임은 9-slice 대상이 아니다.

## Victory

종이 Band, 청록 Line_Top/Bottom, 중앙 Slash, 끝 Slash_Edge/Short, 먹색 Tag, Diamond, Label_Line, Title_Line, Flash. 아래 선은 좌우 두 인스턴스로 나누어 태그 주위를 비운다. Flash는 단일 흰 섬광이며 확인용 조립 화면에서는 숨겨 두고 개별 썸네일로 표시한다. 연출 시 별도 레이어에서 짧게 재생한다.

## Defeat

Victory와 같은 크기·위치의 먹색 Band / 회색 선·Tag·Diamond. Flash는 없다. 화면 암막은 게임에서 따로 만든다.

## Result

공통 종이 Panel은 사선 컷, 불투명 오프셋 그림자, 모서리 십자 장식을 포함한다. Rule_Top/Bottom과 Stat_Divider는 독립 조각이다. Stat_Divider 하나를 총 3회 배치해 4칸을 만든다.

Button_Return은 Normal/Hover/Pressed 모두 780×172 PNG다. 목업에 없는 Hover/Pressed는 같은 실루엣을 유지하면서 먹색 밝기만 조정한 상태 디자인이다. 청록 하단 선은 요청대로 버튼에 포함되며 실패 상태에서도 유지한다.

Accent_Clear/Defeat는 제목 영역의 짧은 선, Accent_Stat_Clear/Defeat는 기록 칸 선, Accent_Corner_Clear/Defeat는 판 모서리 상태 선이다. 상태에 맞는 세 종류를 함께 교체한다. 보고서 영문 라벨·제목·부제·4개 항목명과 숫자·버튼 주/보조 글자·번호의 자리를 JSON에 기록했다.

## 검증 및 재생성

`validation.json`에 56개 PNG의 크기, 알파 방식, SHA-256 및 원본 해시가 있다. RGBA/정확한 2배 크기/알파 규칙/9-slice 경계/투명 창/변형 크기/시트 폭을 자동 검사했고, 6개 시트를 육안 확인했다. Unity 런타임 검증은 포함하지 않는다.

저장소 루트에서 `python ArtDirection/BattleFxMockup/Split_Handoff/build_split.py`로 재생성한다(Pillow, NumPy 필요). 이 명령은 해당 산출물 PNG·JSON·JPG를 다시 쓴다. README는 유지된다.
