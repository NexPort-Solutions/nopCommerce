using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nop.Plugin.Misc.Nexport.Infrastructure.ModelState
{
    public class ModelStateHelpers
    {
        /// <summary>
        /// Serialize model state
        /// </summary>
        /// <param name="modelState">The model state</param>
        /// <returns>Serialized object of the model state</returns>
        public static string SerializeModelState(ModelStateDictionary modelState)
        {
            var errorList = modelState
                .Select(kvp => new ModelStateTransferValue
                {
                    Key = kvp.Key,
                    AttemptedValue = kvp.Value.AttemptedValue,
                    RawValue = kvp.Value.RawValue,
                    ErrorMessages = kvp.Value.Errors.Select(err => err.ErrorMessage).ToList(),
                });

            return JsonConvert.SerializeObject(errorList);
        }

        /// <summary>
        /// Deserialize model state
        /// </summary>
        /// <param name="serializedErrorList">The serialized error list</param>
        /// <returns>Model state dictionary that contains keys and values the previous serialized model state</returns>
        public static ModelStateDictionary DeserializeModelState(string serializedErrorList)
        {
            var errorList = JsonConvert.DeserializeObject<List<ModelStateTransferValue>>(serializedErrorList);
            var modelState = new ModelStateDictionary();

            foreach (var item in errorList)
            {
                var value = item.RawValue;

                // Check if the item is an array
                if (value is JArray array)
                {
                    value = array.ToObject<string[]>();
                }

                modelState.SetModelValue(item.Key, value, item.AttemptedValue);

                foreach (var error in item.ErrorMessages)
                {
                    modelState.AddModelError(item.Key, error);
                }
            }

            return modelState;
        }
    }
}
