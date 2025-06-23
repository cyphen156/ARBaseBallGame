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

#if UNITY_EDITOR
    private void Update()
    {
        if (Mouse.current == null)
            return;

        // 마우스 버튼 눌림
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _isDragging = true;
            _touchStartPosition = Mouse.current.position.ReadValue();
            _beginDragTimeMark = Time.timeAsDouble;
        }

        // 마우스 버튼 뗌
        if (_isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _isDragging = false;
            _touchEndPosition = Mouse.current.position.ReadValue();

            double elapsedDraggingTime = Time.timeAsDouble - _beginDragTimeMark;

            GameManager.Instance.ProcessInput(_touchStartPosition, _touchEndPosition, elapsedDraggingTime);
        }
    }
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
        }
    }
}
