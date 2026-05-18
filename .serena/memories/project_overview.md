# Project-Vertex 개요

## 목적
Unity 6 기반 덱빌딩 로그라이트 + JRPG 파티 시스템 게임. "VERTEX"라는 제목.
- Slay the Spire 스타일 덱빌딩 + 탄약 시스템(에너지와 별개의 제2자원) + JRPG 합류 캐릭터 시스템

## 기술 스택
- Unity 6000.3.13f1, C#, URP
- TextMeshPro (UI 텍스트)
- ScriptableObject 기반 데이터 설계
- 플랫폼: PC (Windows)

## 폴더 구조 (Assets/Scripts)
- Card/ : CardData(SO), CardEffect(추상 SO), CardContext, CardView, HandView, RunData
- Card/Effects/ : DamageEffect, BlockEffect, DrawEffect, ApplyStatusEffect, AddCardToPileEffect
- Enemy/ : EnemyData(SO), EnemyAction(SO), EnemyInstance(런타임), EnemyView, EnemyZoneView
- Gameplay/ : BattleManager(싱글턴), GameManager(싱글턴, DontDestroyOnLoad)
- Develop/ : DebugBattleStarter

## 핵심 아키텍처 패턴
- Singleton: GameManager, BattleManager, RunData
- Observer(이벤트): BattleManager.OnHandChanged → HandView, BattleManager.OnEnemiesChanged → EnemyZoneView
- Command(SO): CardEffect.Execute(CardContext) — 카드 효과를 SO로 조합
- Data/View 분리: EnemyInstance(데이터) ↔ EnemyView(뷰), CardData ↔ CardView
