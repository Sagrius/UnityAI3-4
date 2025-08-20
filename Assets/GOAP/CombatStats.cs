using UnityEngine;

public class CombatStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public int attackPower = 10;
    [Range(0, 100)]
    public int healingThreshold = 50;
    public enum AgentType {Villager,Falcon,Mage,Enemy};
    public AgentType agentType = AgentType.Villager;


    public bool IsUnderAttack { get; private set; }
    private Coroutine underAttackCoroutine;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage, has {currentHealth} health remaining.");

        IsUnderAttack = true;
        if (underAttackCoroutine != null) StopCoroutine(underAttackCoroutine);
        underAttackCoroutine = StartCoroutine(UnderAttackTimer());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator UnderAttackTimer()
    {
        yield return new WaitForSeconds(3f); // Agent feels "in combat" for 3 seconds after being hit
        IsUnderAttack = false;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} healed for {amount}, has {currentHealth} health.");
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
