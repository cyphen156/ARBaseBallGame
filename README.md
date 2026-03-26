# AR BaseBallGame

모바일 AR 환경의 입력 편차를 정규화하고, 일관된 물리 결과를 생성하는 구조를 구현한 프로젝트입니다.


![curve](./docs/gifs/curve.gif)
![swing](./docs/gifs/swing.gif)

> ㅎㅎㅇㅇㅇㅌ · **Unity6 12th ARProject Team4**  
> Reference: **컴투스 프로야구**

---

## 프로젝트 개요

Unity AR Foundation 기반 모바일 AR 야구 시뮬레이션입니다.  
AR 카메라로 평면을 인식해 경기장을 배치하고, 실제 공간에서 투수/타자 모드로 플레이합니다.

- **팀 구성** : 황해원(팀장 / AR 인식), **오융택(본인 / 야구 게임 로직·시스템)**
- **개발 기간** : 2025.06.16 ~ 2025.06.27 (12일)
- **테스트 기기** : Galaxy S20 Ultra / Galaxy Jump 2
- **팀 레포지토리** : [BIT-Unity12th-XRProjects/ARBaseBallGame](https://github.com/BIT-Unity12th-XRProjects/ARBaseBallGame)
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

## 주요 시스템

### 입력 파이프라인

모바일 환경에서는 모든 입력이 화면 픽셀 좌표 기반으로 처리되기 때문에, 기기별 해상도 차이에 따라 동일한 드래그가 다른 물리값으로 변환됩니다.  
입력 수집(PlayerController)과 해석(GameManager)의 책임을 분리하고,  
Screen 비율 정규화 + 종횡비 보정으로 기기 독립적인 물리력을 산출합니다.
```
드래그 입력 → PlayerController → GameManager.ProcessInput() → Pitcher / Bat
버튼 입력  → Button Name → Enum 파싱 → UIManager → GameManager
```

| 비교 항목 | 기준 | 비교 |
|:---:|:---:|:---:|
| **해상도 차이 보정** | ![FHD](./docs/gifs/fhd.gif) | ![기본](./docs/gifs/fhd_portable.gif) |
| **드래그 길이 정규화** | ![짧은 드래그](./docs/gifs/fast_02_s.gif) | ![기본](./docs/gifs/fast_01.gif) |
| **시작 포지션 비교** | ![짧은 드래그](./docs/gifs/fast_02_s.gif) | ![다른 시작점](./docs/gifs/fast_03_s_diffpos.gif) |
| **입력 시간 비교** | ![기본](./docs/gifs/fast_01.gif) | ![긴 입력 시간](./docs/gifs/fast_04_long.gif) |

---

### 배트 반사 물리

공과 배트가 충돌하는 순간의 방향과 속도를 직접 활용해 반사 물리력을 계산하는 일반화 구조입니다.  
배트 콜라이더를 분할하고 위치별 가중치(0.3x~1.3x)와 스윙 가속도를 반영해, 타격 위치·타이밍·스윙 속도가 모두 타구 결과에 영향을 줍니다.

![배트 콜라이더](./docs/gifs/bat.png)

| 비교 항목 | 케이스 1 | 케이스 2 |
|:---:|:---:|:---:|
| **히트 위치에 따른 반사** | ![](./docs/gifs/hit_lm_lu_comp.gif) | ![](./docs/gifs/hit_cm_lu_comp.gif) |
| **측면 히트포인트** | ![](./docs/gifs/hit_lv_comp.gif) | ![](./docs/gifs/bat_lv_diff.gif) |

---

### 마그누스 효과

AR 월드 스케일에서는 타석까지 거리가 짧아 커브 궤적이 드러나지 않습니다.  
중력 축소, 역방향 보정, 투구 힘 기반 magnusStrength 스케일링으로 4방향 커브 궤적을 구현했습니다.

![커브 4방향 비교](./docs/gifs/curve_comb.gif)
```csharp
// Ball.cs — 투구 방향 기반 스핀 축 결정 + 중력 역보정
private void ApplySpin(PitchType type)
{
    switch (type)
    {
        case PitchType.Fastball:
            rb.angularVelocity = transform.right * -30f;
            rb.useGravity = false;
            break;
        case PitchType.Curve:
            Vector3 cross = Vector3.Cross(transform.forward, _direction);
            float side = Vector3.Dot(cross, Vector3.up);
            float directionSign = Mathf.Sign(side);
            float verticalFlip = Mathf.Sign(_direction.y);
            Vector3 spinAxis = transform.up;

            if (verticalFlip < 0)
            {
                spinAxis = Vector3.Reflect(spinAxis, transform.right);
                Physics.gravity = _flippedGravity;
            }

            float forceFactor = Mathf.InverseLerp(1f, 10f, _force);
            float finalSpin = Mathf.Lerp(1f, 2.5f, forceFactor);
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
    rb.linearVelocity *= (1f - _resistance * Time.fixedDeltaTime);
}
```

---

## 사후 개선점

입력 정규화는 화면 공간 기준으로는 성공했지만, AR 경기장 배치 거리가 force 계산에 반영되지 않아 체감 편차가 발생했습니다.  
magnusStrength가 고정값에 의존하면서, 투구 강도에 따른 물리적 차이를 충분히 반영하지 못했습니다.
- **방법 A — 월드 스케일 정규화** : AR Raycast 거리 기준 월드 스케일 보정 → force 및 magnusStrength 동시 정규화
- **방법 B — 동적 파라미터 산출** : 카메라-스트라이크존 거리를 force 보정값으로 반영 → magnusStrength 동적 산출

---

## 기술 스택

| 항목 | 내용 |
|---|---|
| 엔진 | Unity 6 (6.0.34f1) |
| 언어 | C# |
| AR | AR Foundation |
| 입력 | Unity Input System |
