using UnityEngine;

public class ParticleCharBehaviour : MonoBehaviour
{
    void Start()
    {
        //カメラのほうを向く
        var diff = transform.position - Camera.main.transform.position;
        transform.LookAt(transform.position + diff);
    }
}