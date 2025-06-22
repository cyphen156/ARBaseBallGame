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

        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

        _arRaycastManager.Raycast(ray, hitResults);

        if (hitResults.Count > 0)
        {
            Vector3 spawnPosition = hitResults[0].pose.position;

            GameObject baseballGameObject = Instantiate(baseballGamePrefab, spawnPosition, Quaternion.identity);
            Vector3 direction = Camera.main.transform.position - baseballGameObject.transform.position;
            baseballGameObject.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            isCreated = true;
        }
    }
}
