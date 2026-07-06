using UnityEngine;
using Manager;
using Unity.Cinemachine;
using System.Collections;

public class Gumsa : MonoBehaviour
{

    void OnEnable() => GlobalManager.OnSceneChanged += OnSceneChanged;
    void OnDisable() => GlobalManager.OnSceneChanged -= OnSceneChanged;
    //카메라 구도 변경 감지
    void OnSceneChanged(Scenes newScene)
    {
        if (newScene == Scenes.whole)
        {
            transform.localScale = new Vector3(0, 0, 0); //사라지기
        }
        else
        {
            {
                StartCoroutine(ExecuteAfterTime(0.1f)); //0.1초 대기
            }
            IEnumerator ExecuteAfterTime(float time)
            {
                yield return new WaitForSeconds(time);
                transform.localScale = new Vector3(27, 27, 1); //다시 보이기
            }
        }
    }
}
