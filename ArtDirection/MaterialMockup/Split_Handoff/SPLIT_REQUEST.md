# 마테리얼 화면 분절 요청

정본은 둘 다 1672×941이다.
- `../Material_Enemy_Mockup.png` (적 탭, 2편 읽는 중)
- `../Material_Companion_Mockup.png` (동료 탭, 잠긴 3편)

보기용은 `Preview_Enemy.jpg`, `Preview_Companion.jpg`(960×540)다. 결과는 `../Extracted/`에 낸다.

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 위 미리보기 2장까지만이다. 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 결과 확인은 1200폭 이하 확인용 시트 1장(`../Extracted/Contact_Sheet.jpg`)으로 한다.

## 재사용 (다시 뽑지 않음, README에 "재사용"으로만 적기)

모양이 같으면 재사용하고, **다르면 새로 뽑고 README에 이유를 적는다.**

| 쓰임 | 기존 조각 |
|---|---|
| ← 로비 | `LoadoutMockup/Extracted/Back_Normal(_Hover)` + `Icon_BackArrow` |
| 필터 체크박스 | `ArmoryMockup/CardCatalogExtracted/Checkbox_*` |
| 등급 배지 (일반·엘리트·보스) | `TrainingMockup/Extracted/TypeBadge_*` |
| 격자 칸 아래 기록 점 (마름모 5개) | `TrainingMockup/Extracted/DefeatPip_Filled/Empty` |
| 목록 스크롤 막대 | `TrainingMockup/Extracted/Scrollbar_*` |
| 잠긴 칸 자물쇠 (작은 것) | `TrainingMockup/Extracted/Icon_Lock` |

## 공통 분절 규칙

- **글자·숫자는 전부 뺀다.**
  - 제목, 05, 영문 라벨, 탭 글자(적·ENEMY·동료·COMPANION), 필터 글자
  - 섹션 제목, "확인 2 · 전체 14", 이름, "???", 등급 배지 글자, 소속 배지 글자
  - "격퇴 2회 · 기록 2 / 5", 편 번호 01~05, "ARCHIVE-02", 기록 제목, 본문
  - 잠김 문구, 아래쪽 안내문, "관측 자료 / 전투 기록 기반"
- **그림은 칸만 남긴다.** 적·동료 초상, 큰 대상 그림, 실루엣은 넣지 않는다. 게임에서 실제 그림을 얹고, 실루엣도 게임에서 칠한다.
- 판과 칸의 알파는 0 또는 255, 종이 질감은 유지한다. 지운 자리는 질감으로 복원한다.
- 상태별 조각은 같은 크기·같은 위치로 맞춘다.
- 기호(자물쇠, 클립)는 따로 투명 PNG로 뽑는다.

## 조각

| 조각 | 설명 | 상태 |
|---|---|---|
| `Base_Material` | 1672×941 무문자 바탕. 바깥 로비 배경, 판, 머리 장식, 분류·격자·기록 구역 판과 구분선만 남긴다. 탭·칸·대상 머리 판·편 탭·서류 판·글자·그림은 뺀다 | — |
| `CategoryTab` | 왼쪽 위 큰 분류 탭 (적 / 동료) | Normal(종이) / Hover / Selected(먹색 + 왼쪽 청록 띠) |
| `SubjectCell` | 격자 칸. 초상 창은 투명, 아래 이름·점 띠 칸은 포함 | Normal / Hover / Selected(청록 테두리) / Unknown(회색) |
| `SubjectHeader` | 기록 구역 위쪽 대상 머리 판(큰 그림 칸 + 정보 자리). 그림 창은 투명. 바탕에 포함돼 있으면 생략하고 창 좌표만 적는다 | — |
| `Badge_Affiliation` | 동료 소속 청록 배지 칸 (글자 없음, 가로로 늘어나게 9-slice) | — |
| `ProgressCell` | 이름 아래 5칸 진행 막대의 한 칸 | Filled(청록) / Empty |
| `EntryTab` | 01~05 색인 탭 (글자 없음) | Normal / Hover / Selected(청록 아래 선) / Locked(회색) / LockedSelected(회색 + 청록 아래 선) |
| `Icon_LockTab` | 잠긴 탭 글자 앞 작은 자물쇠 | — |
| `Document_Paper` | 서류 한 장 판 (모서리 접힘·그림자 포함, 클립 제외) | — |
| `Document_Clip` | 서류 오른쪽 위 클립 (투명 배경) | — |
| `Document_Rule` | 제목 아래 괘선 (가로로 늘어나게) | — |
| `Icon_LockLarge` | 잠긴 편 가운데 큰 자물쇠 | — |

## 결과물

`../Extracted/` 폴더에 아래를 넣는다.
- 조각 PNG
- `README.md` (재사용 목록과 새로 뽑은 이유)
- `Contact_Sheet.jpg`
- `layout.json`, 조각마다 아래를 적는다.
  - 원본 크롭 좌표
  - 1672×941 배치 좌표 (좌상단)
  - 9-slice (Left, Bottom, Right, Top)
  - 반복 간격: 격자 열·행 간격과 열 수, 편 탭 간격, 진행 칸 간격, 기록 점 간격
  - 창 좌표: 격자 칸 초상 창, 대상 머리 그림 창
  - 서류 판 안쪽 영역: 문서 번호 / 제목 / 괘선 / 본문 스크롤 영역 / 스크롤 막대 자리 / 아래쪽 출처 글자 자리
  - 글자 자리 좌표
    - 분류 탭 글자, 필터, 섹션 제목·개수
    - 칸의 이름, 대상 머리의 이름·배지·진행 글자
    - 편 탭 번호, 잠김 문구, 아래쪽 안내문
