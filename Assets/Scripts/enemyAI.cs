using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class enemyAI : MonoBehaviour {

    private NavMeshAgent navmeshAgent;
    private Transform movePosTransform;
    private Animator enemyAnimator;
    private Rigidbody rb;
    private BoxCollider bc;
    public bool canSeePlayer;

    //line of sight / field of view
    //source : https://www.youtube.com/watch?v=rQG9aUWarwE
    public float viewRadius, viewAngle;
    public LayerMask targetMask, obsMask, groundMask;

    //patrol around randomly, unguided
    //source : https://www.youtube.com/watch?v=UjkSFoLxesw
    public float walkPointRange;
    Vector3 walkPoint;
    public bool walkPointSet;

    // Start is called before the first frame update
    void Start() {
        movePosTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        navmeshAgent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();

        StartCoroutine(findPlayer(0.3f));
    }

    // Update is called once per frame
    void Update() {
        //if the enemy is activated and cant see player then patrol
        if (canSeePlayer) navmeshAgent.destination = movePosTransform.position;
        else { patrol(); }

        //if moving then play move animation
        /*
        if (navmeshAgent.velocity.magnitude > 1) enemyAnimator.SetBool("walking", true);
        else { enemyAnimator.SetBool("walking", false); }
        enemyAnimator.SetFloat("walkingSpeed", navmeshAgent.velocity.magnitude);
        */
    }

    //enemy line of sight / field of view
    public Vector3 dirFromAngle(float angleInDeg, bool isGlobal) {
        if (!isGlobal) { angleInDeg += transform.eulerAngles.y; }
        return new Vector3(Mathf.Sin(angleInDeg * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDeg * Mathf.Deg2Rad));
    }

    void findVisibleTargets() {
        canSeePlayer = false;
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInView.Length; i++) {
            Transform target = targetsInView[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
                float distToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obsMask)) {
                    canSeePlayer = true;
                }
            }
        }
    }

    IEnumerator findPlayer(float delay) {
        while (true) {
            yield return new WaitForSeconds(delay);
            findVisibleTargets();
        }
    }

    //patrols enemy around randomly without move/patrol points
    //code learnt from : https://www.youtube.com/watch?v=UjkSFoLxesw
    void patrol() {
        if (!walkPointSet) searchWalkPoint();
        if (walkPointSet) navmeshAgent.SetDestination(walkPoint);

        Vector3 distToWalk = transform.position - walkPoint;
        if (distToWalk.magnitude < 1f) walkPointSet = false;
    }

    void searchWalkPoint() {
        float randZ = Random.Range(-walkPointRange, walkPointRange);
        float randX = Random.Range(-walkPointRange, walkPointRange);
        walkPoint = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);
        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask)) walkPointSet = true;
    }
}
