using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[GenerateAuthoringComponent]
public struct FieldOfViewData : IComponentData
{
    public float visLong;
    public float visWide;
    public float myPersonalSpace;
    public bool useNaive;
    public float myRadius;
    public float detectionRadius;
    public LayerMask agentMask;
    public LayerMask wallMask;
    public LayerMask fallenAgentMask;
    public LayerMask obstacleMask;
}

public class VisibleAgents : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<Transform> visibleAgents = new List<Transform>();
    public List<Transform> visibleWalls = new List<Transform>();
    public List<Transform> visibleFallenAgents = new List<Transform>();
    public List<Transform> collidedAgents = new List<Transform>();
    public List<Transform> collidedWalls = new List<Transform>();
    public List<Transform> collidedObstacles = new List<Transform>();

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentObject(entity, visibleAgents);
        dstManager.AddComponentObject(entity, visibleWalls);
        dstManager.AddComponentObject(entity, visibleFallenAgents);
        dstManager.AddComponentObject(entity, collidedAgents);
        dstManager.AddComponentObject(entity, collidedWalls);
        dstManager.AddComponentObject(entity, collidedObstacles);
    }
}