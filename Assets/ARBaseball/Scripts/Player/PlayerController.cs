using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation.Samples;

/// <summary>
/// 사용자의 입력을 처리하고 플레이어의 행동을 제어하는 클래스입니다.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Input actions")]
    public InputActionReferences _inputActionReferences;

    private bool _isDragging;
    private double _beginDragTimeMark;
    private Vector2 _touchStartPosition;
    private Vector2 _touchEndPosition;

    [SerializeField] private RectTransform _line;
    [SerializeField] private Canvas _canvas;

#if UNITY_EDITOR
    private void Update()
    {
        if (_isDragging == false)
        {
            return;
        }

        float time = (float)(Time.timeAsDouble - _beginDragTimeMark);
        UpdateDragLine(_touchStartPosition, _touchEndPosition, time);
    }
    //private void Update()
    //{
    //    if (Mouse.current == null)
    //        return;

    //    // 마우스 버튼 눌림
    //    if (Mouse.current.leftButton.wasPressedThisFrame)
    //    {
    //        _isDragging = true;
    //        _touchStartPosition = Mouse.current.position.ReadValue();
    //        _beginDragTimeMark = Time.timeAsDouble;
    //    }

    //    if (_isDragging)
    //    {
    //        _touchEndPosition = Mouse.current.position.ReadValue();

    //        float time = (float)(Time.timeAsDouble - _beginDragTimeMark);
    //        UpdateDragLine(_touchStartPosition, _touchEndPosition, time);
    //    }

    //    // 마우스 버튼 뗌
    //    if (_isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
    //    {
    //        _isDragging = false;
    //        _touchEndPosition = Mouse.current.position.ReadValue();

    //        double elapsedDraggingTime = Time.timeAsDouble - _beginDragTimeMark;

    //        GameManager.Instance.ProcessInput(_touchStartPosition, _touchEndPosition, elapsedDraggingTime);
    //        HideDragLine();
    //    }
    //}
#endif

    private void OnEnable()
    {
        _inputActionReferences.screenTapPosition.action.performed += OnTouchPositionPerformed;
        _inputActionReferences.screenTapPosition.action.Enable();
        _inputActionReferences.screenTap.action.started += OnTouchPressPerformed;
        _inputActionReferences.screenTap.action.canceled += OnTouchPressPerformed;
        _inputActionReferences.screenTap.action.Enable();
    }

    private void OnDisable()
    {
        _inputActionReferences.screenTapPosition.action.performed -= OnTouchPositionPerformed;
        _inputActionReferences.screenTapPosition.action.Disable();
        _inputActionReferences.screenTap.action.started -= OnTouchPressPerformed;
        _inputActionReferences.screenTap.action.canceled -= OnTouchPressPerformed;
        _inputActionReferences.screenTap.action.Disable();
    }

    void OnTouchPositionPerformed(InputAction.CallbackContext context)
    {
        _touchEndPosition = context.ReadValue<Vector2>();
        Debug.Log("터치 포지션");
    }

    void OnTouchPressPerformed(InputAction.CallbackContext context)
    {
        // 터치 눌림
        if (context.ReadValueAsButton())
        {
            Debug.Log("드래그 시작");
            if (_isDragging == false)
            {
                _isDragging = true;
                _touchStartPosition = _touchEndPosition;
                _beginDragTimeMark = context.time;
            }
        }
        // 터치 뗌
        else
        {
            Debug.Log("드래그 끝");
            if (_isDragging)
            {
                _isDragging = false;
                double elapsedDraggingTime = context.time - _beginDragTimeMark; // 드래그 총 시간

                GameManager.Instance.ProcessInput(_touchStartPosition, _touchEndPosition, elapsedDraggingTime);
            }

            HideDragLine();
        }
    }
    private void UpdateDragLine(Vector2 start, Vector2 end, float time)
    {
        if (_line == null || _canvas == null)
        {
            return;
        }

        Vector2 dir = end - start;
        float length = dir.magnitude;

        // 위치 (중간점)
        Vector2 center = start + dir * 0.5f;

        // UI 좌표로 변환
        RectTransform canvasRect = _canvas.transform as RectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            center,
            _canvas.worldCamera,
            out localPoint
        );

        _line.anchoredPosition = localPoint;

        // 길이
        _line.sizeDelta = new Vector2(length, 5f);

        // 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _line.rotation = Quaternion.Euler(0f, 0f, angle);

        // 색 (시간 기반)
        float t = Mathf.Clamp01(time);
        Color color = new Color(t, 1f - t, 0f, 1f);

        UnityEngine.UI.Image img = _line.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.color = color;
        }

        _line.gameObject.SetActive(true);
    }

    private void HideDragLine()
    {
        if (_line != null)
        {
            _line.gameObject.SetActive(false);
        }
    }
}
