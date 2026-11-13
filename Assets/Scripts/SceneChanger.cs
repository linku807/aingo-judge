using UnityEngine;
using UnityEngine.SceneManagement; // 신 전환을 위해 반드시 필요한 네임스페이스

public class SceneChanger : MonoBehaviour
{
    // 버튼에 연결할 메서드
    public void ChangeToSampleScene()
    {
        // "SampleScene" 부분에 전환하고자 하는 신의 정확한 이름을 적어줍니다.
        SceneManager.LoadScene("SampleScene"); 
    }
}