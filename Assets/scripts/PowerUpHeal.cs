public class HealCommand : ICommand
{
    private PlayerController player;
    private float healAmount;

    public HealCommand(PlayerController player, float healAmount)
    {
        this.player = player;
        this.healAmount = healAmount;
    }

    public void Execute()
    {
        player.Heal(healAmount);
    }
}
