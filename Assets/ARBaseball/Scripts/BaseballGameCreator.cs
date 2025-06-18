using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;


/// <summary>
/// AR 공간 평면을 인식하고 터치 입력으로 해당 위치에 게임 프리팹 인스턴싱
/// </summary>
public class BaseballGameCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject baseballGamePrefab;
    [SerializeField]
    private ARRaycastManager _arRaycastManager;
    [SerializeField]
    private InputActionReferences _inputActionReferences;

    private Vector2 _touchPosition;
    private bool _isCreated = false;

    void OnEnable()
    {
        if (_inputActionReferences.screenTapPosition.action != null)
            _inputActionReferences.screenTapPosition.action.performed += OnScreenTapPositionPerformed;

        if (_inputActionReferences.screenTap.action != null)
            _inputActionReferences.screenTap.action.canceled += OnScreenTouchUp;
    }

    private void OnDisable()
    {
        if (_inputActionReferences.screenTapPosition.action != null)
            _inputActionReferences.screenTapPosition.action.performed -= OnScreenTapPositionPerformed;

        if (_inputActionReferences.screenTap.action != null)
            _inputActionReferences.screenTap.action.canceled -= OnScreenTouchUp;
    }

    private void OnScreenTapPositionPerformed(InputAction.CallbackContext obj)
    {
        _touchPosition = obj.ReadValue<Vector2>();
    }

    /// <summary>
    /// 스크린 터치 뗐을 때 baseball game prefab 생성
    /// </summary>
    /// <param name="obj"></param>
    private void OnScreenTouchUp(InputAction.CallbackContext obj)
    {
        if (_isCreated)
            return;

        Debug.Log($"Touch : {_touchPosition}");

        Ray ray = Camera.main.ScreenPointToRay(_touchPosition);
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

        _arRaycastManager.Raycast(ray, hitResults);

        if (hitResults.Count > 0)
        {
            Vector3 spawnPosition = hitResults[0].pose.position;

            GameObject baseballGameObject = Instantiate(baseballGamePrefab, spawnPosition, Quaternion.identity);
            Vector3 direction = Camera.main.transform.position - baseballGameObject.transform.position;
            baseballGameObject.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            _isCreated = true;
        }
    }
}
