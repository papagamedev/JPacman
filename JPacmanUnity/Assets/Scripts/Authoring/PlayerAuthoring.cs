using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public float CollisionRadius;

    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Player
            {
            });
            AddComponent(entity, new CollisionCircle()
            {
                Radius = authoring.CollisionRadius
            });            
        }
    }
}


