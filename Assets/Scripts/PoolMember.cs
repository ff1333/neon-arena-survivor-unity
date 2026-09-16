using UnityEngine;

public class PoolMember : MonoBehaviour
{
    private GameObjectPool owner;
    private bool isReleased;

    public void SetOwner(GameObjectPool newOwner)
    {
        owner = newOwner;
    }

    private void OnEnable()
    {
        isReleased = false;
    }

    public void Release()
    {
        if (isReleased)
        {
            return;
        }

        isReleased = true;

        if (owner == null)
        {
            Debug.LogError($"{name} has no object pool owner.", this);
            return;
        }

        owner.Release(gameObject);
    }
}
