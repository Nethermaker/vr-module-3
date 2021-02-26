using UnityEngine;
using UnityEngine.AI;

public class ZombieFollow : MonoBehaviour
{
    
    public Transform transformToFollow;
   
    NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //Follow the player
        agent.destination = transformToFollow.position;
    }
}