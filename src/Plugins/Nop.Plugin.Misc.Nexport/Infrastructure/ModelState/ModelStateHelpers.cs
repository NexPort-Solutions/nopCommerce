using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nop.Plugin.Misc.Nexport.Infrastructure.ModelState;

public static class ModelStateHelpers
{
    /// <summary>
    /// Serialize model state
    /// </summary>
    /// <param name="modelState">The model state</param>
    /// <returns>Serialized object of the model state</returns>
    public static string SerializeModelState(ModelStateDictionary modelState)
    {
        var errorList = modelState
            .Select(keyValuePair => new ModelStateTransferValue(
                keyValuePair.Key,
                keyValuePair.Value?.AttemptedValue,
                keyValuePair.Value?.RawValue,
                keyValuePair.Value?.Errors.Select(error => error.ErrorMessage).ToList() ?? new()));
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
        if (errorList is null)
        {
            return modelState;
        }
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
