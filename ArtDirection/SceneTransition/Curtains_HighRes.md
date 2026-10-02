# 전환 막 고해상도 교체

내장 imagegen으로 상단·하단 막을 각각 단독 생성. 모델 출력에서 흰 여백을 잘라 2320×548에 맞췄다. Extracted/Curtain_Upper.png, Curtain_Lower.png 교체. 기존은 *_LowRes_Backup.png 보존. 내부 불투명, 사선 컷 외부 투명. 원본 시트 장식의 픽셀 복제는 아니며 같은 방향의 새 고해상도 아트다.

재생성: Export-CurtainsHighRes.ps1. 기존 Split-SceneTransition.ps1을 다시 실행한 뒤에는 이 스크립트도 실행해야 교체본을 유지한다. 기존 예시 그림은 앞선 조립본이다.

## 생성 프롬프트

### Upper

Use case: precise-object-edit / UI texture. Reference is design style ONLY. Generate ONE standalone HIGH RESOLUTION scene transition curtain, NOT a contact sheet. Output canvas3072x1024, curtain artwork spans full width and vertical y149..875 (726 high), giving panel approximately2320:548 aspect. Outside above/below panel is plain flat WHITE staging margin for cropping. No headers, dimensions, arrows, sample labels, typography or annotations anywhere. Panel interior fully OPAQUE near-black #090A0D to #16181B, subtle barely visible diagonal paper hatching and grid, extremely crisp raster detail. Thin restrained graphite seam-side rule12px inset with small white ruler ticks spaced roughly40px. Cyan #0DB8F2 only accent, NO blur, NO glowing fog, no other colors. Leading corner has two sharp cyan diagonal parallel stripes6 and2 thickness with10 gap, short fine white ticks. Preserve clean enormous central empty area, no character, no illustration. UPPER curtain: cyan decorated TOP LEFT diagonal clipped corner only, cut depth about93px at output scale. Opposite right edge plain. Seam-side ruler goes along BOTTOM edge. Very subtle large V outline watermark at far RIGHT lower corner, low contrast within6% surface luminance. No other embellishments.

### Lower

Use case: precise-object-edit / UI texture. Reference is design style ONLY. Generate ONE standalone HIGH RESOLUTION scene transition curtain, NOT a contact sheet. Output canvas3072x1024, curtain artwork spans full width and vertical y149..875 (726 high), giving panel approximately2320:548 aspect. Outside above/below panel is plain flat WHITE staging margin for cropping. No headers, dimensions, arrows, sample labels, typography or annotations anywhere. Panel interior fully OPAQUE near-black #090A0D to #16181B, subtle barely visible diagonal paper hatching and grid, extremely crisp raster detail. Thin restrained graphite seam-side rule12px inset with small white ruler ticks spaced roughly40px. Cyan #0DB8F2 only accent, NO blur, NO glowing fog, no other colors. Leading corner has two sharp cyan diagonal parallel stripes6 and2 thickness with10 gap, short fine white ticks. Preserve clean enormous central empty area, no character, no illustration. LOWER curtain: cyan decorated BOTTOM RIGHT diagonal clipped corner only, cut depth about93px at output scale. Opposite left edge plain. Seam-side ruler goes along TOP edge. NO V watermark. Visually pair with same near-black upper panel. No other embellishments.

