using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public List<GameObject> agents;
    private List<float> timeSteps;
    private int stepCounter;

    void Start()
    {
        // Initialize the timeSteps list and stepCounter
        timeSteps = new List<float>();
        stepCounter = 0;

        int childCount = transform.childCount;

        // If there is at least one child GameObject, get the reference to the first one
        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++) {
                Transform childTransform = transform.GetChild(i);
                GameObject childGameObject = childTransform.gameObject;
                agents.Add(childGameObject);

            }


            // Now you have a reference to the first child GameObject
            // Debug.Log("The name of the first child GameObject is: " + firstChildGameObject.name);

        }
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
        float timeStep = endTime - startTime;
        timeSteps.Add(timeStep);

        stepCounter++;

        if (stepCounter == 100)
        {
            // Calculate the median
            timeSteps.Sort();
            float median = (timeSteps[49] + timeSteps[50]) / 2;

            // Print the median timestep
            Debug.Log($"Median time step: {median * 1000f} ms");

            // Reset the timeSteps list and stepCounter
            timeSteps.Clear();
            stepCounter = 0;
        }
    }
}
