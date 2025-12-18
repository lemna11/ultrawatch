using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace UI;

public partial class LoadoutSelectionMenu : Control {
    public Action<string, WeaponManager.EquipSide> OnWeaponSelected;

    public Action<string> OnLegSelected;

    [Export]
    public VBoxContainer left_arm_container;

    [Export]
    public VBoxContainer right_arm_container;

    [Export]
    public VBoxContainer legs_container;

    private readonly List<string> available_weapons = [];
    private readonly List<string> available_legs = [];

    public override void _Ready() {
        ScanForWeapons();
        ScanForLegs();
        PopulateWeaponButtons();
        PopulateLegButtons();
        Visible = false;
    }

    private void ScanForWeapons() {
        var dir = DirAccess.Open("res://Weapons");
        if (dir != null) {
            foreach (var dirName in dir.GetDirectories()){
                if (!dirName.StartsWith('.')) {
                    var weaponPath = $"res://Weapons/{dirName}/{dirName}.tres";
                    if (ResourceLoader.Exists(weaponPath)) {
                        available_weapons.Add(weaponPath);
                    }
                }
            }
        } else {
            GD.PrintErr("Could not populate weapons UI");
            throw new Exception();
        }
    }

    private void ScanForLegs() {
        // this will be insufficient in the future; when legs have their own resources
        // when that happens we should find a better way to find all leg abilities (and weapons)
        var legAbilityType = typeof(Legs.ILegAbility);
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes()) {
            if (type.IsClass && !type.IsAbstract && type.IsAssignableTo(legAbilityType)) {
                available_legs.Add(type.Name);
            }
        }
    }

    private void PopulateWeaponButtons() {
        foreach (var weaponPath in available_weapons) {
            var weaponName = GetWeaponClassName(weaponPath);
            var button = new Button {
                Text = weaponName,
                CustomMinimumSize = new Vector2(150, 40)
            };
            button.Pressed += () => OnWeaponSelected(weaponPath, WeaponManager.EquipSide.Left);
            left_arm_container.AddChild(button);
        }

        foreach (var weaponPath in available_weapons) {
            var weaponName = GetWeaponClassName(weaponPath);
            var button = new Button {
                Text = weaponName,
                CustomMinimumSize = new Vector2(150, 40)
            };
            button.Pressed += () => OnWeaponSelected(weaponPath, WeaponManager.EquipSide.Right);
            right_arm_container.AddChild(button);
        }
    }

    private void PopulateLegButtons() {
        foreach (var legClassName in available_legs) {
            var button = new Button {
                Text = FormatLegName(legClassName),
                CustomMinimumSize = new Vector2(150, 40)
            };
            button.Pressed += () => OnLegSelected(legClassName);
            legs_container.AddChild(button);
        }
    }

    private static string FormatLegName(string className) {
        return CapitalLetter().Replace(className, " $1").Trim();
    }

    private string GetWeaponClassName(string weaponPath) {
        var parts = weaponPath.Split('/');
        if (parts.Length >= 3) {
            return parts[^2];
        }
        return "Unknown";
    }

    public void ToggleMenu() {
        Visible = !Visible;

        if (Visible) {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        } else {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape && Visible) {
            ToggleMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    [GeneratedRegex("([A-Z])")]
    private static partial Regex CapitalLetter();
}
