헌터킬러 바닥 타격 VFX / GroundImpact

Crater_A/B/C: 380x190, center alpha max230(약.90), dark grayscale.
Dust_Puff: 2048x256, 8열1행, 프레임256x256, 여백0. 개별8프레임 Dust_Frames/ 제공.
Debris_01~06: 24,28,32,16,30,22px 정사각.
Shock_Ring:600x300, 흰색 RGB / alpha, 중앙 완전투명.
모든 pivot(.5,.5), PNG RGBA straight alpha, 필터 Bilinear, wrap Clamp, mipmaps off.
크기/피벗/프레임 rect/layout.json 참조. 2배판은 업스케일만 될 것이므로 추가하지 않음.

먼지·고리는 백색RGB에 알파로 농도 표현하므로 게임에서 색 곱하기 가능.
구덩이·돌은 요청대로 어두운RGB 유지. 기존의 매우 어두운 tint를 다시 곱하면 지나치게 어두워짐.
내장 imagegen 투명출력이 지원 오류여서 마젠타/검정 키로 생성 후 알파 추출·중성화·디스필.
원화는 별도 GroundImpact/Sources. 최종 PNG에는 단색 배경/키색/글자/불꽃 없음.

코드·타이밍·위치·씬·meta 변경 없음.
현재 GroundImpactFx.cs는 단일 Soft 스프라이트를 움직이며 시트 애니메이터가 없다. 이8프레임 시트의 실제 재생 연결은 후속 통합 작업.
Shock_Ring은 이미 납작하므로 기존 사각 Ring의 Y축.38 스케일과 이중 압축 주의.
현재 Sprite 자체가 코드 생성 private 프로퍼티이므로 이 산출물만으로 게임 자동 교체가 된 것은 아님.
타이밍/위치 동작은 원래 코드가 기준. GIF는 요청서의 24fps 타격프레임 15,21,27,33,42,45를 보여주는 검수용.
선택사항인 푸른/보라 먼지는 런타임 tint로 처리, Crater_Glow는 생략.
검증: 크기, 투명 가장자리, 키색 없음, 고리 중심투명, 마지막 먼지 거의 투명.
