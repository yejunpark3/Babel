# 육각형 타일 맵 시스템 사용법

## 빠른 시작

### 1. 기본 설정
1. 새 씬을 만들거나 기존 씬에서 시작
2. 빈 GameObject를 생성하고 "HexTileMap"으로 이름 변경
3. `HexTileMapSceneSetup` 컴포넌트를 추가
4. Inspector에서 "Setup Hex Tile Map System" 버튼 클릭

### 2. 수동 설정 (고급 사용자)
1. 빈 GameObject에 다음 컴포넌트들을 순서대로 추가:
   - `HexTileMapManager`
   - `HexTileMapInteraction`
   - `HexTileMapDemo`

2. HexTileMapManager 설정:
   - Hex Size: 1.0 (타일 크기)
   - Map Size: (10, 10) (맵 크기)
   - Generate On Start: true

3. 씬에 Canvas 추가 (UI를 원하는 경우):
   - Canvas 컴포넌트
   - Canvas Scaler 컴포넌트
   - Graphic Raycaster 컴포넌트

## 테스트하기

### 시스템 테스트
1. 씬에 빈 GameObject 생성
2. `HexTileMapSystemTest` 컴포넌트 추가
3. Play 모드에서 자동으로 테스트 실행
4. 또는 Inspector에서 "Run All Tests" 버튼 클릭

### 성능 테스트
- Inspector에서 "Performance Test" 버튼 클릭하여 대형 맵 성능 측정

## 사용 예제

### 프로그래밍 방식으로 타일 생성
```csharp
var tileMap = GetComponent<HexTileMapManager>();

// 단일 타일 생성
var coords = new HexCoordinates(0, 0);
tileMap.CreateTile(coords, HexTileType.Grass);

// 여러 타일 생성
for (int q = -5; q <= 5; q++) {
    for (int r = -5; r <= 5; r++) {
        var coord = new HexCoordinates(q, r);
        tileMap.CreateTile(coord, HexTileType.Grass);
    }
}
```

### 경로 찾기
```csharp
var start = new HexCoordinates(0, 0);
var end = new HexCoordinates(5, 5);
var path = HexPathfinding.FindPath(start, end, tileMap);

if (path != null) {
    Debug.Log($"경로 길이: {path.Count}");
    foreach (var coord in path) {
        Debug.Log($"경로: {coord}");
    }
}
```

### 범위 내 타일 찾기
```csharp
var center = new HexCoordinates(0, 0);
var range = 3;
var tilesInRange = HexPathfinding.GetTilesInRange(center, range);
```

## 조작법 (런타임)

### 마우스 조작
- **좌클릭**: 타일 선택 또는 경로 시작/끝점 설정
- **우클릭**: 페인트 모드에서 타일 타입 순환 변경

### 키보드 조작
- **P키**: 페인트 모드 토글
- **C키**: 선택 및 경로 초기화
- **R키**: 선택된 타일에서 이동 가능한 범위 표시
- **1-6키**: 페인트할 타일 타입 변경
  - 1: Grass (잔디)
  - 2: Water (물)
  - 3: Mountain (산)
  - 4: Forest (숲)
  - 5: Desert (사막)
  - 6: Stone (돌)

## 확장하기

### 새로운 타일 타입 추가
1. `HexTileType` enum에 새 타입 추가
2. `HexTile.UpdateTileProperties()` 메서드에 새 타입의 속성 정의

### 사용자 정의 상호작용 추가
1. `HexTileMapInteraction`을 상속받아 새 클래스 생성
2. 필요한 이벤트 핸들러 오버라이드

### 시각적 개선
1. `Assets/Materials/`에 새 재질 생성
2. `HexTileMapManager`의 `hexTileMaterial` 필드에 할당
3. `HexTileMapInteraction`의 하이라이트 재질들 설정

## 문제 해결

### 타일이 보이지 않음
- 카메라 위치 확인 (Y축 위에서 아래로 내려다보도록)
- 조명 설정 확인 (Global Light 2D 또는 Directional Light)
- 재질 설정 확인

### 마우스 상호작용이 작동하지 않음
- HexTileMapInteraction 컴포넌트가 추가되었는지 확인
- Camera 참조가 올바르게 설정되었는지 확인
- 타일에 Collider가 있는지 확인

### 성능 문제
- 큰 맵의 경우 타일 풀링 구현 고려
- LOD(Level of Detail) 시스템 도입
- GPU 인스턴싱 사용 검토