using Weapons;

namespace UI;

public partial class Hud : Control {
    [Export]
    public Player player;

    [Export]
    public WeaponManager weapon_manager;

    [Export]
    public Label max_health;

    [Export]
    public Label cur_health;

    [Export]
    public Label left_max_ammo;

    [Export]
    public Label left_cur_ammo;

    [Export]
    public Label right_max_ammo;

    [Export]
    public Label right_cur_ammo;

    public override void _Ready() {
        player.update_hud += OnPlayerHudUpdateRequested;
        weapon_manager.update_hud += OnWeaponHudUpdateRequested;
    }

    public override void _ExitTree() {
        if (player is not null) {
            player.update_hud -= OnPlayerHudUpdateRequested;
        }
    }


    private void OnPlayerHudUpdateRequested() {
        max_health.Text = player.max_health.ToString();
        cur_health.Text = player.cur_health.ToString();
    }

    private void OnWeaponHudUpdateRequested() {
        if (weapon_manager.left_current_weapon is null || weapon_manager.left_current_weapon.magazine_size == 1) {
            left_max_ammo.GetParent<Control>().Visible = false;
        } else {
            left_max_ammo.GetParent<Control>().Visible = true;
            left_max_ammo.Text = weapon_manager.left_current_weapon.magazine_size.ToString();
            left_cur_ammo.Text = weapon_manager.left_current_weapon.current_ammo.ToString();
        }

        if (weapon_manager.right_current_weapon is null || weapon_manager.right_current_weapon.magazine_size == 1) {
            right_max_ammo.GetParent<Control>().Visible = false;
        } else {
            right_max_ammo.GetParent<Control>().Visible = true;
            right_max_ammo.Text = weapon_manager.right_current_weapon.magazine_size.ToString();
            right_cur_ammo.Text = weapon_manager.right_current_weapon.current_ammo.ToString();
        }
    }
}
