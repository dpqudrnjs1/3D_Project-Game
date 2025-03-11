using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// NPC의 상태를 나타내는 열거형(enum)
public enum AIState
{
    Idle,       // 대기 상태
    Wandering,  // 배회 상태
    Attacking   // 공격 상태
}

public class NPC : MonoBehaviour, IDamagable
{
    [Header("Stats")]                  // NPC의 기본 스탯 (체력, 이동 속도, 아이템 드롭 등)
    public int health;                 // 체력
    public float walkSpeed;            // 걷는 속도
    public float runSpeed;             // 뛰는 속도
    public ItemData[] dropOnDeath;     // 사망 시 드롭할 아이템 목록

    [Header("AI")]                     // NPC의 AI 관련 변수
    private NavMeshAgent agent;        // 네비게이션 에이전트 (길찾기 기능 제공)
    public float detectDistance;       // 플레이어를 감지하는 거리
    private AIState aiState;           // 현재 AI 상태

    [Header("Wandering")]              // 배회 관련 변수
    public float minWanderDistance;    // 최소 배회 거리
    public float maxWanderDistance;    // 최대 배회 거리
    public float minWanderWaitTime;    // 배회 전 최소 대기 시간
    public float maxWanderWaitTime;    // 배회 전 최대 대기 시간

    [Header("Combat")]                 // 전투 관련 변수
    public int damage;                 // 공격력
    public float attackRate;           // 공격 속도 (공격 간격)
    private float lastAttackTime;      // 마지막 공격 시간
    public float attackDistance;       // 공격 가능한 거리

    private float playerDistance;      // 플레이어와의 거리 계산 변수
    public float fieldOfView = 120f;   // 시야각

    private Animator animator;                      // 애니메이터 (애니메이션 관리)
    private SkinnedMeshRenderer[] meshRenderers;    // NPC의 외형을 담당하는 메쉬 렌더러

    private void Awake()
    {
        // 초기화: NPC의 주요 컴포넌트 가져오기
        agent = GetComponent<NavMeshAgent>();                            // 네비게이션 에이전트 가져오기
        animator = GetComponentInChildren<Animator>();                   // 자식 오브젝트에서 애니메이터 찾기
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();  // 스킨 렌더러 찾기
    }

    void Start()
    {
        // 게임이 시작되면 NPC의 초기 상태를 Wandering(배회)로 설정
        SetState(AIState.Wandering);
    }

    void Update()
    {
        // 플레이어와의 거리 계산
        playerDistance = Vector3.Distance(transform.position, CharacterManager.Instance.Player.transform.position);

        // 움직이는 애니메이션 설정 (Idle 상태일 때는 "Moving"을 false로 설정)
        animator.SetBool("Moving", aiState != AIState.Idle);

        // 현재 상태에 따라 업데이트 로직 실행
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
        // AI 상태 변경 및 관련 설정 조정
        aiState = state;

        switch (aiState)
        {
            case AIState.Idle:
                agent.speed = walkSpeed; // 걷는 속도로 설정
                agent.isStopped = true;  // 이동 중지
                break;
            case AIState.Wandering:
                agent.speed = walkSpeed; // 걷는 속도로 설정
                agent.isStopped = false; // 이동 가능
                break;
            case AIState.Attacking:
                agent.speed = runSpeed;  // 뛰는 속도로 설정
                agent.isStopped = false; // 이동 가능
                break;
        }

        // 애니메이션 속도를 이동 속도에 맞게 조정
        animator.speed = agent.speed / walkSpeed;
    }

    void PassiveUpdate()
    {
        // 배회 상태에서 목적지에 도착하면 대기 상태로 전환 후 일정 시간이 지나면 다시 배회
        if (aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }

        // 플레이어가 감지 범위 내에 들어오면 공격 상태로 전환
        if (playerDistance < detectDistance)
        {
            SetState(AIState.Attacking);
        }
    }

    void WanderToNewLocation()
    {
        // 대기 상태일 때만 새로운 배회 위치로 이동
        if (aiState != AIState.Idle) return;

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation()); // 새로운 목적지를 설정
    }

    Vector3 GetWanderLocation()
    {
        NavMeshHit hit;

        // 랜덤한 방향으로 이동할 위치를 찾음
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);

        // 플레이어 감지 범위 내에 위치가 설정되지 않도록 조정
        int i = 0;
        while (Vector3.Distance(transform.position, hit.position) < detectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);
            i++;
            if (i == 30) 
                break; // 30번 반복해도 유효한 위치를 찾지 못하면 중단
        }

        return hit.position;
    }
    void AttackingUpdate()
    {       
        if(playerDistance < attackDistance && IsPlayerInFieldOfView())      // 플레이어와의 거리가 공격 거리보다 짧고, 플레이어가 시야각 내에 있을 경우
        {    
            agent.isStopped = true;                                          // 네비게이션 에이전트(적 AI의 이동을 담당하는 컴포넌트)를 멈춤
            if(Time.time - lastAttackTime > attackRate)                     // 마지막 공격 시간으로부터 공격 속도(attackRate)보다 더 오래 경과했는지 확인
            {               
                lastAttackTime = Time.time;                                                                         // 마지막 공격 시간을 현재 시간으로 업데이트
                CharacterManager.Instance.Player.controller.GetComponent<IDamagable>().TakePhysicalDamage(damage);  // 플레이어에게 물리적 피해를 입힘

                // 애니메이터 속도를 정상적으로 설정하고 공격 애니메이션을 재생
                animator.speed = 1;
                animator.SetTrigger("Attack");
            }
            else
            {               
                if(playerDistance < detectDistance)        // 플레이어가 감지 범위 내에 있을 경우
                {                    
                    agent.isStopped = false;                // 네비게이션 에이전트 이동 재개
                    NavMeshPath path = new NavMeshPath();
                    if(agent.CalculatePath(CharacterManager.Instance.Player.transform.position, path))     // 내비메시 경로 계산
                    {                      
                        agent.SetDestination(CharacterManager.Instance.Player.transform.position);  // 플레이어의 위치를 따라 이동
                    }
                }
                else // 플레이어가 감지 범위를 벗어난 경우
                {               
                    agent.SetDestination(transform.position);   // 현재 위치를 유지하면서 이동 정지
                    agent.isStopped = true;
                    SetState(AIState.Wandering);                // AI 상태를 '배회(Wandering)' 상태로 변경
                }
            }
        }
        else // 플레이어가 공격 범위 밖에 있을 경우
        {            
            agent.SetDestination(transform.position);           // 현재 위치를 유지하면서 이동 정지
            agent.isStopped = true;
            SetState(AIState.Wandering);                        // AI 상태를 '배회(Wandering)' 상태로 변경
        }
    }

    bool IsPlayerInFieldOfView()
    {
        // 플레이어와 적 사이의 방향 벡터 계산
        Vector3 directionToPlayer = CharacterManager.Instance.Player.transform.position - transform.position;
        // 적의 정면 방향(transform.forward)과 플레이어 방향 벡터 간의 각도 계산
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        // 계산된 각도가 시야각(fieldOfView)의 절반보다 작으면 플레이어가 시야 내에 있음
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