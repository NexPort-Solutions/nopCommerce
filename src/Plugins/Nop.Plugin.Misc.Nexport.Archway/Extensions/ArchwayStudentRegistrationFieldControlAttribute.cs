using System;

namespace Nop.Plugin.Misc.Nexport.Archway.Extensions
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class ArchwayStudentRegistrationFieldControlAttribute : Attribute
    {
        public ArchwayStudentRegistrationFieldControlAttribute(string controlName)
        {
            ControlName = controlName;
        }

        public string ControlName { get; set; }
    }
}
