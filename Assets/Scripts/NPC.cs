using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// NPC�� ���¸� ��Ÿ���� ������(enum)
public enum AIState
{
    Idle,       // ��� ����
    Wandering,  // ��ȸ ����
    Attacking   // ���� ����
}

public class NPC : MonoBehaviour, IDamagable
{
    [Header("Stats")]                  // NPC�� �⺻ ���� (ü��, �̵� �ӵ�, ������ ��� ��)
    public int health;                 // ü��
    public float walkSpeed;            // �ȴ� �ӵ�
    public float runSpeed;             // �ٴ� �ӵ�
    public ItemData[] dropOnDeath;     // ��� �� ����� ������ ���

    [Header("AI")]                     // NPC�� AI ���� ����
    private NavMeshAgent agent;        // �׺���̼� ������Ʈ (��ã�� ��� ����)
    public float detectDistance;       // �÷��̾ �����ϴ� �Ÿ�
    private AIState aiState;           // ���� AI ����

    [Header("Wandering")]              // ��ȸ ���� ����
    public float minWanderDistance;    // �ּ� ��ȸ �Ÿ�
    public float maxWanderDistance;    // �ִ� ��ȸ �Ÿ�
    public float minWanderWaitTime;    // ��ȸ �� �ּ� ��� �ð�
    public float maxWanderWaitTime;    // ��ȸ �� �ִ� ��� �ð�

    [Header("Combat")]                 // ���� ���� ����
    public int damage;                 // ���ݷ�
    public float attackRate;           // ���� �ӵ� (���� ����)
    private float lastAttackTime;      // ������ ���� �ð�
    public float attackDistance;       // ���� ������ �Ÿ�

    private float playerDistance;      // �÷��̾���� �Ÿ� ��� ����
    public float fieldOfView = 120f;   // �þ߰�

    private Animator animator;                      // �ִϸ����� (�ִϸ��̼� ����)
    private SkinnedMeshRenderer[] meshRenderers;    // NPC�� ������ ����ϴ� �޽� ������

    private void Awake()
    {
        // �ʱ�ȭ: NPC�� �ֿ� ������Ʈ ��������
        agent = GetComponent<NavMeshAgent>();                            // �׺���̼� ������Ʈ ��������
        animator = GetComponentInChildren<Animator>();                   // �ڽ� ������Ʈ���� �ִϸ����� ã��
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();  // ��Ų ������ ã��
    }

    void Start()
    {
        // ������ ���۵Ǹ� NPC�� �ʱ� ���¸� Wandering(��ȸ)�� ����
        SetState(AIState.Wandering);
    }

    void Update()
    {
        // �÷��̾���� �Ÿ� ���
        playerDistance = Vector3.Distance(transform.position, CharacterManager.Instance.Player.transform.position);

        // �����̴� �ִϸ��̼� ���� (Idle ������ ���� "Moving"�� false�� ����)
        animator.SetBool("Moving", aiState != AIState.Idle);

        // ���� ���¿� ���� ������Ʈ ���� ����
        switch (aiState)
        {
            case AIState.Idle:
                PassiveUpdate();
                break;
            case AIState.Wandering:
                PassiveUpdate();
                break;
            case AIState.Attacking:
                AttackingUpdate();
                break;
        }
    }

    private void SetState(AIState state)
    {
        // AI ���� ���� �� ���� ���� ����
        aiState = state;

        switch (aiState)
        {
            case AIState.Idle:
                agent.speed = walkSpeed; // �ȴ� �ӵ��� ����
                agent.isStopped = true;  // �̵� ����
                break;
            case AIState.Wandering:
                agent.speed = walkSpeed; // �ȴ� �ӵ��� ����
                agent.isStopped = false; // �̵� ����
                break;
            case AIState.Attacking:
                agent.speed = runSpeed;  // �ٴ� �ӵ��� ����
                agent.isStopped = false; // �̵� ����
                break;
        }

        // �ִϸ��̼� �ӵ��� �̵� �ӵ��� �°� ����
        animator.speed = agent.speed / walkSpeed;
    }

