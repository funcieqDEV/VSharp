namespace VSharpLib
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using VSharp;

    [Module]
    class json
    {
        /// <summary>
        /// Parses a JSON string into a dynamic object.
        /// </summary>
        /// <param name="content">The JSON string to parse.</param>
        /// <returns>A dynamic object representing the parsed JSON.</returns>
        public object? parse(string content)
        {
            return parseElement(JsonDocument.Parse(content).RootElement);
        }

        /// <summary>
        /// Serializes an object to its JSON string representation.
        /// </summary>
        /// <param name="json">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        /// <exception cref="Exception">Thrown when trying to serialize a null object.</exception>
        public string toString(object? json)
        {
            if (json == null)
            {
                throw new Exception("Cannot serialize null");
            }
            return JsonSerializer.Serialize(json);
        }

        /// <summary>
        /// Parses a JSON element into a dynamic object.
        /// </summary>
        /// <param name="element">The JSON element to parse.</param>
        /// <returns>A dynamic object representing the parsed JSON element.</returns>
        public static object? parseElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var dict = new Dictionary<object, object?>();
                foreach (JsonProperty prop in element.EnumerateObject())
                {
                    dict[prop.Name] = parseElement(prop.Value);
                }
                return new VSharpObject { Entries = dict };
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                var list = new List<object?>();
                foreach (JsonElement arrayElement in element.EnumerateArray())
                {
                    list.Add(parseElement(arrayElement));
                }
                return list;
            }

            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    else
                        return element.GetDouble();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.ToString();
            }
        }

        /// <summary>
        /// Pretty prints a JSON object as a formatted string for better readability.
        /// </summary>
        /// <param name="json">The JSON object to pretty print.</param>
        /// <returns>A formatted JSON string.</returns>
        public string prettyPrint(object? json)
        {
            if (json == null)
            {
                throw new Exception("Cannot pretty print null");
            }
            return JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Merges two JSON objects into one. If there are duplicate keys, the values from the second object will overwrite the first.
        /// </summary>
        /// <param name="json1">The first JSON object.</param>
        /// <param name="json2">The second JSON object.</param>
        /// <returns>A merged JSON object.</returns>
        public object merge(object json1, object json2)
        {
            // Assuming json1 and json2 are both VSharpObjects
            var dict1 = ((VSharpObject)json1).Entries;
            var dict2 = ((VSharpObject)json2).Entries;

            foreach (var kvp in dict2)
            {
                dict1[kvp.Key] = kvp.Value;
            }

            return new VSharpObject { Entries = dict1 };
        }

        /// <summary>
        /// Checks if the specified JSON object contains a given key.
        /// </summary>
        /// <param name="json">The JSON object to check.</param>
        /// <param name="key">The key to search for in the JSON object.</param>
        /// <returns>True if the key exists in the JSON object; otherwise, false.</returns>
        public bool containsKey(object json, string key)
        {
            var dict = ((VSharpObject)json).Entries;
            return dict.ContainsKey(key);
        }
    }
}
