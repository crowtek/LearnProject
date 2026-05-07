using UnityEngine;

public class BattleTrigger : MonoBehaviour
{
    [SerializeField] private BattleEntityData entityData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerInputHandler>().SetInputActive(false);

            BattleManager.Instance.StartBattle(entityData);
        }
    }
}
