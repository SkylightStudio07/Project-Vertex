# 상점 v2 분절

정본: Shop_Mockup_v2_Canonical.png (1672×941). 출력 좌표계: 1920×1080, 좌상단 원점.

## 제작 방식과 범위

원본은 스크립트에서만 읽었습니다. 판은 정본의 외형을 기준으로 복원하고, 글자·번호·카드·아이템·워터마크가 없는 깨끗한 여백에서 추출한 종이 질감을 채웠습니다. 따라서 원본을 사각형으로 자른 이미지가 아니라 **질감 복원판**입니다. 질감은 원본 샘플을 반전 반복하고 #F1F0EC에 맞춰 보정했습니다. 회색 불투명 그림자 띠(4px)는 각 큰 판에 포함되어 있습니다. 원본 파일은 변경하지 않았습니다.

워터마크와 기호는 원본 크롭의 종이 밝기를 제거해 분리했습니다. 아주 옅은 선화 특성상 미세한 종이 잡음이 남을 수 있습니다. 판과 셀, 가격표 알파는 0/255입니다. 워터마크는 원본 선 가장자리를 살리는 가변 알파를 사용하며 게임에서 전체 알파를 약 0.08부터 조절하십시오.

## 조립

- Panel_Cards/Supply/Service/Credits/Shopkeeper와 Button_Leave/Close에는 제목·숫자·가격·대사·기호가 없습니다. Panel_Cards의 등록 +, Credits의 청록 막대는 포함합니다.
- 이름판의 런타임 영문은 **SHOPKEEPER**입니다. 02 번호는 넣지 마십시오. 번호·제목·미세 영문·SOLD OUT 등은 모두 TMP로 넣습니다.
- Watermark_Cards/Supply/Service는 해당 판 위에 별도 레이어로 올립니다. 원본 위치는 JSON에 있습니다. Watermark_Service는 정본의 카드 삭제 선화입니다.
- PriceTag_Normal/Hover를 카드·아이템에 공용으로 사용합니다. 150×28을 기준으로 아이템에는 폭130으로 9-slice합니다. Icon_Credit/Arrow와 숫자는 별도입니다.
- ItemCell_Normal/Hover는 상품 아이콘 아래에 배치합니다. 상품 그림은 포함하지 않습니다.
- Icon_Close는 Button_Close, Icon_Leave와 Arrow는 Button_Leave, Icon_Scissors/RemoveCard는 서비스에 사용합니다.
- SoldOut_Stamp는 글자 없는 사선 먹색 띠와 해칭을 합친150×230 오버레이입니다. 더 세밀하게 제어하려면 별도 SoldOut_Hatching과 SoldOut_Band를 사용하십시오. 두 방식은 중복해서 올리지 않습니다.
- Disabled 서비스는 흐린 테두리이며, 글자와 기호도 게임에서 회색으로 조절하고 입력을 차단합니다.

## 좌표와 9-slice

layout.json의 source_crop_xywh는 원본 PNG 픽셀, placement_1920_xy는 화면 배치, output_size는 실제 PNG 크기입니다. 원본 크롭은 복원/추출에 사용한 참고 영역을 뜻합니다. 큰 판은 정본의 실제 비율로 출력했으며 이전 LAYOUT_BRIEF의 구역 치수와 다릅니다. 가격표·셀은 재사용 규격으로 정규화했습니다.

nine_slice_lbrt 순서는 Left, Bottom, Right, Top(출력 픽셀)입니다. 모서리 컷·그림자·등록 마크를 보존할 여백을 지정했습니다. PriceTag/ItemCell/Close는7px 경계입니다. 기호·워터마크·품절 해칭은 Simple로 사용합니다. 9-slice는 최소한 좌우/상하 경계 합보다 크게 사용하십시오.

## 재생성·검증

python ArtDirection/ShopMockup/Split_Handoff/split_shop.py

Contact_Sheet.jpg 한 장(1200px 폭)에 전 조각이 있습니다. PNG 규격, 원본 크롭 범위와 판 알파를 자동 확인합니다. Unity importer 및 .meta는 변경하지 않았습니다.
