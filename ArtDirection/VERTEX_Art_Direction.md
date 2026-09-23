# Project VERTEX Art Direction

## 1. Core Visual Direction

**VERTEX의 UI 아트 스타일은 `플랫 실루엣 + 기하학적 포인트 컬러`를 기본으로 한다.**

핵심 인상은 다음과 같다.

- 차갑고 무정한 백색 시스템 UI
- 관측 / 기록 / 좌표계 / 측정 장비를 연상시키는 인터페이스
- 얇은 선, 십자표식, 원호, 눈금, 좌표 숫자, 별 / 컴퍼스 모티프 반복
- 디테일한 일러스트보다 면 분할과 형태감으로 완성도를 확보
- 정보 전달과 실제 게임 HUD 가독성을 우선

레퍼런스 우선순위:

1. **HAMETIC 시스템 메뉴**
   - 플랫 실루엣
   - 별 / 컴퍼스 모티프
   - 큰 면 분할
   - 블랙 / 화이트 대비
   - 제한된 포인트 컬러

2. **VERTEX Event Viewer / The White Vertex 실장 이미지**
   - 오프화이트 배경
   - 얇은 기하학 선
   - 좌표 / 로그 / 관측 시스템 느낌
   - 모서리 절단형 패널
   - 세계관 안의 시스템 화면처럼 보이는 UI

3. **현재 실장된 VERTEX 전투 화면**
   - 실제 적용 기준
   - 정보 우선순위와 HUD 가독성 확인용

4. **히어로 목록형 UI**
   - 카드형 정보 배치
   - 둥근 패널
   - 아이콘화된 스탯
   - 업그레이드 카드 / 플레이어 스탯 HUD에 부분 적용

5. **맥시미 계열 메뉴 타이포그래피**
   - 굵고 큰 산세리프 대문자
   - 타이틀 / 로고 / 메인 메뉴 등 제한된 화면에서만 사용


## 2. Color System

### Base
- Off-white / White
- Black
- Neutral gray

### Primary Accent
- Cyan / Light Blue

기본 UI는 모노톤으로 유지하고, 인터랙션과 선택 상태에만 블루 / 시안을 제한적으로 사용한다.

### Functional Colors
카드 / 기능 계열은 색상으로 빠르게 구분한다.

- **Attack**: 공격 계열 포인트 컬러
- **Skill**: 스킬 계열 포인트 컬러
- **Power**: 파워 계열 포인트 컬러

색상은 전체 UI를 덮지 않고 다음 요소에만 제한한다.

- 아이콘
- 얇은 강조선
- 선택 상태
- 작은 라벨
- 카드 타입 인디케이터

### Warning
- Red는 HP, 위험, 경고 등 의미가 명확한 곳에만 소량 사용한다.


## 3. Shape Language

VERTEX UI는 평범한 직사각형 박스를 반복하지 않는다.

주요 형태:

- 모서리가 잘린 긴 사각 패널
- 비대칭 면 분할
- 얇은 이중 외곽선
- 긴 가로형 HUD 바
- 작은 태그형 라벨
- 좌표축 형태의 구분선
- 원호 / 레이더 형태
- 십자 / 작은 점 / 별형 마커

장식 요소:

- `+` 형태 좌표 마커
- 작은 다이아몬드
- 4방향 별
- 나침반 / 컴퍼스
- 얇은 원형 스케일
- 불완전한 원호
- 숫자 좌표
- 짧은 점선
- 아주 작은 시스템 라벨

장식은 정보보다 눈에 띄어서는 안 된다.


## 4. Vertex Mark

`UI_VertexMark_CompassVoid_Concept01_Trim.png`는 **Project VERTEX의 핵심 마크**다.

단순 로고가 아니라 UI 전체에 반복되는 기본 기하 모티프로 취급한다.

사용 예:

- 메뉴 헤더
- 버튼 끝장식
- 섹션 구분자
- 로딩 표시
- 현재 위치 마커
- 패널 코너 장식
- 상태 HUD 심볼
- 이벤트 선택 강조
- 맵 장식

주의:

- 지나치게 크게 반복하지 않는다.
- 핵심 기능 아이콘과 혼동되지 않도록 장식용과 기능용을 구분한다.
- 변형 시 기본 4방향 별 / 컴퍼스 실루엣은 유지한다.


## 5. Typography

### Main Title / Logo
- 매우 굵은 산세리프
- 대문자
- 큰 스케일
- 강한 대비
- HAMETIC / 맥시미 메뉴 계열의 인상 참고

적용 범위:

- 타이틀 화면
- 메인 메뉴
- 대형 챕터 타이틀
- 결과 화면의 큰 제목

### Gameplay UI
본문과 HUD는 대형 타이포 스타일을 남발하지 않는다.

- 간결한 산세리프
- 작은 시스템 라벨
- 숫자 가독성 우선
- 자간을 약간 넓혀 기계적 / 관측 시스템 인상 강화 가능


