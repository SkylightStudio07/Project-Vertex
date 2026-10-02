# 대화 UI 분절

정본: Dialogue_Story_Mockup.png / Dialogue_Overlay_Mockup.png. 글자·번호·캐릭터·초상·배경·상점 화면은 포함하지 않았습니다.

## 제작 방식

판은 정본 하단의 깨끗한 여백에서 추출한 종이 질감을 반전 반복하고 #F1F2F4로 보정해 복원했습니다. 단순 사각 크롭이 아니며 원본 배경이 보이던 부분도 종이 RGB로 채웠습니다. 판 윤곽·끝 장식·칸은 요청 규격으로 재구성했습니다. 워터마크는 원본 나침반 밝기 마스크로 추출했습니다.

## 알파

Dialogue_Box는1920×300 RGBA입니다. y0 알파0에서 y119 알파255까지 선형 변화하고, y120~299는 전부255입니다. 장식에도 같은 알파를 적용했습니다. RGB를 배경과 섞어 굽지 않은 straight alpha PNG이며 런타임에서 실제 장면과 합성합니다. 8비트 PNG의 정상적인256단계 정밀도입니다. 하단은 불투명 종이입니다.

나침반은 선 가장자리를 살리는 가변 알파입니다. 전체 알파0.12부터 조절하십시오. 나머지 판·칸·기호 알파는0/255입니다. Portrait_Cell의 가운데는 초상을 얹기 위한 투명 영역입니다.

## 조립

- Dialogue_Box: 왼쪽 마름모·양 끝 모서리 장식 포함, 나침반 제외.
- Dialogue_Watermark:160×160 독립 나침반. 원본의 미세한 질감이 일부 남을 수 있습니다.
- Name_AccentLine:120×3 **흰색**, 캐릭터 색으로 tint합니다.
- Advance_Diamond14×14 + Advance_Line48×2: 청록 넘기기 표시. 깜빡임은 게임에서 처리합니다.
- Portrait_Cell96×96: 원본의 초상 크기와 달리 요청 규격으로 정규화했습니다. 초상 그림은 별도로 올립니다.
- Choice_Panel760×72: Normal/Hover/Disabled. Answer_Panel640×60: Normal/Hover. 모든 글자·숫자·화살표를 제외했습니다.
- Icon_Arrow20×20, NumberUnderline18×2: 선택지·답변 공용. Disabled에서는 게임에서 회색으로 tint합니다.

## 좌표·9-slice

layout.json의 source_crop_xywh는1920×1080 원본에서 참고한 영역이고 output_size는 출력 크기입니다. 원본 목업과 요청 규격이 다르므로 placements에 구현용 요청 좌표를 별도로 기록했습니다. 모든 좌표는 좌상단 기준입니다.

nine_slice_lbrt는 Left, Bottom, Right, Top 순서입니다. Dialogue_Box는128,0,160,0이며 **높이300 고정, 가로만** 늘립니다. 위120px 그라데이션을 세로로 늘리지 마십시오. Choice/Answer는22px, 초상 칸은19px 경계입니다. 나침반·밑줄·기호는 Simple로 사용합니다.

## 재생성·검증

python ArtDirection/DialogueMockup/Split_Handoff/split_dialogue.py

PNG 크기, 크롭 범위, 그라데이션 행별 알파·단조성, 하단 불투명, 다른 판의0/255 알파를 자동 검증합니다. Contact_Sheet.jpg는1200px 폭 한 장입니다. 원본 및 Unity importer/.meta는 수정하지 않았습니다.