    void PassiveUpdate()
    {
        // ��ȸ ���¿��� �������� �����ϸ� ��� ���·� ��ȯ �� ���� �ð��� ������ �ٽ� ��ȸ
        if (aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }

        // �÷��̾ ���� ���� ���� ������ ���� ���·� ��ȯ
        if (playerDistance < detectDistance)
        {
            SetState(AIState.Attacking);
        }
    }

    void WanderToNewLocation()
    {
        // ��� ������ ���� ���ο� ��ȸ ��ġ�� �̵�
        if (aiState != AIState.Idle) return;

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation()); // ���ο� �������� ����
    }

    Vector3 GetWanderLocation()
    {
        NavMeshHit hit;

        // ������ �������� �̵��� ��ġ�� ã��
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);

        // �÷��̾� ���� ���� ���� ��ġ�� �������� �ʵ��� ����
        int i = 0;
        while (Vector3.Distance(transform.position, hit.position) < detectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);
            i++;
            if (i == 30) 
                break; // 30�� �ݺ��ص� ��ȿ�� ��ġ�� ã�� ���ϸ� �ߴ�
        }

        return hit.position;
    }
    void AttackingUpdate()
    {       
        if(playerDistance < attackDistance && IsPlayerInFieldOfView())      // �÷��̾���� �Ÿ��� ���� �Ÿ����� ª��, �÷��̾ �þ߰� ���� ���� ���
        {    
            agent.isStopped = true;                                          // �׺���̼� ������Ʈ(�� AI�� �̵��� ����ϴ� ������Ʈ)�� ����
            if(Time.time - lastAttackTime > attackRate)                     // ������ ���� �ð����κ��� ���� �ӵ�(attackRate)���� �� ���� ����ߴ��� Ȯ��
            {               
                lastAttackTime = Time.time;                                                                         // ������ ���� �ð��� ���� �ð����� ������Ʈ
                CharacterManager.Instance.Player.controller.GetComponent<IDamagable>().TakePhysicalDamage(damage);  // �÷��̾�� ������ ���ظ� ����

                // �ִϸ����� �ӵ��� ���������� �����ϰ� ���� �ִϸ��̼��� ���
                animator.speed = 1;
                animator.SetTrigger("Attack");
            }
            else
            {               
                if(playerDistance < detectDistance)        // �÷��̾ ���� ���� ���� ���� ���
                {                    
                    agent.isStopped = false;                // �׺���̼� ������Ʈ �̵� �簳
                    NavMeshPath path = new NavMeshPath();
                    if(agent.CalculatePath(CharacterManager.Instance.Player.transform.position, path))     // ����޽� ��� ���
                    {                      
                        agent.SetDestination(CharacterManager.Instance.Player.transform.position);  // �÷��̾��� ��ġ�� ���� �̵�
                    }
                }
                else // �÷��̾ ���� ������ ��� ���
                {               
                    agent.SetDestination(transform.position);   // ���� ��ġ�� �����ϸ鼭 �̵� ����
                    agent.isStopped = true;
                    SetState(AIState.Wandering);                // AI ���¸� '��ȸ(Wandering)' ���·� ����
                }
            }
        }
        else // �÷��̾ ���� ���� �ۿ� ���� ���
        {            
            agent.SetDestination(transform.position);           // ���� ��ġ�� �����ϸ鼭 �̵� ����
            agent.isStopped = true;
            SetState(AIState.Wandering);                        // AI ���¸� '��ȸ(Wandering)' ���·� ����
        }
    }

    bool IsPlayerInFieldOfView()
    {
        // �÷��̾�� �� ������ ���� ���� ���
        Vector3 directionToPlayer = CharacterManager.Instance.Player.transform.position - transform.position;
        // ���� ���� ����(transform.forward)�� �÷��̾� ���� ���� ���� ���� ���
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        // ���� ������ �þ߰�(fieldOfView)�� ���ݺ��� ������ �÷��̾ �þ� ���� ����
        return angle < fieldOfView * 0.5f;
    }

  public void TakePhysicalDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        StartCoroutine(DamageFlash());
    }

    void Die()
    {
        for (int i = 0; i < dropOnDeath.Length; i++)
        {
            Instantiate(dropOnDeath[i].dropPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    IEnumerator DamageFlash()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].material.color = new Color(1.0f, 0.6f, 0.6f);

        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].material.color = Color.white;
    }
}