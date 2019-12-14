using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

namespace UnityStandardAssets.Vehicles.Car {
    [RequireComponent(typeof(CarController))]
    public class CarAgent : Agent {
        private CarController carController;
        private Rigidbody rigidBody;
        public Transform resetPoint;

        private float lapTime = 0;
        private float bestLapTime = 0;
        private bool isCollided = false;
        private bool startLinePassed = false;
        public bool resetToClosestWaypoint = false;
        private List<string> commands = new List<string>();

        public Transform[] trackWaypoints = new Transform[14];

        // When the object enters the scene
        public void Awake() {
            carController = GetComponent<CarController>();
            rigidBody = GetComponent<Rigidbody>();
            Debug.Log("AWOKEN");
            //file.Open();
        }

        // When the agent requests an action
        // Called every tick to check what the car should do next
        public override void AgentAction(float[] vectorAction, string textAction) {
            // agent's action code goes here
            float h = vectorAction[0]; // this will be -1 or 1 (left or right)
            Debug.Log(h);

            if (h > 0.5)
            {
                Debug.Log("RIGHT");
                commands.Add("RIGHT");
            }
            else if (h < -0.5)
            {
                Debug.Log("LEFT");
                commands.Add("LEFT");
            }
            else
            {
                Debug.Log("STRAIGHT");
                commands.Add("STRAIGHT");
            }

            carController.Move(h, 1, 0, 0);

            if(isCollided)
            {
                // we hit something
                AddReward(-1.0f);
                Done();
            } else
            {
                //hit nothing
                AddReward(0.05f);
            }
        }

        public override void CollectObservations() {
            // collect observations here, if you're not just using visual obs
        }

        public override void AgentReset() {
            // Reset to closest waypoint if we're training
            if(resetToClosestWaypoint) {
                float min_distance = 1e+6f;
                int index = 0;
                for(int i = 1; i < trackWaypoints.Length; i++) {
                    float distance = Vector3.SqrMagnitude(trackWaypoints[i].position - transform.position);
                    if(distance < min_distance) {
                        min_distance = distance;
                        index = i;
                    }
                }
                transform.SetPositionAndRotation(trackWaypoints[index-1].position, new Quaternion(0,0,0,0));
                transform.LookAt(trackWaypoints[index].position);
            } else {
                // Reset to beginning if we're NOT training
                lapTime = 0;
                transform.position = resetPoint.position;
                transform.rotation = resetPoint.rotation;
            }

            // No matter whether we're training or not, we also need to:
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            isCollided = false;
        }
        
        void FixedUpdate() {
            lapTime += Time.fixedDeltaTime;
            Debug.Log(string.Format("Delta Time: {0}", Time.deltaTime)); //0.02 seconds

        }

        private void Update() {
            // float angle = Mathf.Clamp(steering, -1, 1) * 90f;
            // wheelFrontLeft.localEulerAngles = new Vector3(0f, -90f + angle / 3, 0f);
            // wheelFrontRight.localEulerAngles = new Vector3(0f, -90f + angle / 3, 0f);
        }

        private void OnTriggerEnter(Collider other) {
            // if we hit the start line...
            if(other.CompareTag("StartLine")) {
                if(!startLinePassed) {
                    if (lapTime < bestLapTime) {
                        bestLapTime = lapTime;
                    }
                    Debug.Log("Lap completed: " + lapTime);
                    lapTime = 0;
                    startLinePassed = true;
                }
            } else {
                // we hit a wall...
                isCollided = true;
                System.IO.File.WriteAllLines(@"Assets/commands.txt", commands);
            }
        }

        private void OnTriggerExit(Collider other) {
            startLinePassed = false;
        }
    }
}