using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentCircleSpawner : MonoBehaviour
{
    public GameObject AgentPrefab;

    public GameObject DummyAgentPrefab;

    public AgentManager agentManager;

    public int NumberOfAgents;
    public float radius = 20f;
    public float theta = 0f;

    float maxAngle = 2*Mathf.PI;

    void Start()
    {
        SpawnAgents();
    }

    void SpawnAgents()
    {
        for (int i = 0; i < NumberOfAgents; i++)
        {
            // Generate random position within the specified constraints
            float x = radius*Mathf.Cos(theta);
            float y = radius*Mathf.Sin(theta);

            Vector3 position = new Vector3(x, y, 0);

            // Instantiate the AgentPrefab at the generated position
            GameObject agent = Instantiate(AgentPrefab, position, Quaternion.identity);
            Agent ag = agent.GetComponent<Agent>();
            ag.attractorFinalGoal = new Vector3(radius*Mathf.Cos(theta + Mathf.PI), radius*Mathf.Sin(theta + Mathf.PI), 0);
            ag.attractor = (Vector2) ag.attractorFinalGoal;
            // ag.personalSpace = i;
            // ag.maxVelocity += i*0.01f;

            theta += maxAngle / NumberOfAgents;

            // NavMeshAgent nav = dummyAgent.AddComponent<NavMeshAgent>();

            // nav.radius = 0.32f;

            // nav.avoidancePriority = 99;

            // nav.autoRepath = false;

            // nav.autoTraverseOffMeshLink = false;

            agentManager.agents.Add(agent);
        }
    }
}
