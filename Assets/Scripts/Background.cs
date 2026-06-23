using UnityEngine;

public class Square1 : MonoBehaviour
{
    float speed = 3f;
    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
    }
}

public class Square2 : MonoBehaviour
{
    float speed = 5f;
    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
    }
}
