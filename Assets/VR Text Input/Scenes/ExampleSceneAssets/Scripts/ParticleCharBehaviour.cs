using UnityEngine;

public class ParticleCharBehaviour : MonoBehaviour
{
    void Start()
    {
        //カメラのほうを向く
        var diff = transform.position - Camera.main.transform.position;
        transform.LookAt(transform.position + diff);

        //背景テキストのサイズをランダム化
        GetComponent<TextMesh>().characterSize = Random.Range(0.01f, 0.04f);
    }
}