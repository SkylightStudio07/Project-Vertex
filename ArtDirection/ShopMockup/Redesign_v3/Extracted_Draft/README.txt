상점 v4 / SPLIT_REQUEST_v3 반영

1920×1080, 좌상단 원점, rect=x,y,width,height. SVG 논리 크기, PNG UI 2배 / Shop_BG 1배.
신규 조각에는 글자·숫자·토냐·배경 그림·카운터·카드·보급품 그림·그림자가 없다.
Shop_BG는 오른쪽 기하 면만 포함하며 왼쪽 690px 및 상단 106px는 투명.
가격표 PriceTag 9-slice: source PNG L/B/R/T=24/24/24/24; 논리값은12px.
Panel/Plinth/ItemCell은 고정 크기 또는 비율 유지; 임의 9-slice 금지.

외부 재사용: ../../Extracted/의 아이콘 6종, 품절 3종, 워터마크 3종, 닫기 상태 3종.
외부 PNG는 기존 크기 그대로 재사용한다. 신규 SVG/2배 PNG 요구는 새로 만든 조각에 적용.
ZIP 단독으로 모든 런타임 자산이 포함되는 것은 아니다. 원래 Redesign_v3 위치에 풀어 외부 상대경로를 유지한다.

그림자는 UnityEngine.UI.Shadow만 사용. 큰 판 offset(12,-12), 먹색30%; 받침/칸8px, 가격표6px 먹색24%; Use Graphic Alpha. PNG 그림자를 더하지 않는다.
상태 우선순위: SoldOut > Unaffordable > Hover > Normal. 구매 불가 상태에서도 툴팁 사용 가능.
돈 부족 시 받침/칸/가격표 모두 Unaffordable. 가격은 ShopStockEntry.unaffordableColor, 미리보기는 코드 기본값 Color.red.
카드 제거는 이미 사용했거나 돈 부족이면 Disabled. 나가기 Pressed는 그림자6px.
품절은 기존 마스크 3종과 TMP SOLD OUT을 겹친다. 실제 rect는 layout templates.sold_out의 정규화 좌표를 content rect에 적용.

보급품 이름은 실제 ItemData 명칭, 16px, 한 줄 말줄임. 가격표와 분리.
크레딧 숫자 앞에도 기존 Icon_Credit을 사용한다.
TMP 폰트 Pretendard, text_boxes에 크기·색·정렬·자간·바인딩 기록.
제목 y118~210, 퀘스트 자리 y222~332. 상단 HUD는 y0~106.

Shop_Mockup_v4.png: 프로젝트 토냐 원본/배경/카운터/아이템 + 제공 게임 캡처 카드 + 신규 조각 조립본.
카드는 AI 목업에서 잘라내지 않았다. 원본 게임 캡처의 해상도 한계는 존재한다.
목업에만 HUD/퀘스트 캡처가 있으며 해당 요소는 조각에 포함되지 않는다.
Asset_Overview.png는 검수용으로 임포트하지 않는다.
Unity 적용 및 meta 수정은 하지 않았다.
