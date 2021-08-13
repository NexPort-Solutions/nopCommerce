using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer
{
    public class NexportCustomerRegistrationFieldAnswersEditModel : BaseNopModel
    {
        public NexportCustomerRegistrationFieldAnswersEditModel()
        {
            Options = new List<NexportRegistrationFieldOption>();
        }

        public NexportRegistrationFieldModel RegistrationField { get; set; }

        public IList<NexportRegistrationFieldOption> Options { get; set; }

        public IList<NexportRegistrationFieldAnswer> Answers { get; set; }
    }

    [Serializable]
    public class EditRegistrationFieldAnswerRequestModel {
        public int FieldId { get; set; }

        public bool? AllowMultipleSelection { get; set; }

        public string AnswerValue { get; set; }

        public IList<int> PreviousAnswers { get; set; }

        public IList<int> AnswerFieldOptions { get; set; }

        public IFormCollection FormCollection { get; set; }
    }
}