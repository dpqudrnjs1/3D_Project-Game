using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f;     // ��ȣ�ۿ��� üũ�ϴ� �ֱ� (�� ����)
    private float lastCheckTime;        // ���������� üũ�� �ð�
    public float maxCheckDistance;      // �ִ� üũ �Ÿ�
    public LayerMask layerMask;         // �浹�� ������ ���̾� ����


    // ���� ��ȣ�ۿ� ������ ���� ������Ʈ �� �������̽�

    public GameObject curInteractGameObject;
    private IInteractable curInteractable;
    
    public TextMeshProUGUI promptText;      // ȭ�鿡 ǥ�õ� ��ȣ�ۿ� ������Ʈ �ؽ�Ʈ
    private Camera camera;                  // ī�޶� ����

    void Start()
    {
        camera = Camera.main;               // ���� ī�޶� ������
    }

    // �� �����Ӹ��� �����
    void Update()
    {        
        if (Time.time - lastCheckTime > checkRate)   // ������ üũ �ֱ�(checkRate)���� �� �ð��� ������ ����
        {
            lastCheckTime = Time.time;              // ������ üũ �ð� ����


            // ȭ�� �߾ӿ��� ���̸� ��� �浹 �˻�
            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            // ���̰� Ư�� ������Ʈ�� �浹�ϸ� ����
            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                // ���ο� ������Ʈ�� �������� ��� ������Ʈ
                if (hit.collider.gameObject != curInteractGameObject)
                {
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();
                    SetPromptText(); // ������Ʈ �ؽ�Ʈ ����
                }
            }

            // �浹�ϴ� ������Ʈ�� ���ٸ� �ʱ�ȭ
            else
            {
                curInteractGameObject = null;
                curInteractable = null;
                promptText.gameObject.SetActive(false); // ������Ʈ �ؽ�Ʈ ����
            }
        }
    }

    // ��ȣ�ۿ� ������Ʈ �ؽ�Ʈ ����
    private void SetPromptText()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = curInteractable.GetInteractPrompt(); // �������̽����� ������Ʈ �޽��� ������
    }

    // ��ȣ�ۿ� �Է��� ������ �� ����
    public void OnInteractInput(InputAction.CallbackContext context)
    {
        // �Է��� ���۵Ǿ��� ���� ��ȣ�ۿ� ������ ������Ʈ�� ���� ���
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            curInteractable.OnInteract(); // ��ȣ�ۿ� ����
            curInteractGameObject = null;
            curInteractable = null;
            promptText.gameObject.SetActive(false); // ������Ʈ �ؽ�Ʈ ����
        }
    }
}
