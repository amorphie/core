using System.Text.Json;
using System.Text.Json.Nodes;
using amorphie.core.Extensions;
using amorphie.core.Helper;
using NJsonSchema;
namespace amorphie.service.Filters;
public class FilterHelper
{
    public static async Task<dynamic> FilterResponseAsync(JsonElement body, NJsonSchema.JsonSchema schema)
    {
        if (body.TryConvertToJsonObject(out JsonObject? pairs) && pairs != null)
        {

            var tObjectKeysLower = pairs.Select(x => x.Key);
            var keyToBeStayed = schema.Properties.Where(p => tObjectKeysLower.Contains(p.Key))
                .Select(p => p.Key)
                .ToList();

            var tNewObject = pairs.Where(p => keyToBeStayed.Contains(p.Key)).ToDictionary();
            var ser = AmorphieJsonSerializer.Serialize(tNewObject);
            var des = AmorphieJsonSerializer.Deserialize<object>(ser);

            return des ?? body;
        }
        return body;
    }
    public static (JsonObject, bool) FilterAndEncrypt(JsonObject body, NJsonSchema.JsonSchema schema, string instanceId)
    {
        bool hasAnyEncryption = false;
        if (body != null)
        {
            var filterResult = FilterAndEncryptJsonObject(body, schema, instanceId);

            // var ser = WfJsonSerializer.Serialize(filterResult.Item1);
            // var des = WfJsonSerializer.Deserialize<object>(ser);

            return (filterResult.Item1 ?? body, filterResult.Item2);
        }
        return (body, hasAnyEncryption);
    }

    public static (JsonObject, bool) FilterAndEncryptJsonObject(JsonObject data, NJsonSchema.JsonSchema schema, string instanceId)
    {
        var dataKeys = data.Where(p => p.Value != null).Select(p => p.Key).ToList();
        if (dataKeys is not null)
        {
            var keyToBeStayed = schema.Properties.Where(p => dataKeys.Contains(p.Key))
                .Select(p => p.Key)
                .ToList();

            var tNewObject = data.Where(p => keyToBeStayed.Contains(p.Key)).ToDictionary();
            if (tNewObject is not null)
            {
                var encResult = EncryptDict(tNewObject, schema.ActualProperties, instanceId);
                if (encResult.Item1 != null)
                {
                    return (DictionaryToJsonObject(encResult.Item1) ?? data, encResult.Item2);
                }
                return (data, encResult.Item2);
            }

        }
        //second param indicates if there is encryption or not
        return (data, false);
    }
    /// <summary>
    /// Item1 encrypted => data, Item2 => shows data is encrypted
    /// </summary>
    /// <param name="data"></param>
    /// <param name="actualProperties"></param>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    public static (IDictionary<string, JsonNode?>?, bool) EncryptDict(IDictionary<string, JsonNode?>? data, IReadOnlyDictionary<string, JsonSchemaProperty> actualProperties, string instanceId)
    {
        bool hasAnyEncryption = false;

        if (data is not null)
        {
            foreach (var item in actualProperties)
            {
                bool? hasEncryptKey = item.Value.ExtensionData?.TryGetValue("$encrypt", out _);
                if (hasEncryptKey == true)
                {
                    var value = data[item.Key]?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        var encrypted = AesHelper.EncryptString(instanceId, value);
                        data[item.Key] = encrypted;
                        hasAnyEncryption = true;
                    }
                }
                else
                {

                    var itemActualProp = item.Value.ActualSchema.ActualProperties;
                    if (itemActualProp.Count > 0)
                    {
                        if (data.TryGetValue(item.Key, out var nodeValue))
                        {
                            var innerDict = nodeValue as IDictionary<string, JsonNode?>;
                            var encResult = EncryptDict(innerDict, itemActualProp, instanceId);
                            if (encResult.Item1 != null)
                            {
                                data[item.Key] = encResult.Item1 as JsonNode;
                            }
                        }
                    }
                }
            }
            return (data, hasAnyEncryption);
        }
        return (null, false);
    }

    private static JsonObject? DictionaryToJsonObject(IDictionary<string, JsonNode> dict)
    {
        var ser = AmorphieJsonSerializer.Serialize(dict);
        return AmorphieJsonSerializer.Deserialize<JsonObject>(ser);
    }

}