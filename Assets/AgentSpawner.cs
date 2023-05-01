using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentSpawner : MonoBehaviour
{
    public GameObject AgentPrefab;

    public GameObject DummyAgentPrefab;
    public int NumberOfAgents;
    public float WorldX = 10f;
    public float WorldY = 10f;
    public float Margin = 0.1f;

    void Start()
    {
        SpawnAgents();
    }

    void SpawnAgents()
    {
        for (int i = 0; i < NumberOfAgents; i++)
        {
            // Generate random position within the specified constraints
            float x = Random.Range(Margin, WorldX - Margin);
            float y = Random.Range(Margin, WorldY - Margin);

            Vector3 position = new Vector3(x, y, 0);

            // Instantiate the AgentPrefab at the generated position
            GameObject agent = Instantiate(AgentPrefab, position, Quaternion.identity);
            Agent ag = agent.GetComponent<Agent>();
            ag.attractorFinalGoal = new Vector3(1, 8, 0);
            GameObject dummyAgent = Instantiate(DummyAgentPrefab, position, Quaternion.identity);

            // NavMeshAgent nav = dummyAgent.AddComponent<NavMeshAgent>();

            // nav.radius = 0.32f;

            // nav.avoidancePriority = 99;

            // nav.autoRepath = false;

            // nav.autoTraverseOffMeshLink = false;

            dummyAgent.transform.SetParent(agent.transform);
        }
    }
}
