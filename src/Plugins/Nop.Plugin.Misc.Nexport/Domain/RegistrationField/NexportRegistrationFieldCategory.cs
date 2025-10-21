using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

public class NexportRegistrationFieldCategory: BaseEntity
{
    public string Title { get; set; }

    public string Description { get; set; }

    public int DisplayOrder { get; set; }
}

public class NexportRegistrationFieldCategoryComparer : IEqualityComparer<NexportRegistrationFieldCategory>
{
    public bool Equals(NexportRegistrationFieldCategory x, NexportRegistrationFieldCategory y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null)
            return false;
        if (y is null)
            return false;
        if (x.GetType() != y.GetType())
            return false;
        return x.Id == y.Id;
    }

    public int GetHashCode(NexportRegistrationFieldCategory obj)
    {
        return obj.Id;
    }
}