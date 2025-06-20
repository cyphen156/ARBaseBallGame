using UnityEngine;

public class UIBehaviour : MonoBehaviour
{

    protected string ButtonParser()
    {
        string buttonName = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

        // 버튼 이름에서 "Button" 접두사를 제거합니다.
        const string prefix = "Button";
        if (buttonName.StartsWith(prefix))
        {
            buttonName = buttonName.Substring(prefix.Length);
        }

        return buttonName;
    }
}
