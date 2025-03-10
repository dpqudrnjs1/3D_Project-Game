using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots; // �κ��丮 ���� �迭

    public GameObject inventoryWindow; // �κ��丮 UI â
    public Transform slotPanel; // ���Ե��� ��� �ִ� �г�
    public Transform dropPosition; // �������� ���� ��ġ

    [Header("Selected Item")]
    public TextMeshProUGUI selectedItemName; // ������ �������� �̸�
    public TextMeshProUGUI selectedItemDescription; // ������ �������� ����
    public TextMeshProUGUI selectedItemStatName; // ������ �������� �ɷ�ġ �̸�
    public TextMeshProUGUI selectedItemStatValue; // ������ �������� �ɷ�ġ ��
    public GameObject useButton; // ��� ��ư
    public GameObject equipButton; // ���� ��ư
    public GameObject unEquipButton; // ���� ���� ��ư
    public GameObject dropButton; // ������ ��ư

    private PlayerController controller; // �÷��̾� ��Ʈ�ѷ�
    private PlayerCondition condition; // �÷��̾� ���� ����

    ItemData selectedItem; // ���õ� ������ ������
    int selectedItemIndex = 0; // ���õ� �������� ���� �ε���

    int curEquipIndex; // ���� ������ �������� �ε����� �����ϴ� ����

    // Start�� ������ ���۵� �� �� �� ����Ǵ� �Լ�
    void Start()
    {
        controller = CharacterManager.Instance.Player.controller; // �÷��̾� ��Ʈ�ѷ� ��������
        condition = CharacterManager.Instance.Player.condition; // �÷��̾� ���� ��������
        dropPosition = CharacterManager.Instance.Player.dropPosition; // �������� ���� ��ġ ��������

        controller.inventory += Toggle; // �κ��丮 â ����/�ݱ� �̺�Ʈ ���
        CharacterManager.Instance.Player.addItem += AddItem; // ������ �߰� �̺�Ʈ ���

        inventoryWindow.SetActive(false); // ���� ���� �� �κ��丮 â ��Ȱ��ȭ
        slots = new ItemSlot[slotPanel.childCount]; // ���� �迭 ũ�� ����

        // ���� �г��� �ڽ� ������ŭ ���� �迭 �ʱ�ȭ
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>(); // ���� ��������
            slots[i].index = i; // ������ �ε��� ����
            slots[i].inventory = this; // ���� �κ��丮 ����
        }
        ClearSelectedItemWindow(); // ���õ� ������ ���� �ʱ�ȭ
    }

    // Update�� �� �����Ӹ��� ȣ������� ���� ������ ����
    void Update()
    {

    }

    // ������ ������ â�� �ʱ�ȭ�ϴ� �Լ�
    void ClearSelectedItemWindow()
    {
        selectedItemName.text = string.Empty; // ������ �̸� �ʱ�ȭ
        selectedItemDescription.text = string.Empty; // ������ ���� �ʱ�ȭ
        selectedItemStatName.text = string.Empty; // ������ �ɷ�ġ �̸� �ʱ�ȭ
        selectedItemStatValue.text = string.Empty; // ������ �ɷ�ġ �� �ʱ�ȭ

        useButton.SetActive(false); // ��� ��ư ��Ȱ��ȭ
        equipButton.SetActive(false); // ���� ��ư ��Ȱ��ȭ
        unEquipButton.SetActive(false); // ���� ���� ��ư ��Ȱ��ȭ
        dropButton.SetActive(false); // ������ ��ư ��Ȱ��ȭ
    }

    // �κ��丮 â�� ���ų� �ݴ� �Լ�
    public void Toggle()
    {
        if (IsOpen()) // ���� �κ��丮 â�� ���� �ִٸ�
        {
            inventoryWindow.SetActive(false); // �κ��丮 â �ݱ�
        }
        else // ���� �ִٸ�
        {
            inventoryWindow.SetActive(true); // �κ��丮 â ����
        }
    }

    // �κ��丮 â�� ���� �ִ��� Ȯ���ϴ� �Լ�
    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy; // �κ��丮 â�� Ȱ��ȭ�Ǿ� �ִ��� ��ȯ
    }

    // �������� �κ��丮�� �߰��ϴ� �Լ�
    void AddItem()
    {
        ItemData data = CharacterManager.Instance.Player.itemData; // �߰��� ������ ������ ��������
        if (data.canStack) // �������� ��ø ������ ���
        {
            ItemSlot slot = GetItemStack(data); // ������ �������� �ִ� ���� ã��
            if (slot != null) // ���� �������� �ִٸ�
            {
                slot.quantity++; // ������ ���� ����
                UpdateUI(); // UI ������Ʈ
                CharacterManager.Instance.Player.itemData = null; // ������ ������ �ʱ�ȭ
                return;
            }
        }

        // ����ִ� ���� ��������
        ItemSlot emptySlot = GetEmptySlot();

        // ����ִ� ������ �ִٸ�
        if (emptySlot != null)
        {
            emptySlot.item = data; // ���Կ� ������ ����
            emptySlot.quantity = 1; // ���� 1�� ����
            UpdateUI(); // UI ������Ʈ
            CharacterManager.Instance.Player.itemData = null; // ������ ������ �ʱ�ȭ
            return;
        }

        // �� ������ ������ �������� �ٴڿ� ����
        ThrowItem(data);
        CharacterManager.Instance.Player.itemData = null; // ������ ������ �ʱ�ȭ
    }
    public void UpdateUI()
    {
        // ��� ������ ��ȸ�ϸ鼭 UI�� ������Ʈ�ϴ� �Լ�
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null) // ���Կ� �������� �ִٸ�
            {
                slots[i].Set(); // ���� UI�� ������Ʈ
            }
            else // ������ ����ִٸ�
            {
                slots[i].Clear(); // ���� UI�� �ʱ�ȭ
            }
        }
    }

    ItemSlot GetItemStack(ItemData data)
    {
        // ���� ������ �������� �̹� �����ϸ鼭 �ִ� ������ �ʰ����� ���� ������ ã�� �Լ�
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i]; // �ش� ���� ��ȯ
            }
        }
        return null; // ������ null ��ȯ
    }

    ItemSlot GetEmptySlot()
    {
        // ����ִ� ������ ã�� �Լ�
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
            {
                return slots[i]; // �� ���� ��ȯ
            }
        }
        return null; // ������ null ��ȯ
    }

    void ThrowItem(ItemData data)
    {
        // �������� ���� �� �ش� �������� �������� dropPosition ��ġ�� ����
        Instantiate(data.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }

    // ItemSlot ��ũ��Ʈ ���� ����
    public void SelectItem(int index)
    {
        // ������ ������ �������� ������ �Լ� ����
        if (slots[index].item == null) return;

        selectedItem = slots[index].item; // ������ ������ ����
        selectedItemIndex = index; // ������ �������� ���� �ε��� ����

        selectedItemName.text = selectedItem.displayName; // ������ �̸� ǥ��
        selectedItemDescription.text = selectedItem.description; // ������ ���� ǥ��

        selectedItemStatName.text = string.Empty; // �ɷ�ġ �̸� �ʱ�ȭ
        selectedItemStatValue.text = string.Empty; // �ɷ�ġ �� �ʱ�ȭ

        // �������� �Һ���(consumable) �������� ��� �ɷ�ġ ǥ��
        for (int i = 0; i < selectedItem.consumables.Length; i++)
        {
            selectedItemStatName.text += selectedItem.consumables[i].type.ToString() + "\n"; // �ɷ�ġ Ÿ�� �߰�
            selectedItemStatValue.text += selectedItem.consumables[i].value.ToString() + "\n"; // �ɷ�ġ �� �߰�
        }

        // ������ Ÿ�Կ� ���� ��ư Ȱ��ȭ ����
        useButton.SetActive(selectedItem.type == ItemType.Consumable); // �Һ��� �������̸� ��� ��ư Ȱ��ȭ
        equipButton.SetActive(selectedItem.type == ItemType.Equipable && !slots[index].equipped); // ���� ������ �������̸� ���� �������� ���� ��� ���� ��ư Ȱ��ȭ
        unEquipButton.SetActive(selectedItem.type == ItemType.Equipable && slots[index].equipped); // ���� ������ �������̸� �̹� ������ ��� ���� ���� ��ư Ȱ��ȭ
        dropButton.SetActive(true); // ������ ��ư Ȱ��ȭ
    }

    public void OnUseButton()
    {
        // ������ �������� �Һ��� ���������� Ȯ��
        if (selectedItem.type == ItemType.Consumable)
        {
            // �Һ��� �������� �ɷ�ġ�� ����
            for (int i = 0; i < selectedItem.consumables.Length; i++)
            {
                switch (selectedItem.consumables[i].type)
                {
                    case ConsumableType.Health:
                        condition.Heal(selectedItem.consumables[i].value); // ü�� ȸ��
                        break;
                    case ConsumableType.Hunger:
                        condition.Eat(selectedItem.consumables[i].value); // ����� �ؼ�
                        break;
                }
            }
            RemoveSelctedItem(); // ������ ��� �� ����
        }
    }

    public void OnDropButton()
    {
        // �������� ������ �Լ�
        ThrowItem(selectedItem); // �������� ���� ��ġ�� ����
        RemoveSelctedItem(); // �κ��丮���� ������ ����
    }

    void RemoveSelctedItem()
    {
        // ���õ� ������ ���� ����
        slots[selectedItemIndex].quantity--;

        // ���� ������ ������ 0 ���϶�� ������ ���
        if (slots[selectedItemIndex].quantity <= 0)
        {
            selectedItem = null; // ���õ� ������ �ʱ�ȭ
            slots[selectedItemIndex].item = null; // ������ ������ ����
            selectedItemIndex = -1; // ���õ� �ε��� �ʱ�ȭ
            ClearSelectedItemWindow(); // ���õ� ������ ���� UI �ʱ�ȭ
        }
    }

    // ���� ��ư�� ������ �� ȣ��Ǵ� �Լ�
    public void OnEquipButton()
    {
        // ���� ������ �������� ������ ���� ����
        if (slots[curEquipIndex].equipped)
        {
            UnEquip(curEquipIndex);
        }

        // ���ο� �������� ����
        slots[selectedItemIndex].equipped = true;                        // ���õ� �������� ���� ���·� ����
        curEquipIndex = selectedItemIndex;                              // ���� ������ �������� �ε����� ������Ʈ
        CharacterManager.Instance.Player.equip.EquipNew(selectedItem); // ĳ������ ���� ���� ������Ʈ
        UpdateUI(); // UI ����

        SelectItem(selectedItemIndex); // ���õ� ������ UI ������Ʈ
    }

    // Ư�� �������� ���� �����ϴ� �Լ�
    void UnEquip(int index)
    {
        slots[index].equipped = false; // �ش� �������� ���� ���¸� ����
        CharacterManager.Instance.Player.equip.UnEquip(); // ĳ������ ���� �������� ����
        UpdateUI(); // UI ����

        // ���õ� �������� ������ �����۰� �����ϸ� �ٽ� ���� UI ����
        if (selectedItemIndex == index)
        {
            SelectItem(selectedItemIndex);
        }
    }

    // ���� ���� ��ư�� ������ �� ȣ��Ǵ� �Լ�
    public void OnUnEquipButton()
    {
        UnEquip(selectedItemIndex); // ���� ���õ� �������� ���� ����
    }

    // Ư�� �������� �κ��丮�� ������ ���� �̻� �ִ��� Ȯ���ϴ� �Լ�
    public bool HasItem(ItemData item, int quantity)
    {
        return false; // ����� �׻� false ��ȯ (���� ���� �ʿ�)
    }
}
