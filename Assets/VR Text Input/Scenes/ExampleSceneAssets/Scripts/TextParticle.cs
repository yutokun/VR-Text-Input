using UnityEngine;

public class TextParticle : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] string characters;

    [SerializeField, Header("配置")] float radius = 20f;

    void Start()
    {
        foreach (var item in characters)
        {
            var obj = Instantiate(prefab, Random.insideUnitSphere * radius, Quaternion.identity, transform);
            obj.GetComponent<TextMesh>().text = item.ToString();
        }
    }
}