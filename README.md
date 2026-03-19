# AR BaseBallGame

![curve](./docs/gifs/curve.gif)
![swing](./docs/gifs/swing.gif)

> 팀 레포지토리 : [BIT-Unity12th-XRProjects/ARBaseBallGame](https://github.com/BIT-Unity12th-XRProjects/ARBaseBallGame)

---

## 프로젝트 소개

### Unity의 **AR Foundation**으로 만든 **모바일 AR 야구 시뮬레이션**입니다.

스마트폰 AR 카메라로 **평면(Plane)** 을 인식해 경기장을 배치하고, 실제 공간에서 **투수/타자 모드**로 플레이합니다.  
드래그 제스처를 **힘·방향·타이밍**으로 정규화해 **투구/스윙**에 매핑하고,  
**마그누스 효과**와 **배트 반사 물리**로 현실감 있는 궤적과 타격감을 구현했습니다.

- **팀 구성** : 황해원(팀장 / AR 인식), **오융택(본인 / 야구 게임 로직·시스템)**
- **개발 기간** : 2025.06.16 ~ 2025.06.27 (12일)
- **기획 의도** : 일상 공간에서 바로 즐기는 캐주얼 야구 체험
- **테스트 기기** : Galaxy S20 Ultra / Galaxy Jump 2
- **포트폴리오** : [[Unity6] AR 야구 시뮬레이션](https://cyphen156.tistory.com/461)

---

## 수행 역할

팀 프로젝트로, 야구 게임 로직과 시스템 전반을 담당했습니다.

- 드래그 입력 → 화면 비율 기반 정규화 → 물리력 변환 파이프라인 설계 및 구현
- 마그누스 효과 기반 투구 물리 구현 (직구 / 커브)
- 배트 충돌 기반 타격 반사 시스템 설계 및 구현
- 버튼 이름 → enum 파싱 기반 UI 이벤트 중앙 라우팅 구조 설계
- 턴제 볼카운트 · 아웃카운트 처리 구현

---

## 구현한 기능

- **게임 루프** : `Init → Ready → Play → End`, 타자/투수 모드 선택, 라운드/타이머 운영
- **투수 모드** : 구종(직구/커브) 선택 → 드래그 기반 힘·방향·타이밍 산출 → `Shoot`
- **타자 모드** : 드래그 스윙 → 배트 충돌 반사 벡터 계산(타격 위치, 스윙 가속도) → 파울/안타/홈런 판정
- **물리·판정** : 마그누스 효과(커브), 반사
- **UI 반영** : 모드별 HUD, 스트라이크 존, 타이머/카운트/스코어 실시간 갱신

---

## 핵심 설계

### 입력 파이프라인
 
AR 환경에서는 탭·드래그·UI 입력이 동일한 터치를 공유하고, 모바일 기기마다 해상도와 종횡비가 달라 같은 드래그가 다른 물리력으로 이어집니다.  
이 특성을 전제로 입력 수집(PlayerController)과 해석(GameManager)의 책임을 분리하고,  
Screen 비율 기반 정규화 + 종횡비 보정으로 기기 독립적인 물리력을 산출하는 단일 파이프라인으로 설계했습니다.
 
```
드래그 입력 → PlayerController → GameManager.ProcessInput() → Pitcher / Bat
버튼 입력  → Button Name → Enum 파싱 → UIManager → GameManager
```

| 비교 항목 | 기준 | 비교 |
|:---:|:---:|:---:|
| **해상도 차이 보정** | ![FHD](./docs/gifs/fhd.gif) | ![기본](./docs/gifs/fhd_portable.gif) |
| **드래그 길이 정규화** | ![짧은 드래그](./docs/gifs/fast_02_s.gif) | ![기본](./docs/gifs/fast_01.gif) |
| **시작 포지션 비교** (비율에 의한 벡터 산출) | ![짧은 드래그](./docs/gifs/fast_02_s.gif) | ![다른 시작점](./docs/gifs/fast_03_s_diffpos.gif) |
| **입력 시간 비교** (가속도 변환) | ![기본](./docs/gifs/fast_01.gif) | ![긴 입력 시간](./docs/gifs/fast_04_long.gif) |

---

### 배트 반사 물리
Unity 충돌 시점에는 입사 벡터·충돌 법선·배트 속도 등 물리 정보가 그대로 존재합니다.  
이 특성을 활용해 타격 결과를 사전 매핑하지 않고 충돌 시점의 물리값으로 직접 계산하는 일반화 구조를 선택했습니다.  
**배트 콜라이더를 분할**하고 **위치별 가중치(0.3x~1.3x)와 유저의 입력에 의해 전달된 물리력**를 반영해  
타격 위치·타이밍·스윙 속도가 모두 타구 결과에 영향을 줍니다.

![배트 콜라이더](./docs/gifs/bat.png) 

| 비교 항목 | 케이스 1 | 케이스 2 |
|:---:|:---:|:---:|
| **히트 위치에 따른 반사 비교** | ![](./docs/gifs/hit_lm_lu_comp.gif) | ![](./docs/gifs/hit_cm_lu_comp.gif) |
| **측면 히트포인트 비교** | ![](./docs/gifs/hit_lv_comp.gif) | ![](./docs/gifs/bat_lv_diff.gif) |

---

### 마그누스 효과 적용
AR 월드 스케일에서는 타석까지의 거리가 짧아 **마그누스 힘이 충분히 누적되지 않고**,  
**직진 속도 성분이 상대적으로 크게 작용**해 **커브 궤적이 거의 드러나지 않습니다.**
이를 보완하기 위해 중력을 조정하고 아래 방향 커브는 역방향으로 보정했으며,  
magnusStrength를 스케일링해 **회전 효과가 명확히 드러나도록 설계**했습니다. 

![커브 4방향 비교](./docs/gifs/curve_comb.gif)

```csharp
// Ball.cs — 투구 방향 기반 스핀 축 결정 + 중력 역보정
private void ApplySpin(PitchType type)
{
    switch (type)
    {
        case PitchType.Fastball:
            rb.angularVelocity = transform.right * -30f; // Backspin
            rb.useGravity = false;
            break;
        case PitchType.Curve:
            Vector3 cross = Vector3.Cross(transform.forward, _direction);
            float side = Vector3.Dot(cross, Vector3.up);        // 좌/우 판별
            float directionSign = Mathf.Sign(side);             // +1 Curve Left / -1 Curve Right
            float verticalFlip = Mathf.Sign(_direction.y);
            Vector3 spinAxis = transform.up;
 
            if (verticalFlip < 0)                               // 아래로 던질 때 스핀 축 플립 + 중력 역보정
            {
                spinAxis = Vector3.Reflect(spinAxis, transform.right);
                Physics.gravity = _flippedGravity;
            }
 
            float forceFactor = Mathf.InverseLerp(1f, 10f, _force);
            float finalSpin = Mathf.Lerp(1f, 2.5f, forceFactor); // 투구 힘에 비례한 스핀 세기
 
            rb.angularVelocity = spinAxis * directionSign * -finalSpin * 0.8f;
            break;
    }
}
 
// Ball.cs — 마그누스 힘 + 공기 저항 수동 적용
private void FixedUpdate()
{
    if (_pitchType == PitchType.Curve)
    {
        Vector3 w = rb.angularVelocity;
        Vector3 v = rb.linearVelocity;
 
        if (w != Vector3.zero && v != Vector3.zero)
        {
            Vector3 magnusForce = Vector3.Cross(w, v).normalized * magnusStrength;
            rb.AddForce(magnusForce, ForceMode.Acceleration);
        }
    }
 
    rb.linearVelocity *= (1f - _resistance * Time.fixedDeltaTime); // 공기 저항 수동 적용
}
```

---

## 사후 개선점

입력 정규화는 화면 공간 기준으로는 성공했지만, 월드 공간 기준 거리 보정이 빠져있었습니다.  
AR 경기장은 사용자가 배치하는 시점에 카메라와 스트라이크존 사이의 월드 거리가 결정됩니다.  
그러나 `ProcessInput`의 force 계산은 드래그 시간 기반으로만 처리되어 이 거리를 반영하지 않습니다.  
결과적으로 경기장을 가까이 배치하면 공이 너무 빠르게, 멀리 배치하면 너무 약하게 느껴지는 체감 편차가 발생했습니다.  

**현재 고려하고 있는 개선 방은 두 가지입니다.**  
- **방법 A — 월드 스케일 정규화** : 경기장 생성 시점의 AR Raycast 거리를 기준으로 월드 스케일 자체를 보정해 동일한 물리력이 동일한 체감으로 작용하도록 한다.
- **방법 B — force 스케일 보정** : 카메라-스트라이크존 거리를 측정해 `ProcessInput`의 force 값에 보정값으로 직접 반영한다.

---

## 기술 스택

| 항목 | 내용 |
|---|---|
| 엔진 | Unity 6 (6.0.34f1) |
| 언어 | C# |
| AR | AR Foundation |
| 입력 | Unity Input System |
