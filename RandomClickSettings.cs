using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using System.Windows.Forms;

namespace RandomClick;

public class RandomClickSettings : ISettings
{
    //Mandatory setting to allow enabling/disabling your plugin
    public ToggleNode Enable { get; set; } = new ToggleNode(false);


    public ToggleNode Work { get; set; } = new ToggleNode(false);

    [Menu("OnOff Key", "")]
    public HotkeyNodeV2 OnOff { get; set; } = new(Keys.End);

    [Menu("Action Delay")]
    public RangeNode<int> ActionDelay { get; set; } = new RangeNode<int>(50, 0, 1000);

    [Menu("Off if full inventory")]
    public ToggleNode InventoryOff { get; set; } = new ToggleNode(false);

    [Menu("Off Count")]
    public RangeNode<int> OffCount { get; set; } = new RangeNode<int>(50, 0, 60);
}