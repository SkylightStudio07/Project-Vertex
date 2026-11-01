# CompanionSkill — raster v2

다시 그림: Tag_Back.png, Tag_Back_Enhanced.png

원본 바이트 그대로 복사: Charge_Bar_Back.png, Charge_Bar_Fill.png, CutIn_Edge.png, CutIn_Edge_Enhanced.png, CutIn_Eye_Cp01.png, CutIn_Mask.png, CutIn_Watermark.png, Face_Frame.png, Face_Frame_Enhanced.png, Face_Mask.png, Face_Placeholder.png, List_HeaderRule.png, List_Panel.png, Mark_Enhanced.png, Mark_Enhanced_White.png, Order_Badge.png, Order_Button.png, Order_Button_Active.png, Order_Button_Disabled.png, Order_Diamond.png, Ready_Tag.png, Row_Hover.png, Row_Normal.png, Row_ReadyBar.png, Tag_Divider.png

내장 imagegen으로 생성한 Paper/Ink 소재를 원본 PNG 채움 영역에 적용했다. 새 SVG나 도형 재생성은 없다. 소재 원출력과 실제 프롬프트는 ArtDirection/RasterRedo_v2/Generated_Materials/에 보관했다. 저주파 조명 편차를 제거하고 약한 질감으로 정규화했다. 

9-slice는 기존 layout.json의 nine_slice_px 값을 읽어 사용했다. 생성 소재를 좌우·상하 반전 주기로 연결하고 중앙 영역에 정수 주기로 맞춰 고정 영역과 늘어나는 영역 사이의 연속성을 유지했다. 큰 얼룩·문자·그림을 추가하지 않았다.

검증: 원본 폴더의 PNG 27개를 모두 포함. 전부 이름·크기·알파 완전 일치. 복사 파일은 바이트 일치. 변경 파일의 채움 밖 픽셀(그림자, 모서리 십자, 테두리, 청록 선, 투명 창 등)도 완전 일치. validation.json 참조.

Contact_Sheet.jpg: 전/후 비교, 일부 소재 확대, 9-slice 1.3배 확장, 기존 좌표로 조립한 확인 화면. 텍스트/카드 아트는 조각에 포함하지 않는다.

layout.json은 새로 만들지 않았다. 기존 ..\Extracted\layout.json 사용. 게임 Assets 파일은 변경하지 않았다.
