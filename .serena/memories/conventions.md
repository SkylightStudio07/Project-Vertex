# 코드 컨벤션 & 스타일

## 파일 헤더 (일부 파일)
```
// filename   : xxx.cs
// 작성자    : 이름
// 작성일    : YYYY-MM-DD
// description   : 설명
```

## 네이밍
- 클래스/SO: PascalCase
- private 필드: camelCase (일부 _camelCase)
- public 프로퍼티: PascalCase (=> 프로퍼티 표현식)
- 이벤트: On+동사(PascalCase), e.g. OnHandChanged, OnDamaged, OnDied

## SO 등록
[CreateAssetMenu(fileName = "...", menuName = "Game Asset/...")] 패턴 사용

## 주석
한국어 주석 혼용. 중요 제약사항은 주석으로 명시.

## 특이사항
- EnemyInstance는 MonoBehaviour 미상속 (퍼포먼스 이유, 수정 금지 명시)
- CardData는 Instantiate로 복사해서 SO 원본 오염 방지
- Effects는 SO로 조합, Execute(CardContext) 패턴
