using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using VSharp;

namespace VSharpLib
{
    [Module]
    class Http 
    {
        private readonly Interpreter interpreter;

        public Http(Interpreter interpreter) {
            this.interpreter = interpreter;
        }

        public HttpResponse Get(string url) 
        {
             // Initialize HttpClient
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                return new HttpResponse(response);
            }
        }

        public HttpResponse Get(VSharpObject headers, string url) 
        {
             // Initialize HttpClient
            using (HttpClient client = new HttpClient())
            {
                SetHeaders(client, headers);
                HttpResponseMessage response = client.GetAsync(url).Result;
                return new HttpResponse(response);
            }
        }

        public HttpResponse Post(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.PostAsync(url, null).Result;
                return new HttpResponse(response);
            }
        }

        public HttpResponse Post(string url, object? body)
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, payload).Result;
                return new HttpResponse(response);
            }
        }

        public HttpResponse Post(VSharpObject headers, string url, object? body)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeaders(client, headers);
                StringContent payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, payload).Result;
                return new HttpResponse(response);
            }
        }

        void SetHeaders(HttpClient client, VSharpObject headers)
        {
            foreach (var (name, value) in headers.Entries)
            {
                client.DefaultRequestHeaders.Add(name?.ToString() ?? "null", value?.ToString() ?? "null");
            }
        }

        public HttpResponse Post(VSharpObject headers, string url)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeaders(client, headers);
                HttpResponseMessage response = client.PostAsync(url, null).Result;
                return new HttpResponse(response);
            }
        }

         public HttpResponse Put(VSharpObject headers, string url)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeaders(client, headers);
                HttpResponseMessage response = client.PutAsync(url, null).Result;
                return new HttpResponse(response);
            }
        }

        public HttpResponse Put(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.PutAsync(url, null).Result;
                return new HttpResponse(response);
            }
        }

        public HttpResponse Put(string url, object? body)
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PutAsync(url, payload).Result;
                return new HttpResponse(response);
            }
        }

        public HttpResponse Put(VSharpObject headers, string url, object? body)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeaders(client, headers);
                StringContent payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PutAsync(url, payload).Result;
                return new HttpResponse(response);
            }
        }

        public WebServer Server()
        {
            return new WebServer(interpreter);
        }
    }


    class HttpResponse 
    {
        HttpResponseMessage response;

        public HttpResponse(HttpResponseMessage response)
        {
            this.response = response;
        }

        public int Status { get {
            return (int) response.StatusCode;
        } }

        public int StatusCode() 
        {
            return (int)response.StatusCode;
        }

        public string Content()
        {
            return response.Content.ReadAsStringAsync().Result;
        }

        public object? Json()
        {
            string content = Content();
            JsonDocument output = JsonDocument.Parse(content);

            return VSharpLib.Json.ParseElement(output.RootElement);
        }
    }

    class WebServer 
    {
        public VSharpObject Get { get; }
        public VSharpObject Post {get; }
        public VSharpObject Put  {get; }
        public VSharpObject Delete  {get; }

        Dictionary<string, PathMatcher> paths { get; }

        bool running;
        Interpreter interpreter;

        public WebServer(Interpreter interpreter) 
        {
            Get = new VSharpObject { Entries = new Dictionary<object, object?>()};
            Post = new VSharpObject { Entries = new Dictionary<object, object?>() };
            Put = new VSharpObject { Entries = new Dictionary<object, object?>() };
            Delete = new VSharpObject { Entries = new Dictionary<object, object?>() };
            paths = new Dictionary<string, PathMatcher>();
            running = false;
            this.interpreter = interpreter;
        }

        void HandleIncomingConnections(int port)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
        
            // Start the listener
            listener.Start();
            while (running)
            {
                // Wait for a request
                HttpListenerContext ctx = listener.GetContext();

                // Get the request and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

               
                var url = req.Url?.AbsolutePath ?? "/";
                var result = paths[req.HttpMethod].Match(url);

                if (result == null) 
                {
                    resp.StatusCode = (int) HttpStatusCode.NotFound;
                    resp.OutputStream.Write(Encoding.UTF8.GetBytes("Route not found"));
                    return;
                }


                var (func, pathVariables) =((Function, string[])) result;

                try 
                {
                    List<object?> args = new List<object?> { new ServerRequest(req), new ServerResponse(resp) };
                    args.AddRange(pathVariables);

                    func.Invoke(args, interpreter);
                } finally 
                {
                    resp.Close();
                }
            }
        }

        public void Stop()
        {
            running = false;
        }

        void InsertHandlers(string methodName, VSharpObject obj)
        {
            PathMatcher instance = new PathMatcher();
            paths[methodName] = instance;
            foreach (var (path, handler) in obj.Entries) 
            {
                string strPath = path as string ?? throw new Exception("Paths can only be strings");
                Function handlerFunc = handler as Function ?? throw new Exception("Handlers must be funcitons");
                instance.Insert(strPath, handlerFunc);

            }
        }

        public void Start()
        {
           Start(8080);
        }

        public void Start(int port)
        {
            running = true;

            InsertHandlers("GET", Get);
            InsertHandlers("POST", Post);
            InsertHandlers("PUT", Put);
            InsertHandlers("DELETE", Delete);

            HandleIncomingConnections(port);
        }
    }

    class ServerRequest
    {
        readonly HttpListenerRequest req;

        public ServerRequest(HttpListenerRequest req)
        {
            this.req = req;
        }

        public string Body() 
        {
            Span<byte> bytes = new Span<byte>();
            req.InputStream.Read(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public object? Json()
        {
            return VSharpLib.Json.ParseElement(JsonDocument.Parse(Body()).RootElement);
        }
    }

    class ServerResponse 
    {

        readonly HttpListenerResponse res;

        bool statusSet = false;

        public ServerResponse(HttpListenerResponse res)
        {
            this.res = res;
        }

        void Status(int code)
        {
            res.StatusCode =code;
            statusSet = true;
        }

        void Json(object json)
        {
            if (!statusSet) res.StatusCode = 200;
            res.AddHeader("Content-Type", "text/json");
            res.OutputStream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(json)));
        }

        void Html(string html)
        {
            if (!statusSet)  res.StatusCode = 200;
            res.AddHeader("Content-Type", "text/html");
            res.OutputStream.Write(Encoding.UTF8.GetBytes(html));
        }

        void Text(string text)
        {
            if (!statusSet) res.StatusCode = 200;
            res.AddHeader("Content-Type", "text/plain");
            res.OutputStream.Write(Encoding.UTF8.GetBytes(text));
        }

        void Data(byte[] data)
        {
            if (!statusSet) res.StatusCode = 200;
            res.OutputStream.Write(data);
        }

        void Error(int code)
        {
            res.StatusCode = code;
        }

        void Error(int code, string message)
        {
            res.StatusCode = code;

        }

    }

    class PathMatcher
    {
        Function? value;

        Dictionary<string, PathMatcher> children;
        PathMatcher? wildCard;

        public PathMatcher()
        {
            children = new Dictionary<string, PathMatcher>();
            wildCard = null;
            value = null;
        }

        public void Insert(string path, Function handler) {
            if (path == "" || path == "/")
            {
                value = handler;
                return;
            }
            int end = path.IndexOf("/");
            string nextPath;
            string item;

            if (end == -1) 
            {
                item = path.Substring(0, path.Length - 1);
                nextPath = ""; //there is no next path since we dont have any othr "/"
            } else 
            {
                item = path.Substring(0, end);
                int start = end + 1; //skipping the /
                nextPath = path.Substring(start, path.Length - start);
                
            }

            if (item == "{}")
            {
                if (wildCard == null) {
                    wildCard = new PathMatcher();
                }

                wildCard.Insert(nextPath, handler);
                return;
            }


            PathMatcher matcher;
            if (!children.ContainsKey(item))
            {
                matcher = new PathMatcher();
                children[item] = matcher;
            } else
            {
                matcher = children[item];
            }

            matcher.Insert(nextPath, handler);
        }


        public (Function, string[])? Match(string path) 
        {
            List<string> pathVariables = new List<string>();
            Function? result = Match(SplitString(path, "/"), pathVariables);   
            if (result == null) {
                return null;
            }
            return (result, pathVariables.ToArray());
        }

        static IEnumerator<string> SplitString(string input, string delimiter)
        {
            string[] parts = input.Split(delimiter);
            
            foreach (var part in parts)
            {
                yield return part;
            }
        }


        Function? Match(IEnumerator<string> path, List<string> pathVariables)
        {
            if (path.Current == null) {
                return null;
            }
            if (children.ContainsKey(path.Current)) 
            {
                PathMatcher next = children[path.Current];
                if (!path.MoveNext()) 
                {
                    return next.value ?? null;
                }
                next.Match(path, pathVariables);
            }        

            if (wildCard != null && path.Current == "{}")
            {
                pathVariables.Add(path.Current);
                if (!path.MoveNext()) 
                {
                    return wildCard.value ?? null;
                }
                wildCard.Match(path, pathVariables);
            }

            return null;
        }
    }
}