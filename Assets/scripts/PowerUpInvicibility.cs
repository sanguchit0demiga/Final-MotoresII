public class InvincibilityCommand : ICommand
{
    private PlayerController player;
    private float duration;

    public InvincibilityCommand(PlayerController player, float duration)
    {
        this.player = player;
        this.duration = duration;
    }

    public void Execute()
    {
        player.ActivateInvincibility(duration);
    }
}
