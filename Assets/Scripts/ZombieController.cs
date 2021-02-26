using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    
    public Transform transformToFollow;
    public Animator animator;
    

    NavMeshAgent agent;
    

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
       
    }

    // Update is called once per frame
    void Update()
    {
        //Follow the player
        agent.destination = transformToFollow.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit");
    }
}