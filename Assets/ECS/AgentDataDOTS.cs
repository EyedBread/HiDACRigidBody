using Unity.Entities;
using Unity.Mathematics;

public struct AgentData : IComponentData
{
    public const float Epsilon = 0.05f;
    public bool IsFallen;
    public float RepelsionMagThreshold;
    public float DetectionRadius;
    public float2 PrevForce;
    public float2 Velocity;
    // Add other fields that you require in your Agent component
}