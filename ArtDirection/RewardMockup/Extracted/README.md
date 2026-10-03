# 전투 보상 화면 — 2배 분절

확정 미리보기 2장을 기준으로 새로 그린 PNG 16개다. 원본 목업을 자르거나 확대하지 않았다. 도형·선화·종이 질감 모두 실제 출력 해상도에서 새로 만들었다. 원본 PNG는 스크립트로 크기와 해시만 읽었다.

## 산출물

| 분류 | 조각 |
|---|---|
| 판·머리 | Panel, Panel_Watermark, Rule, Header_NoTick |
| 보상 줄 | Row_Normal, Row_Hover, Row_NumTick |
| 아이콘 | Icon_Slot, Icon_Gold, Icon_Card, Icon_Arrow |
| 버튼 | Button_Primary, Button_Primary_Hover, Button_Secondary, Button_Secondary_Hover |
| 카드 받침 | Card_Plinth |

모든 PNG는 RGBA, 화면 크기의 정확히 2배, 알파 0/255다. 기본 헤어라인은 PNG 2px, 호버 테두리와 아이콘 선화 등은 4px이다. 색은 종이 #EEF0F2 / 먹색 #16181B / 청록 #0DB8F2를 기준으로 한다.

아이템 줄도 목업에서 먹색 칸을 쓰므로 Icon_Slot을 공유한다. Icon_Slot_Item은 만들지 않았다. 아이템 그림·회색 카드 틀·글자·전투 배경·암막은 포함하지 않는다. 계속 버튼의 화살표는 TMP 문자열로 넣는 방식이며, 별도 흰 화살표도 만들지 않았다.

## 좌표와 9-slice

`layout.json`은 1920×1080 화면 기준, 좌상단 원점, 오른쪽 +X / 아래 +Y다. rect는 `[x,y,width,height]`다. 각 조각에 scale=2, 실제 PNG 크기, 기본 화면 rect를 기록했다. **장면 조립은 `scenes.reward`와 `scenes.card`의 instances를 기준으로 한다.** assets의 기본 rect는 조각의 대표 배치다.

Panel 하나를 보상 목록 `[482,154,1000,828]`, 카드 선택 `[424,152,1110,800]`에 각각 Sliced로 사용한다. 판의 9-slice는 **2배 PNG 기준 Left 216 / Bottom 104 / Right 216 / Top 216**이다. 72px 사선 컷, 모서리 십자와 가장자리 선은 고정 테두리 안에 들어간다. 네 모서리를 크기 변경 전후 픽셀 단위로 비교했다.

Row의 경계는 L56/B8/R60/T8, Button은 네 방향 모두16, Rule은 L2/B0/R2/T0이다(모두 PNG 픽셀). Row와 Rule은 가로만 늘리고 높이를 유지한다. 나머지 조각은 9-slice가 필요 없어 null이다. UI 크기는 테두리 합보다 크게 유지한다.

워터마크는 판에 포함하지 않고 별도 조각으로 둔다. #E7E9EB 색과 이진 알파로 종이보다 약 3% 어둡게 그렸으며, 판을 늘려도 워터마크 크기는 유지한다. JSON에 판 오른쪽 위를 기준으로 한 위치도 기록했다.

## 보상 줄과 글자

첫 줄 `[536,390,908,144]`, 반복 간격154px, 줄 사이 빈 간격10px이다. 두 번째 줄만 Row_Hover를 사용한다. 글자 번호와 번호 밑줄은 분리했다.

줄 안의 모든 위치는 `rows.offsets`에서 **줄 좌상단 기준 1배 rect**로 제공한다. 번호·밑줄·칸·아이콘·이름·설명·화살표를 포함한다. 설명 없는 골드는 `name_without_description`, 설명 있는 카드·아이템은 `name_with_description`과 `description`을 쓴다. 이름·설명·번호의 글자 크기와 색도 별도로 제공한다.

Icon_Slot은 미리보기 실측에 맞춰 화면110×110(220×220 PNG)이다. 구급키트는 이 칸 안의94×94 영역에 넣고 상하좌우8px 여백을 둔다. 아이콘 선화는80×80 화면 크기다. 아이템 그림은 데이터에서 가져와야 한다.

머리 영문·한글·번호, 바닥 상태·버튼 글자에는 박스·정렬·대략 글자 크기·색을 기록했다. 실제 TMP 폰트와 문자열에 맞춰 최종 크기를 조정한다. 호버 버튼은 목업에 없는 파생 상태로, 주 버튼은 먹색을 약6% 밝게 하고 청록 선을 두껍게 했으며 보조 버튼은 테두리를 청록으로 바꿨다.

## 카드 선택

실제 카드 자리는 `[486,378,294,400]`, `[832,378,294,400]`, `[1180,378,294,400]`이다. 회색 틀은 PNG로 만들지 않았다. 받침은 가운데 숫자 영역을 완전히 비운 양쪽 선만 포함한다. 카드 3장 자리·받침·번호 글자 자리는 JSON에 각각 들어 있다. Assembly_Card의 넓은 빈 공간은 실제 카드와 글자를 제외한 정상 상태다.

## Unity 임포트

Sprite / Single, PPU200, Full Rect, Mip Maps Off, Compression None, Max Size4096, Bilinear 권장. Canvas Reference Pixels Per Unit100 기준으로 RectTransform은 JSON의 1배 크기를 사용한다. Sprite Editor Border에는 JSON의 2배 PNG 값을 그대로 입력하고 Panel·Row·Button은 Image Type Sliced를 쓴다. 좌상단 피벗에서 anchoredPosition은 `(x,-y)`다.

이번 작업은 ArtDirection 인계 파일 생성까지다. Unity 임포터·씬·프리팹은 변경하지 않았으며 런타임 검증은 수행하지 않았다.

## 확인 및 재생성

- Contact_Sheet.jpg: 조각 16개 확인, 폭1120.
- Assembly_Reward.jpg: 글자·아이템 그림 없는 보상 목록, 폭1120.
- Assembly_Card.jpg: 같은 Panel을 실제 9-slice로 다른 크기에 조립, 폭1120.
- validation.json: RGBA·2배 크기·이진 알파·변형 크기 일치·9-slice 네 모서리 보존·파일 해시 검사 결과.

확인 시트의 체크무늬와 이름은 게임 PNG에 포함되지 않는다. 저장소 루트에서 `python ArtDirection/RewardMockup/Split_Handoff/build_split.py`로 재생성한다(Pillow·NumPy 필요). PNG·JSON·확인용 JPG는 다시 쓰며 README는 유지된다.
