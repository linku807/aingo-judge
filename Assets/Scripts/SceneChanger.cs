using UnityEngine;
using Manager;
using Unity.Cinemachine;

public class MovingCamera : MonoBehaviour
{
    private Scenes self = Scenes.main;
    private CinemachineCamera virtualCamera;
    void OnEnable() => GlobalManager.OnSceneChanged += OnSceneChanged;
    void OnDisable() => GlobalManager.OnSceneChanged -= OnSceneChanged;
    void Start()
    {
        virtualCamera = GetComponent<CinemachineCamera>();

        if(this.gameObject.name == "main camera") self = Scenes.main;
        else if(this.gameObject.name == "right camera") self = Scenes.byunhosa;
        else if(this.gameObject.name == "top camera") self = Scenes.jepanjang;
        else if(this.gameObject.name == "left camera") self = Scenes.gumsa;
        else if(this.gameObject.name == "whole camera") self = Scenes.whole;
    }

    // Update is called once per frame
    void OnSceneChanged(Scenes newScene)
    {
        if (newScene == self)
        {
            virtualCamera.Priority = 1;
        }else{
            virtualCamera.Priority = 0;
        }
    }
}
