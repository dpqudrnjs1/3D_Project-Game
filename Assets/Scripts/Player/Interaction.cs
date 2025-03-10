using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f;     // 상호작용을 체크하는 주기 (초 단위)
    private float lastCheckTime;        // 마지막으로 체크한 시간
    public float maxCheckDistance;      // 최대 체크 거리
    public LayerMask layerMask;         // 충돌을 감지할 레이어 설정


    // 현재 상호작용 가능한 게임 오브젝트 및 인터페이스

    public GameObject curInteractGameObject;
    private IInteractable curInteractable;
    
    public TextMeshProUGUI promptText;      // 화면에 표시될 상호작용 프롬프트 텍스트
    private Camera camera;                  // 카메라 참조

    void Start()
    {
        camera = Camera.main;               // 메인 카메라를 가져옴
    }

    // 매 프레임마다 실행됨
    void Update()
    {        
        if (Time.time - lastCheckTime > checkRate)   // 설정된 체크 주기(checkRate)보다 긴 시간이 지나면 실행
        {
            lastCheckTime = Time.time;              // 마지막 체크 시간 갱신


            // 화면 중앙에서 레이를 쏘아 충돌 검사
            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            // 레이가 특정 오브젝트와 충돌하면 실행
            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                // 새로운 오브젝트를 감지했을 경우 업데이트
                if (hit.collider.gameObject != curInteractGameObject)
                {
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();
                    SetPromptText(); // 프롬프트 텍스트 설정
                }
            }

            // 충돌하는 오브젝트가 없다면 초기화
            else
            {
                curInteractGameObject = null;
                curInteractable = null;
                promptText.gameObject.SetActive(false); // 프롬프트 텍스트 숨김
            }
        }
    }

    // 상호작용 프롬프트 텍스트 설정
    private void SetPromptText()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = curInteractable.GetInteractPrompt(); // 인터페이스에서 프롬프트 메시지 가져옴
    }

    // 상호작용 입력이 들어왔을 때 실행
    public void OnInteractInput(InputAction.CallbackContext context)
    {
        // 입력이 시작되었고 현재 상호작용 가능한 오브젝트가 있을 경우
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            curInteractable.OnInteract(); // 상호작용 수행
            curInteractGameObject = null;
            curInteractable = null;
            promptText.gameObject.SetActive(false); // 프롬프트 텍스트 숨김
        }
    }
}
