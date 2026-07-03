using UnityEngine;
using Manager;
using Unity.Cinemachine;

public class Character : MonoBehaviour
{

    void OnEnable() => GlobalManager.OnSceneChanged += OnSceneChanged;
    void OnDisable() => GlobalManager.OnSceneChanged -= OnSceneChanged;

    // Update is called once per frame
    void OnSceneChanged(Scenes newScene)
    {
        if (newScene == Scenes.whole)
        {
            transform.localScale = new Vector3(0, 0, 0);
        }else{
            transform.localScale = new Vector3(30, 30, 1);
        }
    }
}
