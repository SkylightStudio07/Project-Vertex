# 동료 스킬 분절 — 지시 목록 / 삼각 컷인

수정된 삼각 구도와 `Split_Handoff/SPLIT_REQUEST.md`에 따라 PNG27개를 제작했다. 목업의 전투 화면을 잘라 쓰지 않았다. 도형26개는 최종2배 캔버스에 새로 그렸고, 쇼 눈매1개는 **내장 image_gen으로 별도 직사각 원화를 새로 제작**했다.

## 출력 구성

- 버튼: Order_Button / Active / Disabled, Order_Diamond, Order_Badge.
- 목록: List_Panel, List_HeaderRule, Row_Normal / Hover, Row_ReadyBar, Face_Frame / Enhanced, Face_Mask, Face_Placeholder, Ready_Tag, Charge_Bar_Back / Fill, Mark_Enhanced / White.
- 컷인: CutIn_Mask, CutIn_Edge / Enhanced, CutIn_Watermark, CutIn_Eye_Cp01.
- 대사: Tag_Back / Enhanced, Tag_Divider.

모든 조각은 `scale: 2`이며 실제 PNG 크기는 JSON에 있다. 알파는 전체0/255다. 눈매도 이번에는 불투명 직사각으로 만들었다. 글자·전투 배경·HUD·캐릭터·적·손패는 PNG에 없다. 대사 글자는 확인용 Assembly JPG에만 넣었다. 목록의 쇼 얼굴은 별도로 추출하지 않았고 게임의 기존 얼굴 그림을 넣어야 한다.

## 좌표 / 목록

1920×1080 화면, 좌상단 원점, +X 오른쪽 / +Y 아래다. rect는 `[x,y,width,height]`다. 기본 배치는 **확정 Order 미리보기**를 따르며, 초안 요청서의 x240 위치와는 다르다.

- 버튼: `[90,848,270,44]`.
- 목록: `[86,628,390,216]`(3명), 버튼 위 간격4.
- 첫 줄: `[96,660,370,60]`, 반복62, 빈 간격2.
- 인원N에 따라 판 높이 `30+62N`, 판 top `848-4-height`. 행 위치와 머리도 판 이동량만큼 함께 옮긴다.

`list.offsets`는 **행 좌상단 기준**이다. 이름·스킬·READY·남은 수는 글자 박스·정렬·크기·보통/호버 색을 담는다. 첫 줄은 Row_Hover+ReadyBar+Ready_Tag, 둘째는3/4 충전, 셋째는1/4 충전+강화표식 예시다. 충전 막대 Back은18×16 화면 크기,4칸을21px 간격으로 배열한다. Fill은 각 칸 안쪽(2,2)에14×12로 놓는다.

얼굴 프레임은60×52, 안쪽 Mask는48×40이며 프레임 안쪽(6,6)에 둔다. 두 프레임 모두 같은 Mask를 쓴다. Normal과 Enhanced 가운데는 완전히 투명하다. 얼굴 없는 경우 Placeholder를 마스크 안에 넣는다. 준비 마름모는 흰색 조각이므로 런타임 tint로 청록/회색을 지정한다.

## 컷인 마스크와 눈매 규격

삼각 꼭짓점은 **(0,0), (800,0), (0,380)**. 사선은 `y=380-0.475x`, 화면 좌표 회전은 **약 -25.408°**(오른쪽으로 갈수록 위), Unity Z는 **+25.408°**다. 게임 상단·왼쪽 밖으로 붙는 형태다. 대사나 다른 장식은 이 마스크에 넣지 않는다.

눈매는1920×912 PNG이고 멈춘 화면 rect는 **[-80,-38,960,456]**이다. 마스크 바운딩 박스800×380보다 사방80/38px씩, 즉 각 방향10% 여유가 있다. 런타임에서 CutIn_Mask 아래의 자식으로 놓고 넘치는 부분을 숨긴다. RectMask2D는 삼각형을 만들지 못하므로 Sprite 기반 Unity Mask 또는 동일 삼각 클리핑 셰이더를 사용한다. Mask Graphic은 숨긴다.

