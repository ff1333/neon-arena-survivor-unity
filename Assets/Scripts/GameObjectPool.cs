using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class GameObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField, Min(1)] private int initialSize = 16;

    private readonly Queue<GameObject> available = new Queue<GameObject>();
    
    private void Awake()
    {
        Warm(initialSize);
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject instance = available.Count > 0 ? available.Dequeue() : CreateInstance();
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    public void Release(GameObject instance)
    {
        instance.SetActive(false);
        instance.transform.SetParent(transform);
        available.Enqueue(instance);
    }

    private void Warm(int count)
    {
        if (prefab == null)
        {
            Debug.LogError($"{name} has no prefab assigned.", this);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject instance = CreateInstance();
            instance.SetActive(false);
            available.Enqueue(instance);
        }
    }

    private GameObject CreateInstance()
    {
        GameObject instance = Instantiate(prefab, transform);
        PoolMember member = instance.GetComponent<PoolMember>();
        if (member == null)
        {
           member = instance.AddComponent<PoolMember>();
        }
        member.SetOwner(this);
        return instance;
    }
}
