# ARBaseBallGame

> ㅎㅎㅇㅇㅇㅌ · **Unity6 12th ARProject Team4**  
> Reference: **컴투스 프로야구**

## 프로젝트 소개
### Unity의 **AR Foundation**으로 만든 **모바일 AR 야구 시뮬레이션**입니다.  
스마트폰 AR 카메라로 **평면(Plane)** 을 인식해 경기장을 배치하고, 실제 공간에서 **투수/타자 모드**로 플레이합니다.  
드래그 제스처를 **힘·방향·타이밍**으로 정규화해 **투구/스윙**에 매핑하고, **마그누스 효과**와 **반사/속도 클램프**로 현실감 있는 궤적과 타격감을 구현했습니다.
- **팀 구성** : 황해원(팀장/AR 인식), 오융택(본인/야구 게임 로직·시스템)
- **개발 기간** : 2025.06.16 ~ 2025.06.27 (12일)
- **기획 의도** : 일상 공간에서 바로 즐기는 캐주얼 야구 체험
- **개발 목표**
  - **AR Plane 인식** 후 **경기장 배치**
  - 드래그 제스처를 **투구/스윙**으로 자연스럽게 매핑
  - **마그누스 효과 · 반사** 등 물리 기반 재미 구현

## 핵심 기능
- **AR 경기장 배치**: AR Plane 인식 → **터치 지점 1회 배치**
- **게임 루프**: `Init → Ready → Play → End`, **타자/투수 모드 선택**, 라운드/타이머 운영
- **투수 모드**: 구종(**직구/커브**) 선택 → 드래그 기반 **힘·방향·타이밍** 산출 → `Shoot`
- **타자 모드**: 드래그 스윙 → 배트 충돌 **반사 벡터** 계산(**타격 위치, 스윙 가속도**) → 파울/안타/홈런 판정
- **물리·판정**: **마그누스 효과(커브)**, **반사**
- **UI 반영**: **모드별 HUD**, **스트라이크 존**, **타이머/카운트/스코어** 실시간 갱신


## 기술 스택
- **엔진**: Unity 6 (6.0.34f1)  
- **언어**: C#  
- **AR**: AR Foundation  
- **버전관리**: GitHub Desktop  
- **테스트 기기**: Galaxy S20 Ultra / Galaxy Jump 2

## 실행 방법 (Android)
1. APK 설치 후 앱 실행 → **카메라/센서 권한 허용**  
2. **평면 인식** 후 경기장 배치  
3. **투수/타자 모드** 플레이

### 시연 자료
- **APK**: [ARBaseballGame_v1.0.0.apk](https://github.com/cyphen156/ARBaseBallGame/releases/download/v1.0.0/ARBaseballGame_v1.0.0.apk)
- **Presentation**:
  - [hhwoyt_ARBaseBall.pdf](https://drive.google.com/file/d/17WXuQKkt16ddmN7GcnSnNGUZf2qoJlZP/view)
  -  [hhwoyt_ARBaseBall.pptx](https://docs.google.com/presentation/d/1VJ9vba_Eq3rNBf-zrp2sZzxHUjuZts4d/edit?slide=id.p1#slide=id.p1)

### Contact Us  
황해원: [hhw3287@gmail.com](mailto:hhw3287@gmail.com) / 010-9149-3287/ [GitHub](https://github.com/now2ah)  
오융택: [yungtaekoh@gmail.com](mailto:yungtaekoh@gmail.com) / 010-4810-7201 / [GitHub](https://github.com/cyphen156)  

## Rules
### Asset Management
 - `Resources/` 기준 런타임 로드 자산 관리
   
### Coding Standard
- **Unity Basic C# 스타일 가이드** 준수

---
