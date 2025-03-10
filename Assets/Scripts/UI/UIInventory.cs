using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots; // 인벤토리 슬롯 배열

    public GameObject inventoryWindow; // 인벤토리 UI 창
    public Transform slotPanel; // 슬롯들을 담고 있는 패널
    public Transform dropPosition; // 아이템을 버릴 위치

    [Header("Selected Item")]
    public TextMeshProUGUI selectedItemName; // 선택한 아이템의 이름
    public TextMeshProUGUI selectedItemDescription; // 선택한 아이템의 설명
    public TextMeshProUGUI selectedItemStatName; // 선택한 아이템의 능력치 이름
    public TextMeshProUGUI selectedItemStatValue; // 선택한 아이템의 능력치 값
    public GameObject useButton; // 사용 버튼
    public GameObject equipButton; // 장착 버튼
    public GameObject unEquipButton; // 장착 해제 버튼
    public GameObject dropButton; // 버리기 버튼

    private PlayerController controller; // 플레이어 컨트롤러
    private PlayerCondition condition; // 플레이어 상태 정보

    ItemData selectedItem; // 선택된 아이템 데이터
    int selectedItemIndex = 0; // 선택된 아이템의 슬롯 인덱스

    int curEquipIndex; // 현재 장착된 아이템의 인덱스를 저장하는 변수

    // Start는 게임이 시작될 때 한 번 실행되는 함수
    void Start()
    {
        controller = CharacterManager.Instance.Player.controller; // 플레이어 컨트롤러 가져오기
        condition = CharacterManager.Instance.Player.condition; // 플레이어 상태 가져오기
        dropPosition = CharacterManager.Instance.Player.dropPosition; // 아이템을 버릴 위치 가져오기

        controller.inventory += Toggle; // 인벤토리 창 열기/닫기 이벤트 등록
        CharacterManager.Instance.Player.addItem += AddItem; // 아이템 추가 이벤트 등록

        inventoryWindow.SetActive(false); // 게임 시작 시 인벤토리 창 비활성화
        slots = new ItemSlot[slotPanel.childCount]; // 슬롯 배열 크기 설정

        // 슬롯 패널의 자식 개수만큼 슬롯 배열 초기화
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>(); // 슬롯 가져오기
            slots[i].index = i; // 슬롯의 인덱스 설정
            slots[i].inventory = this; // 현재 인벤토리 설정
        }
        ClearSelectedItemWindow(); // 선택된 아이템 정보 초기화
    }

    // Update는 매 프레임마다 호출되지만 현재 사용되지 않음
    void Update()
    {

    }

    // 선택한 아이템 창을 초기화하는 함수
    void ClearSelectedItemWindow()
    {
        selectedItemName.text = string.Empty; // 아이템 이름 초기화
        selectedItemDescription.text = string.Empty; // 아이템 설명 초기화
        selectedItemStatName.text = string.Empty; // 아이템 능력치 이름 초기화
        selectedItemStatValue.text = string.Empty; // 아이템 능력치 값 초기화

        useButton.SetActive(false); // 사용 버튼 비활성화
        equipButton.SetActive(false); // 장착 버튼 비활성화
        unEquipButton.SetActive(false); // 장착 해제 버튼 비활성화
        dropButton.SetActive(false); // 버리기 버튼 비활성화
    }

    // 인벤토리 창을 열거나 닫는 함수
    public void Toggle()
    {
        if (IsOpen()) // 현재 인벤토리 창이 열려 있다면
        {
            inventoryWindow.SetActive(false); // 인벤토리 창 닫기
        }
        else // 닫혀 있다면
        {
            inventoryWindow.SetActive(true); // 인벤토리 창 열기
        }
    }

    // 인벤토리 창이 열려 있는지 확인하는 함수
    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy; // 인벤토리 창이 활성화되어 있는지 반환
    }

    // 아이템을 인벤토리에 추가하는 함수
    void AddItem()
    {
        ItemData data = CharacterManager.Instance.Player.itemData; // 추가할 아이템 데이터 가져오기
        if (data.canStack) // 아이템이 중첩 가능한 경우
        {
            ItemSlot slot = GetItemStack(data); // 동일한 아이템이 있는 슬롯 찾기
            if (slot != null) // 같은 아이템이 있다면
            {
                slot.quantity++; // 아이템 개수 증가
                UpdateUI(); // UI 업데이트
                CharacterManager.Instance.Player.itemData = null; // 아이템 데이터 초기화
                return;
            }
        }

        // 비어있는 슬롯 가져오기
        ItemSlot emptySlot = GetEmptySlot();

        // 비어있는 슬롯이 있다면
        if (emptySlot != null)
        {
            emptySlot.item = data; // 슬롯에 아이템 저장
            emptySlot.quantity = 1; // 개수 1로 설정
            UpdateUI(); // UI 업데이트
            CharacterManager.Instance.Player.itemData = null; // 아이템 데이터 초기화
            return;
        }

        // 빈 슬롯이 없으면 아이템을 바닥에 버림
        ThrowItem(data);
        CharacterManager.Instance.Player.itemData = null; // 아이템 데이터 초기화
    }
    public void UpdateUI()
    {
        // 모든 슬롯을 순회하면서 UI를 업데이트하는 함수
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null) // 슬롯에 아이템이 있다면
            {
                slots[i].Set(); // 슬롯 UI를 업데이트
            }
            else // 슬롯이 비어있다면
            {
                slots[i].Clear(); // 슬롯 UI를 초기화
            }
        }
    }

    ItemSlot GetItemStack(ItemData data)
    {
        // 같은 종류의 아이템이 이미 존재하면서 최대 개수를 초과하지 않은 슬롯을 찾는 함수
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i]; // 해당 슬롯 반환
            }
        }
        return null; // 없으면 null 반환
    }

    ItemSlot GetEmptySlot()
    {
        // 비어있는 슬롯을 찾는 함수
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
            {
                return slots[i]; // 빈 슬롯 반환
            }
        }
        return null; // 없으면 null 반환
    }

    void ThrowItem(ItemData data)
    {
        // 아이템을 버릴 때 해당 아이템의 프리팹을 dropPosition 위치에 생성
        Instantiate(data.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }

    // ItemSlot 스크립트 먼저 수정
    public void SelectItem(int index)
    {
        // 선택한 슬롯의 아이템이 없으면 함수 종료
        if (slots[index].item == null) return;

        selectedItem = slots[index].item; // 선택한 아이템 저장
        selectedItemIndex = index; // 선택한 아이템의 슬롯 인덱스 저장

        selectedItemName.text = selectedItem.displayName; // 아이템 이름 표시
        selectedItemDescription.text = selectedItem.description; // 아이템 설명 표시

        selectedItemStatName.text = string.Empty; // 능력치 이름 초기화
        selectedItemStatValue.text = string.Empty; // 능력치 값 초기화

        // 아이템이 소비형(consumable) 아이템일 경우 능력치 표시
        for (int i = 0; i < selectedItem.consumables.Length; i++)
        {
            selectedItemStatName.text += selectedItem.consumables[i].type.ToString() + "\n"; // 능력치 타입 추가
            selectedItemStatValue.text += selectedItem.consumables[i].value.ToString() + "\n"; // 능력치 값 추가
        }

        // 아이템 타입에 따라 버튼 활성화 설정
        useButton.SetActive(selectedItem.type == ItemType.Consumable); // 소비형 아이템이면 사용 버튼 활성화
        equipButton.SetActive(selectedItem.type == ItemType.Equipable && !slots[index].equipped); // 장착 가능한 아이템이며 아직 장착하지 않은 경우 장착 버튼 활성화
        unEquipButton.SetActive(selectedItem.type == ItemType.Equipable && slots[index].equipped); // 장착 가능한 아이템이며 이미 장착한 경우 장착 해제 버튼 활성화
        dropButton.SetActive(true); // 버리기 버튼 활성화
    }

    public void OnUseButton()
    {
        // 선택한 아이템이 소비형 아이템인지 확인
        if (selectedItem.type == ItemType.Consumable)
        {
            // 소비형 아이템의 능력치를 적용
            for (int i = 0; i < selectedItem.consumables.Length; i++)
            {
                switch (selectedItem.consumables[i].type)
                {
                    case ConsumableType.Health:
                        condition.Heal(selectedItem.consumables[i].value); // 체력 회복
                        break;
                    case ConsumableType.Hunger:
                        condition.Eat(selectedItem.consumables[i].value); // 배고픔 해소
                        break;
                }
            }
            RemoveSelctedItem(); // 아이템 사용 후 제거
        }
    }

    public void OnDropButton()
    {
        // 아이템을 버리는 함수
        ThrowItem(selectedItem); // 아이템을 현재 위치에 버림
        RemoveSelctedItem(); // 인벤토리에서 아이템 제거
    }

    void RemoveSelctedItem()
    {
        // 선택된 아이템 개수 감소
        slots[selectedItemIndex].quantity--;

        // 만약 아이템 개수가 0 이하라면 슬롯을 비움
        if (slots[selectedItemIndex].quantity <= 0)
        {
            selectedItem = null; // 선택된 아이템 초기화
            slots[selectedItemIndex].item = null; // 슬롯의 아이템 제거
            selectedItemIndex = -1; // 선택된 인덱스 초기화
            ClearSelectedItemWindow(); // 선택된 아이템 정보 UI 초기화
        }
    }

    // 장착 버튼을 눌렀을 때 호출되는 함수
    public void OnEquipButton()
    {
        // 현재 장착된 아이템이 있으면 장착 해제
        if (slots[curEquipIndex].equipped)
        {
            UnEquip(curEquipIndex);
        }

        // 새로운 아이템을 장착
        slots[selectedItemIndex].equipped = true;                        // 선택된 아이템을 장착 상태로 변경
        curEquipIndex = selectedItemIndex;                              // 현재 장착된 아이템의 인덱스를 업데이트
        CharacterManager.Instance.Player.equip.EquipNew(selectedItem); // 캐릭터의 장착 정보 업데이트
        UpdateUI(); // UI 갱신

        SelectItem(selectedItemIndex); // 선택된 아이템 UI 업데이트
    }

    // 특정 아이템을 장착 해제하는 함수
    void UnEquip(int index)
    {
        slots[index].equipped = false; // 해당 아이템의 장착 상태를 해제
        CharacterManager.Instance.Player.equip.UnEquip(); // 캐릭터의 장착 정보에서 제거
        UpdateUI(); // UI 갱신

        // 선택된 아이템이 해제된 아이템과 동일하면 다시 선택 UI 갱신
        if (selectedItemIndex == index)
        {
            SelectItem(selectedItemIndex);
        }
    }

    // 장착 해제 버튼을 눌렀을 때 호출되는 함수
    public void OnUnEquipButton()
    {
        UnEquip(selectedItemIndex); // 현재 선택된 아이템을 장착 해제
    }

    // 특정 아이템이 인벤토리에 지정된 개수 이상 있는지 확인하는 함수
    public bool HasItem(ItemData item, int quantity)
    {
        return false; // 현재는 항상 false 반환 (추후 구현 필요)
    }
}
