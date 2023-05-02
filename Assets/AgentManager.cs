using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public List<GameObject> agents;
    private float timeStep;

    void Start()
    {
        // Initialize the timeStep variable
        timeStep = 0f;
    }

    void FixedUpdate()
    {
        // Record the start time
        float startTime = Time.realtimeSinceStartup;

        // Execute the FixedUpdate() logic in the AgentScript component for each agent
        foreach (GameObject agent in agents)
        {
            Agent agentScript = agent.GetComponent<Agent>();
            if (agentScript != null)
            {
                // Since FixedUpdate() is called automatically by Unity, 
                // you should create a separate public method in your AgentScript, 
                // such as `AgentFixedUpdate()`, that contains the logic for the agent movement
                agentScript.AgentFixedUpdate();
            }
        }

        // Record the end time
        float endTime = Time.realtimeSinceStartup;

        // Calculate the timeStep
        timeStep = endTime - startTime;

        // You can print the timeStep to the console for debugging purposes
        Debug.Log($"Time step: {timeStep * 1000f} ms");
    }
}