다른 동료의 목표 규격도 같은1920×912, 같은 rect다. 눈 중심 목표(원화 픽셀)는 **왼쪽(557,447), 오른쪽(1114,264)**, 눈 하나의 폭은 약308px이다. 화면 목표는 각각(198.5,185.5), (477,94), 폭154px이다. 이는 제작 가이드 좌표이며 자동 랜드마크 실측값은 아니다. 눈·눈썹·앞머리·윗볼까지만 그리고 입과 코끝은 제외한다. 넓은 직사각 전체를 채우되 실제 노출은 삼각 마스크로 결정한다. 이번 원화는 확인 시트에서 두 붉은 눈이 모두 노출됨을 확인했다.

**해상도 출처:** 새 눈매 원화의 생성 원본은1820×864이고,1920×912 규격으로 약5.5% 리샘플했다. 기존 목업을 확대해 만든 것은 아니지만, 눈매까지 네이티브1920×912로 생성된 것은 아니다. 원본·수정 원화는 Split_Handoff의 Eye_Generated_Original.png / Eye_Generated_Final.png에 보관했다. 도형26개는 리샘플 없이 네이티브2배다. 프롬프트는 Eye_Generation_Prompts.json에 있다.

CutIn_Edge는 마스크 바깥으로16px씩 여유를 둔832×412 화면 캔버스이며(-16,-16)에 둔다. 기본은 먹색·회백색, 강화는 먹색 위 청록 이중선이다. 강화 워터마크는(566,4),54×54이며 삼각 안에 넣는다. 새 원화의 머리카락 위에 옅은 회색 선으로 그린다.

## 대사 태그

PNG는 수평570×44 화면 크기(1140×88)다. 양끝 사선과 왼쪽 청록 띠를 고정한 가로9-slice이며 강화판은 위 청록 헤어라인을 추가했다.

**태그 좌상단 기준점(120,342)**에서 화면 각도-25.408°로 회전한다. 높이 방향은 오른쪽 아래(전투 영역)로 뻗는다. 위쪽 가장자리는 컷인 경계 바깥에 약17px 수직거리로 떨어져 있으며 태그 네 모서리가 모두 전투 쪽에 있음을 검사했다. Unity에서는 좌상단 피벗과 anchoredPosition(120,-342), Z=+25.408°를 사용한다. 대사 길이에 따라 폭을 변경할 때도 왼쪽 기준점을 유지한다.

글자와 구분선 위치는 `cutin.tag.text_local` / `divider_local`에 있다. 글자도 태그의 자식으로 넣어 함께 회전한다. ORDER / 흡혈 / 청록 짧은 선 / 대사를 각각 분리한다. 기본·강화 대사 예시는 JSON에 포함되어 있고 확인용 JPG에서만 렌더링했다.

## Unity 임포트 / 9-slice

Sprite Single, PPU200, Full Rect, Mip Maps Off, Compression None, Clamp, Bilinear 권장. Canvas Reference Pixels Per Unit100에서 RectTransform 크기를 JSON의1배 값으로 명시한다. 일반 조각은 좌상단 피벗 기준이다.

9-slice는 JSON `nine_slice_px`에 **2배 PNG 픽셀**로 저장했다(키 left/bottom/right/top). Button은28/8/28/8, List_Panel은36/16/16/36, Row는8/4/8/4, Tag는52/6/52/6, HeaderRule은2/0/2/0이다. Panel은 높이를, Button/Row/Tag/Rule은 가로를 늘린다. 얼굴·마스크·컷인 경계·눈매는9-slice하지 않는다. border가 null이면 Simple이다.

## 확인 및 재생성

Contact_Sheet.jpg(27조각+눈매 확대), Assembly_Order.jpg, Assembly_CutIn.jpg, Assembly_CutIn_Enhanced.jpg를 제공한다. 전부 폭1120이며 체크무늬는 투명 영역 확인용이다. Assembly_Order에는 얼굴 대신 Placeholder를 넣었다. 실제 PNG에는 시트 글자·체크무늬가 없다.

validation.json에 RGBA·정확한2배 크기·4096 제한·이진 알파·변형 크기 일치·얼굴 프레임 투명창·태그의 마스크 바깥 배치 검사 및 해시를 기록했다. Unity 씬·임포터·게임 코드는 변경하지 않았으며 런타임 검증은 하지 않았다.

저장소 루트에서 `python ArtDirection/CompanionSkillMockup/Split_Handoff/build_split.py`로 재생성(Pillow·NumPy 필요). 저장된 Eye_Generated_Final.png를 사용하므로 재생성은 이미지 API를 호출하지 않는다. PNG27개·JSON·시트JPG를 다시 쓰고 README는 유지한다.