## 6. Battle HUD Direction

전투 HUD의 최우선 목표는 **가독성**이다.

현재 VERTEX는 밝은 백청색 배경을 자주 사용하므로, UI가 배경에 묻히지 않도록 명확한 명도 대비가 필요하다.

### Required HUD Elements
- 플레이어 HP
- 적 HP
- 에너지
- 탄약
- 현재 무기
- 버프 / 디버프
- 적 Intent
- 아이템
- 카드 손패
- End Turn
- Map / Deck 접근 UI

### Battle HUD Rules
- 숫자는 반드시 한눈에 읽혀야 한다.
- HP / 에너지 / 탄약은 장식보다 상태 변화가 먼저 보여야 한다.
- 배경 위에서는 완전한 흰색 패널보다 검은 외곽선이나 반투명 어두운 영역을 병행한다.
- 주요 수치 뒤에는 충분한 대비를 확보한다.
- 장식선이 카드나 캐릭터 실루엣을 침범하지 않도록 한다.
- 전투 화면 중앙은 최대한 비워 캐릭터와 적의 가독성을 확보한다.

목표는 **예쁜 화이트 UI가 아니라, 실제 전투에서 빠르게 읽히는 화이트 UI**다.


## 7. Panel Rules

공용 패널은 다음 구성으로 통일한다.

### Large Panel
- Off-white fill
- Black thin border
- 일부 모서리 컷
- 한쪽에 Vertex Mark 또는 작은 좌표 요소
- 강조 상태에서 Cyan line 추가

### Small Label
- 짧은 사각형
- 텍스트 + 작은 별 / 다이아몬드
- 기능명을 표시

### Status Panel
- 아이콘 + 큰 숫자
- 얇은 분할선
- 작은 영어 시스템 라벨
- 필요 시 시안 강조선

### Button
- 긴 가로형
- 모서리 절단
- 왼쪽 기능 아이콘
- 중앙 텍스트
- 오른쪽 Vertex 계열 별 장식
- Hover 시 시안 강조


## 8. Screen-Specific Application

### Event Viewer
현재 실장된 Event Viewer 방향을 핵심 기준으로 삼는다.

- 좌측: 큰 상징 / 사건 이미지 / 기하 패턴
- 우측: 사건 제목과 설명
- 하단 또는 우측: 긴 선택지 버튼
- 좌표 / 로그 / 관측 정보 장식

### Map
- 배경 자체를 가리지 않는 얇은 UI
- 노드와 경로가 최우선
- 현재 위치는 Vertex Mark 계열 마커 사용 가능
- 작은 좌표 / 방위 / 챕터 정보 패널 추가 가능

### Shrine / Companion Selection
- 캐릭터가 중심
- UI는 얇고 평면적으로 배치
- 카드형 상세 정보 구조 참고
- 캐릭터 패시브 / 고유 카드 / 역할을 아이콘 중심으로 정리

### Title / Main Menu
- 굵고 큰 대문자 타이포 사용 가능
- 가장 과감한 실루엣 / 면 분할 허용
- Vertex Mark를 대형 비주얼 요소로 사용할 수 있음


## 9. Production Priority

### Priority 1 — Common UI Kit
- Large panel
- Small panel
- Header bar
- Horizontal button
- Small tag
- Divider
- Vertex decorative elements
- Selection / Hover indicator

### Priority 2 — Battle HUD
- Player HP frame
- Enemy HP frame
- Energy panel
- Ammo panel
- Weapon panel
- Buff / Debuff slot
- Intent frame
- End Turn button

### Priority 3 — Event UI
- Event header
- Choice button
- Log / coordinate ornament
- Image frame

### Priority 4 — Map UI
- Node frame
- Current position marker
- Chapter / floor label
- Map utility buttons

### Priority 5 — Shrine / Companion UI
- Character selection card
- Passive panel
- Unique card preview frame
- Confirm button

### Priority 6 — Title / Menu / Loading
- Main menu layout
- Logo composition
- Loading spinner / Vertex Mark animation


## 10. Do Not Use

다음 방향은 VERTEX의 기본 UI 아트 디렉션에서 제외한다.

- 실사 항공샷 중심 배경 합성
- 망가 잉크 일러스트 스타일
- 과도한 금속 / 유리 / 홀로그램 질감
- 두꺼운 네온 글로우
- 사이버펑크식 RGB 다색 UI
- 지나친 그라디언트
- 과한 그림자 / 베벨 / 3D 버튼
- 판타지 장식 프레임
- 의미 없는 복잡한 HUD 장식
- 정보보다 장식이 우선되는 구성


## 11. One-Line Direction

> **차가운 백색 관측 시스템 UI + 플랫 실루엣 + 별 / 컴퍼스 기반 기하학 + 제한된 블루 포인트 + 실제 플레이에 필요한 높은 가독성**

이 문장을 VERTEX UI 제작 시 최상위 기준으로 사용한다.
