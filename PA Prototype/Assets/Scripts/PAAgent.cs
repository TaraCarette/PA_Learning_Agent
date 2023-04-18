using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PAAgent : Agent
{
    public GameObject bin;

    private float speed;
    private float rotationSpeed;
    private Vector3 startingSpot;

    private Rect goalRect;

    public override void Initialize()
    {
        // might move controller functionality over here since just took stuff anyways
        speed = GetComponent<Controller>().speed;
        rotationSpeed = GetComponent<Controller>().rotationSpeed;


        // might be best to move goal bin stuff over here as well since redoing
        goalRect = bin.GetComponent<ShapeInBin>().drawBinRect(bin.transform);

        startingSpot = transform.localPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // need to add more later
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var move = actions.DiscreteActions[0]; // fix to be 4 separate later since diagonal should be option
        var rotate = actions.DiscreteActions[1];

        if (move == 0)
        {
            transform.Translate(new Vector3(0, speed, 0) * Time.deltaTime);
        }
        if (move == 1)
        {
            transform.Translate(new Vector3(0, -speed, 0) * Time.deltaTime);
        }
        if (move == 2)
        {
            transform.Translate(new Vector3(-speed, 0, 0) * Time.deltaTime);
        }
        if (move == 3)
        {
            transform.Translate(new Vector3(speed, 0, 0) * Time.deltaTime);
        }


        if (rotate == 0)
        {
            transform.Rotate(new Vector3(0, 0, rotationSpeed) * Time.deltaTime);
        }
        if (rotate == 1)
        {
            transform.Rotate(new Vector3(0, 0, -rotationSpeed) * Time.deltaTime);
        }



        // assigning basic reward of be in the goal thing
        if (goalRect.Contains(transform.position))
        {
            SetReward(1f);
            EndEpisode();
        } else 
        {
            // punish for not reaching goal
            SetReward(-0.1f);
        }

    }

    public override void OnEpisodeBegin()
    {
        // return to same initial spot
        // might later want to randomize starting position more
        transform.localPosition = startingSpot;
        transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
    }
}
