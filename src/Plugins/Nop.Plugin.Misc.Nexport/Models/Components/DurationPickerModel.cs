namespace Nop.Plugin.Misc.Nexport.Models.Components;

public class DurationPickerModel
{
    public DurationPickerModel(string containerId, string name)
    {
        ContainerId = containerId;
        Name = name;
    }

    public DurationPickerModel(string containerId, string name, string value)
    {
        ContainerId = containerId;
        Name = name;
        Value = value;
    }

    public string ContainerId { get; set; }
    public string Name { get; set; }
    public string? Value { set; get; }
}
