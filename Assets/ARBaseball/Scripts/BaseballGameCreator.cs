using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


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
    private ARPlaneManager _arPlaneManager;
    [SerializeField]
    private XROrigin _xrOrigin;

    public bool isCreated = false;

    /// <summary>
    /// 스크린 터치 뗐을 때 baseball game prefab 생성
    /// </summary>
    /// <param name="obj"></param>
    public void GenerateBaseballGame(Vector2 touchPosition)
    {
        if (isCreated)
            return;

        Debug.Log($"Touch : {touchPosition}");

        Ray ray = _xrOrigin.Camera.ScreenPointToRay(touchPosition);
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

        _arRaycastManager.Raycast(ray, hitResults);

        if (hitResults.Count > 0)
        {
            Vector3 spawnPosition = hitResults[0].pose.position;

            GameObject baseballGameObject = Instantiate(baseballGamePrefab, spawnPosition, Quaternion.identity);
            Vector3 direction = baseballGameObject.transform.position - _xrOrigin.Camera.transform.position;
            baseballGameObject.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            isCreated = true;

            _arPlaneManager.SetTrackablesActive(false);
            _arPlaneManager.enabled = false;
        }
    }
}
