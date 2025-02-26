namespace Nop.Plugin.Misc.Nexport.Models.Components;

public class NexportDurationPickerModel(string containerId, string name, string value)
{
    public NexportDurationPickerModel(string containerId, string name) : this(containerId, name, null)
    {
    }

    public string ContainerId { get; set; } = containerId;

    public string Name { get; set; } = name;

    public string Value { set; get; } = value;
}