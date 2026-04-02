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

터치 입력을 수집하고, 기기 해상도에 따라 정규화한 뒤,
게임 모드에 따라 적절한 처리 대상에게 전달하는 입력 파이프라인입니다.
 
- 입력 수집과 해석의 책임 분리
- 시작 지점 기준 비율 환산을 위한 정규화 적용
- 화면 종횡비 차이에 따른 방향 왜곡 보정
- 게임 모드에 따른 처리 대상 라우팅

![입력 파이프라인](./docs/gifs/입력%20파이프라인.png)

| 비교 항목 | FHD | PortableFHD |
|:---:|:---:|:---:|
| **해상도 차이 보정** | ![FHD](./docs/gifs/fhd.gif) | ![기본](./docs/gifs/fhd_portable.gif) |
| **드래그 길이 정규화** | ![짧은 드래그](./docs/gifs/fast_02_s.gif) | ![기본](./docs/gifs/fast_01.gif) |
| **시작 포지션 비교** | ![짧은 드래그](./docs/gifs/fast_02_s.gif) | ![다른 시작점](./docs/gifs/fast_03_s_diffpos.gif) |
| **입력 시간 비교** | ![기본](./docs/gifs/fast_01.gif) | ![긴 입력 시간](./docs/gifs/fast_04_long.gif) |

---

### 배트 반사 물리

충돌 순간의 방향·속도와 배트 타격 위치·스윙 가속도를 종합해 반사 물리력을 계산하는 시스템입니다.
 
- 배트 콜라이더 분할 구조
- 타격 위치별 반사력 가중치(0.3x~1.3x)
- 반사 벡터 스윙 가속도 반영

![배트 콜라이더](./docs/gifs/bat.png)

| 비교 항목 | 케이스 1 | 케이스 2 |
|:---:|:---:|:---:|
| **히트 위치에 따른 반사** | ![](./docs/gifs/hit_lm_lu_comp.gif) | ![](./docs/gifs/hit_cm_lu_comp.gif) |
| **측면 히트포인트** | ![](./docs/gifs/hit_lv_comp.gif) | ![](./docs/gifs/bat_lv_diff.gif) |

---

### 마그누스 효과
 
투구 방향 기반 스핀 축과 중력 보정을 적용해 AR 환경에서 4방향 커브 궤적을 구현한 물리 시스템입니다.
 
- 중력 가속도 축소 적용(30%)
- 하향 커브 역방향 보정을 통한 낙차 반전(-20%)
- 마그누스 힘 스케일링을 통한 커브 시각화
 
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

AR Plane 배치 시 Raycast 지점에 따라 카메라와 경기장 간 거리가 달라지면서, 월드 스케일 자체가 매번 다르게 형성됩니다.
이로 인해 동일한 물리력이라도 투구 체감이 경기장 배치 상황에 따라 다르게 나타납니다.
 
| 방법 | 기준 | 특징 | 리스크 |
|---|---|---|---|
| **월드 스케일 정규화** | 물리 일관성 | AR Raycast 거리 기준 월드 스케일 동적 조정  | 메시·콜라이더 스케일 불안정 가능성 |
| **동적 물리력 보정** | 공간 안정성 | 카메라-스트라이크존 거리 비례 물리력·마그누스 힘 실시간 스케일링 | 물리력 일관성 관리 어려움, 연산 비용 증가 |
 
---

## 기술 스택

| 항목 | 내용 |
|---|---|
| 엔진 | Unity 6 (6.0.34f1) |
| 언어 | C# |
| AR | AR Foundation |
| 입력 | Unity Input System |
