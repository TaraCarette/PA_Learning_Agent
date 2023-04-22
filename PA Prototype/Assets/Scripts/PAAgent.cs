using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PAAgent : Agent
{
    // bin and rect temporary while goal is to move agent into bin instead of actual goal
    public GameObject bin;
    private Rect goalRect;

    [Tooltip("Found experimentally, not sure how to calculate normally")]
    public float maxSpeed;

    private Vector3 startingSpot;

    private List <FieldOfView> eyeScripts;
    private List <int> rays;


    public override void Initialize()
    {
        // temporary drawing bin to reacts to agent as need dummy task
        goalRect = bin.GetComponent<ShapeInBin>().drawBinRect(bin.transform);


        // save starting spot so can randmize based around it as new episode begins
        startingSpot = transform.localPosition;

        // initiailize empty lists
        eyeScripts = new List<FieldOfView>(); 
        rays = new List<int>(); 
        // finding all eyes attached to agent and the number of rays to go
        // with each eye
        int eyeCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            // if is an eye and active then add to list to be processed
            if (child.transform.tag == "eye" & child.activeInHierarchy)
            {
                eyeScripts.Add(child.GetComponent<FieldOfView>());
                rays.Add((int) ((eyeScripts[eyeCount].viewAngle / 10) * eyeScripts[eyeCount].rayPer10Degree) + 1);
                eyeCount++;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // the current speed - so can tell if running into wall vs object in front of face
        // currently just did experiemntally, will want to calculate more formally for normalization
        sensor.AddObservation(GetComponent<Controller>().currSpeed / maxSpeed);

        // the information from the raycasts is observed
        for (int e = 0; e < eyeScripts.Count; e++)
        {
            for (int i = 0; i < rays[e]; i++) 
            {
                if (eyeScripts[e].hits[i].collider != null)
                {
                    // add normalized distance
                    sensor.AddObservation(eyeScripts[e].hits[i].distance / eyeScripts[e].viewRadius);

                    // add colour being perceived
                    // colour will be represented as a onehot vector
                    if (eyeScripts[e].hits[i].collider.transform.tag == "blue")
                    {
                        sensor.AddOneHotObservation(0, 2);
                    } else if (eyeScripts[e].hits[i].collider.transform.tag == "pink")
                    {
                        sensor.AddOneHotObservation(1, 2);
                    } else {
                        Debug.Log("Should never get here");
                        sensor.AddObservation(new float[2] {-1f, -1f}); //will need to control size but is empty 1 hot is intent
                    }

                } else {
                    // return values standing in for null
                    sensor.AddObservation(-1f);
                    sensor.AddObservation(new float[2] {-1f, -1f}); //will need to control size but is empty 1 hot is intent
                }
            }
        }

        // will need to add if currently sticky or not
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var move = actions.DiscreteActions[0]; // fix to be 4 separate later since diagonal should be option
        var rotate = actions.DiscreteActions[1];

        // make the agent move and rotate as instructed
        GetComponent<Controller>().moveAgent(move);
        GetComponent<Controller>().rotateAgent(rotate);

        // later add turning sticky on and off



        // punish getting stuck unmoving on walls
        if (GetComponent<Controller>().currSpeed < 0.005)
        {
            AddReward(-0.1f);
        }
        // later turn this into more complex curiostity based exploration
        // assigning basic reward of be in the goal thing
        if (goalRect.Contains(transform.position))
        {
            AddReward(1f);
            EndEpisode();
        } else 
        {
            // punish for not reaching goal
            AddReward(-0.01f);
        }

    }

    public override void OnEpisodeBegin()
    {
        // randomize starting position and rotation
        transform.localPosition = new Vector3(Random.Range(startingSpot[0], startingSpot[0] + 4), Random.Range(startingSpot[1] - 4f, startingSpot[1] + 3f), startingSpot[2]);
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
    }
}
