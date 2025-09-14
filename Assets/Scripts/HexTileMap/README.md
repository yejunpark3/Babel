# 육각형 타일 맵 시스템 (Hexagonal Tile Map System)

Unity를 위한 완전한 육각형 타일 맵 시스템입니다.

## 기능

### 핵심 기능
- **HexCoordinates**: 축방향(axial) 좌표 시스템을 사용한 육각형 그리드 수학
- **HexTile**: 타일 속성과 타입을 관리하는 타일 클래스  
- **HexTileMapManager**: 전체 타일 맵을 관리하는 메인 컴포넌트
- **HexPathfinding**: A* 알고리즘을 사용한 육각형 그리드 경로 찾기
- **HexTileMapInteraction**: 마우스 상호작용과 타일 선택/페인팅
- **HexTileMapDemo**: 시스템을 보여주는 데모 컨트롤러

### 타일 타입
- Empty (빈 타일)
- Grass (잔디) - 이동 가능, 비용 1
- Water (물) - 이동 불가
- Mountain (산) - 이동 불가  
- Forest (숲) - 이동 가능, 비용 2
- Desert (사막) - 이동 가능, 비용 2
- Stone (돌) - 이동 가능, 비용 1

## 사용법

### 기본 설정
1. 빈 GameObject에 `HexTileMapManager` 컴포넌트 추가
2. 원하는 경우 `HexTileMapInteraction` 컴포넌트 추가하여 마우스 상호작용 활성화
3. UI가 있는 경우 `HexTileMapDemo` 컴포넌트 추가

### 조작법
- **좌클릭**: 타일 선택 또는 경로 찾기 시작/끝점 설정
- **우클릭**: 페인트 모드에서 타일 타입 순환 변경
- **P키**: 페인트 모드 토글
- **C키**: 선택 및 경로 초기화
- **R키**: 선택된 타일에서 이동 가능한 범위 표시
- **숫자키 1-6**: 페인트할 타일 타입 변경

### 코드 예제

```csharp
// 타일 맵 생성
var tileMap = GetComponent<HexTileMapManager>();
tileMap.GenerateMap();

// 특정 좌표에 타일 생성
var coords = new HexCoordinates(0, 0);
tileMap.CreateTile(coords, HexTileType.Grass);

// 경로 찾기
var path = HexPathfinding.FindPath(start, end, tileMap);

// 범위 내 타일 찾기
var tilesInRange = HexPathfinding.GetTilesInRange(center, 3);
```

## 시스템 구조

### HexCoordinates
육각형 그리드의 좌표를 나타내는 구조체입니다. 축방향(axial) 좌표계 (q, r)를 사용하며 효율적인 육각형 수학 연산을 제공합니다.

### HexTile
개별 육각형 타일을 나타내는 클래스입니다. 좌표, 타입, 이동 가능성, 이동 비용 등의 속성을 포함합니다.

### HexTileMapManager  
전체 타일 맵을 관리하는 메인 컴포넌트입니다. 타일 생성, 삭제, 시각화를 담당합니다.

### HexPathfinding
A* 알고리즘을 사용하여 육각형 그리드에서의 경로 찾기를 제공하는 정적 클래스입니다.

## 확장 가능성

이 시스템은 다음과 같은 용도로 확장할 수 있습니다:

- 전략 게임의 맵 시스템
- 턴 기반 전술 게임
- 시뮬레이션 게임의 지형 시스템
- 보드 게임 디지털화
- 절차적 맵 생성

## 성능 고려사항

- 대용량 맵을 위해 타일 풀링 구현 가능
- LOD(Level of Detail) 시스템으로 원거리 타일 간소화
- 청크 기반 로딩으로 메모리 사용량 최적화
- GPU 인스턴싱으로 렌더링 성능 향상 가능

## 라이센스

이 프로젝트는 MIT 라이센스 하에 제공됩니다.