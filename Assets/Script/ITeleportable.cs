using UnityEngine;

public interface ITeleportable
{
    Transform GetTransform();
    void OnTeleport();
}
