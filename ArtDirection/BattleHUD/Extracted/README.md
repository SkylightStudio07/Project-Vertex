# Battle HUD v3 분절 결과

`Split_Handoff/SPLIT_REQUEST.md` 기준. 원본 PNG는 디스크에서만 읽었고, 모든 조각에서 글자와 숫자를 제외했습니다.

## 추출 방식

기호(Map/Deck/Energy/Ammo/Arrow)는 지정된 원본의 밝기 마스크로 분리했습니다. MAP은 TurnBanner의 접힌 지도입니다. 권총은 기존 `Canonical_PistolIcon_Model.png`에서 분리했습니다. 판·HP·방패·헤어라인은 원본 위치와 형태를 기준으로 지정 색상으로 재구성했습니다. 따라서 배경이나 글자가 남는 단순 사각 크롭이 아닙니다. 생성형 이미지 도구는 사용하지 않았습니다.

모든 PNG의 알파는 0 또는 255입니다. 판은 #16181B, 안쪽 선은 #3A3E44, 기호는 #F2F3F4입니다. 비활성 END TURN은 명세의 #202328, 비활성 기호는 #5A6069입니다. HP 채움은 원본의 연회색 #C9CED4입니다. 청록은 테두리·선·마름모에만 사용했습니다.

## 조립

- `layout.json`의 `source_crop_xywh`는 원본 픽셀 좌표, `output_size`는 요청 출력 크기, `placement_1920_xy`는 좌상단 기준 배치입니다. `reference_display_size_1920`는 목업에서 차지하던 크기입니다. 지정된 출력 크기와 목업 비율은 일부 다르므로 두 값을 구분했습니다.
- 버튼 판 위에 Icon_Map/Deck 또는 Icon_Arrow와 EndTurn_CornerSlash를 올리고 글자는 게임에서 넣습니다.
- Res_Panel_Normal/Empty는 같은 먹색 판입니다. 적 턴은 에너지 숫자 0, Icon_Energy_Disabled, Button_EndTurn_Disabled, Icon_Arrow_Disabled로 표현합니다. 작은 3 위 청록 선은 없습니다.
- Res_Divider는 판 폭과 같은 264×2입니다. 구분선 2개의 위치는 JSON에 기록했습니다. 에너지 사선은 Res_Slash입니다.
- HP는 Track → Fill → Frame → 런타임 숫자 순서입니다. Frame의 `fill_offset`에 Track/Fill을 배치합니다. Fill은 전체 폭이므로 체력 비율을 적용합니다. 플레이어 청록 끝선은 UV 크롭보다 가로 크기 조절로 유지하십시오.
- Block_Badge/EnemyBlock_Badge는 숫자 없는 방패입니다. Quest의 왼쪽 아래 유형 배지는 별도의 인벤토리 그림과 구분되는 작은 상자 기호입니다.
- EnemyName_Band/TextBand 위에 런타임 글자를 올립니다. Banner_Slash/Line/Diamond는 TurnBanner의 세 장식이며 PLAYER TURN은 포함하지 않습니다.

## 9-slice

JSON `nine_slice_lbrt` 순서는 Unity와 같은 Left, Bottom, Right, Top이며 출력 픽셀 단위입니다. TextBand/EnemyName_Band는 4,4,4,4; 버튼과 기본 슬롯은 3,3,3,3; Quest는 배지 보존을 위해 18,18,3,3입니다. HP_Frame은 실제 창 여백이며 Fill은 2,0,2,0입니다. TopBar는 양 끝 컷 보존을 위해 52,0,52,0이며 높이 72 고정 사용을 권장합니다. 기호와 배너 조각은 Simple로 사용합니다.

이 작업은 ArtDirection의 PNG 납품이며 Unity importer/.meta 설정은 변경하지 않았습니다.

## 재생성 및 확인

`python ArtDirection/BattleHUD/Split_Handoff/split_hud.py`

확인은 `Contact_Sheet.jpg` 한 장(1200px 폭)으로 합니다. 원본이나 전체화면 조립 이미지는 추가로 열 필요가 없습니다.
