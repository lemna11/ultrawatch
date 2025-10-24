public partial class Hud : Control {
    [Export]
    public Player player;

    [Export]
    public Label max_health;

    [Export]
    public Label cur_health;

    public override void _Ready() {
        player.update_hud += OnHudUpdateRequested;
    }

    public override void _ExitTree() {
        if (player is not null) {
            player.update_hud -= OnHudUpdateRequested;
        }
    }


    public void OnHudUpdateRequested() {
        max_health.Text = player.max_health.ToString();
        cur_health.Text = player.cur_health.ToString();
    }

}
