using Unity.Entities;
using UnityEngine;

public class FieldOfViewAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public FieldOfViewData fieldOfViewData;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, fieldOfViewData);
    }
}
