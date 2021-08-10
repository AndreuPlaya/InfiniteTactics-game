using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ConvertedEntityHolder : MonoBehaviour, IConvertGameObjectToEntity
{
    private Entity entity;
    private EntityManager entityManager;
    void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
    {
        this.entity = entity;
        this.entityManager = entityManager;
    }

    public Entity GetEntity()
    {
        return entity;
    }

    public EntityManager GetEntityManager()
    {
        return entityManager;
    }
}
