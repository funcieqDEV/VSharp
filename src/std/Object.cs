namespace VSharpLib
{
    using System.Collections;
    using System.Diagnostics;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using VSharp;

    [Module]
    class Object
    {
        public VSharpObject New()
        {
            return new VSharpObject { Entries = new Dictionary<object, object?>() };
        }
    }
}
