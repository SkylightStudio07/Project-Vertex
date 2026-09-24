#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Copy this file to Assets/Editor/SchoolCodeMcpBridge.cs in a project you control.
// It exposes only a loopback, bearer-protected, fixed-action bridge to the School Code extension.
// It never evaluates C# or shell commands received over HTTP.
[InitializeOnLoad]
public static class SchoolCodeMcpBridge
{
    private const string TokenKey = "SchoolCode.McpBridgeToken";
    private const string PortKey = "SchoolCode.McpBridgePort";
    private const int DefaultPort = 18777;
    private const int MaxBodyBytes = 1024 * 1024;
    private static readonly ConcurrentQueue<Pending> Queue = new ConcurrentQueue<Pending>();
    private static HttpListener listener;
    private static Thread listenerThread;
    private static string token;
    private static int port;

    [Serializable]
    private sealed class Request
    {
        public string action;
        public string scene;
        public string query;
        public string game_object_id;
        public string component_type;
        public string property;
        public string value;
        public string name;
        public string parent_id;
    }

    [Serializable]
    private sealed class GameObjectInfo
    {
        public string id;
        public string name;
        public string path;
        public bool active;
    }

    [Serializable]
    private sealed class ComponentInfo
    {
        public string type;
        public string json;
    }

    [Serializable]
    private sealed class Response
    {
        public bool ok;
        public string error;
        public string scene;
        public GameObjectInfo[] game_objects;
        public ComponentInfo component;
        public string game_object_id;
        public string property;
    }

    private sealed class Pending
    {
        public Request request;
        public readonly ManualResetEvent Done = new ManualResetEvent(false);
        public Response Response;
    }

    static SchoolCodeMcpBridge()
    {
        EditorApplication.update += Pump;
        AssemblyReloadEvents.beforeAssemblyReload += Stop;
        EditorApplication.quitting += Stop;
        // Loading the bridge is enough to start it. The VS Code extension installs
        // the file and places the per-user token before opening the project.
        Start();
    }

    private static string TokenFilePath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".school-code", "unity-editor-token");
    }

    private static string LoadToken()
    {
        try
        {
            var fileToken = File.ReadAllText(TokenFilePath()).Trim();
            if (!string.IsNullOrEmpty(fileToken))
            {
                EditorPrefs.SetString(TokenKey, fileToken);
                return fileToken;
            }
        }
        catch { }
        return EditorPrefs.GetString(TokenKey, "");
    }

    [MenuItem("School Code/MCP Bridge/Start")]
    public static void Start()
    {
        if (listener != null && listener.IsListening) return;
        port = EditorPrefs.GetInt(PortKey, DefaultPort);
        if (port < 1024 || port > 65535) port = DefaultPort;
        token = LoadToken();
        if (string.IsNullOrEmpty(token))
        {
            token = Guid.NewGuid().ToString("N");
            EditorPrefs.SetString(TokenKey, token);
            Debug.Log("School Code Unity Editor MCP token created. Use School Code/MCP Bridge/Copy Token.");
        }
        listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
        try
        {
            listener.Start();
            listenerThread = new Thread(ListenLoop) { IsBackground = true, Name = "SchoolCodeMcpBridge" };
            listenerThread.Start();
            Debug.Log("School Code Unity Editor MCP bridge listening on http://127.0.0.1:" + port + "/mcp");
        }
        catch (Exception e)
        {
            listener = null;
            Debug.LogError("School Code Unity Editor MCP bridge failed to start: " + e.Message);
        }
    }

    [MenuItem("School Code/MCP Bridge/Stop")]
    public static void Stop()
    {
        try { listener?.Stop(); listener?.Close(); } catch { }
        listener = null;
        listenerThread = null;
        while (Queue.TryDequeue(out var pending))
        {
            pending.Response = Error("Unity Editor 브리지가 종료되었습니다.");
            pending.Done.Set();
        }
    }

    [MenuItem("School Code/MCP Bridge/Copy Token")]
    public static void CopyToken()
    {
        if (string.IsNullOrEmpty(token)) Start();
        GUIUtility.systemCopyBuffer = token ?? "";
        Debug.Log("School Code Unity Editor MCP token copied to the clipboard.");
    }

    private static void ListenLoop()
    {
        while (listener != null && listener.IsListening)
        {
            try
            {
                var context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(_ => Handle(context));
            }
            catch { if (listener == null || !listener.IsListening) break; }
        }
    }

    private static void Handle(HttpListenerContext context)
    {
        try
        {
            if (context.Request.HttpMethod != "POST" || context.Request.Url.AbsolutePath != "/mcp")
            {
                Write(context, 404, Error("지원하지 않는 Unity Editor 브리지 경로입니다."));
                return;
            }
            var expected = "Bearer " + token;
            if (!string.Equals(context.Request.Headers["Authorization"], expected, StringComparison.Ordinal))
            {
                Write(context, 401, Error("Unity Editor 브리지 인증이 필요합니다."));
                return;
            }
            if (context.Request.ContentLength64 > MaxBodyBytes)
            {
                Write(context, 413, Error("요청이 너무 큽니다."));
                return;
            }
            string body;
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8)) body = reader.ReadToEnd();
            if (Encoding.UTF8.GetByteCount(body) > MaxBodyBytes)
            {
                Write(context, 413, Error("요청이 너무 큽니다."));
                return;
            }
            var request = JsonUtility.FromJson<Request>(body);
            if (request == null || string.IsNullOrEmpty(request.action))
            {
                Write(context, 400, Error("action이 필요합니다."));
                return;
            }
            var pending = new Pending { request = request };
            Queue.Enqueue(pending);
            if (!pending.Done.WaitOne(TimeSpan.FromSeconds(30)))
            {
                Write(context, 504, Error("Unity Editor 작업 시간이 초과되었습니다."));
                return;
            }
            Write(context, pending.Response != null && pending.Response.ok ? 200 : 400, pending.Response ?? Error("빈 응답입니다."));
        }
        catch (Exception e) { Write(context, 400, Error(e.Message)); }
    }

    private static void Write(HttpListenerContext context, int status, Response response)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(response));
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            using (var stream = context.Response.OutputStream) stream.Write(bytes, 0, bytes.Length);
        }
        catch { try { context.Response.Close(); } catch { } }
    }

    private static void Pump()
    {
        if (!Queue.TryDequeue(out var pending)) return;
        try { pending.Response = Execute(pending.request); }
        catch (Exception e) { pending.Response = Error(e.Message); }
        finally { pending.Done.Set(); }
    }

    private static Response Execute(Request request)
    {
        switch (request.action)
        {
            case "unity_open_scene": return OpenScene(request.scene);
            case "unity_find_gameobjects": return FindGameObjects(request.query ?? "");
            case "unity_get_component": return GetComponent(request);
            case "unity_set_component": return SetComponent(request);
            case "unity_create_gameobject": return CreateGameObject(request);
            case "unity_save_scene": return SaveScene();
            default: return Error("지원하지 않는 Unity Editor 작업입니다.");
        }
    }

    private static Response OpenScene(string scene)
    {
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        if (string.IsNullOrEmpty(scene) || Path.IsPathRooted(scene) || scene.Contains("..") || !scene.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) return Error("프로젝트 안의 .unity 상대 경로가 필요합니다.");
        var full = Path.GetFullPath(Path.Combine(projectRoot, scene));
        if (!full.StartsWith(projectRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(full)) return Error("씬 파일을 찾을 수 없습니다.");
        var opened = EditorSceneManager.OpenScene(full, OpenSceneMode.Single);
        return new Response { ok = true, scene = opened.path };
    }

    private static Response FindGameObjects(string query)
    {
        var items = Resources.FindObjectsOfTypeAll<GameObject>().Where(go => go.scene.IsValid() && !EditorUtility.IsPersistent(go) && (string.IsNullOrEmpty(query) || go.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 || HierarchyPath(go.transform).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)).Take(200).Select(go => new GameObjectInfo { id = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString(), name = go.name, path = HierarchyPath(go.transform), active = go.activeSelf }).ToArray();
        return new Response { ok = true, game_objects = items };
    }

    private static GameObject ResolveGameObject(string id)
    {
        if (!GlobalObjectId.TryParse(id, out var globalId)) return null;
        return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId) as GameObject;
    }

    private static Type ResolveComponentType(string name)
    {
        return TypeCache.GetTypesDerivedFrom<Component>().FirstOrDefault(type => type.FullName == name || type.Name == name);
    }

    private static Response GetComponent(Request request)
    {
        var gameObject = ResolveGameObject(request.game_object_id);if (gameObject == null) return Error("GameObject를 찾을 수 없습니다.");
        var type = ResolveComponentType(request.component_type);if (type == null) return Error("허용된 Component 형식을 찾을 수 없습니다.");
        var component = gameObject.GetComponent(type);if (component == null) return Error("해당 GameObject에 Component가 없습니다.");
        return new Response { ok = true, component = new ComponentInfo { type = type.FullName, json = EditorJsonUtility.ToJson(component, true) } };
    }

    private static Response SetComponent(Request request)
    {
        var gameObject = ResolveGameObject(request.game_object_id);if (gameObject == null) return Error("GameObject를 찾을 수 없습니다.");
        var type = ResolveComponentType(request.component_type);if (type == null) return Error("허용된 Component 형식을 찾을 수 없습니다.");
        var component = gameObject.GetComponent(type);if (component == null) return Error("해당 GameObject에 Component가 없습니다.");
        var serialized = new SerializedObject(component);var property = serialized.FindProperty(request.property);if (property == null) return Error("직렬화된 속성을 찾을 수 없습니다.");
        if (!Assign(property, request.value)) return Error("지원하지 않는 속성 형식입니다. string, bool, int, float만 지원합니다.");
        serialized.ApplyModifiedProperties();EditorUtility.SetDirty(component);return new Response { ok = true, game_object_id = request.game_object_id, property = request.property };
    }

    private static bool Assign(SerializedProperty property, string value)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.String: property.stringValue = value ?? ""; return true;
            case SerializedPropertyType.Boolean: if (bool.TryParse(value, out var boolean)) { property.boolValue = boolean; return true; } return false;
            case SerializedPropertyType.Integer: if (long.TryParse(value, out var integer)) { property.longValue = integer; return true; } return false;
            case SerializedPropertyType.Float: if (float.TryParse(value, out var floating)) { property.floatValue = floating; return true; } return false;
            default: return false;
        }
    }

    private static Response CreateGameObject(Request request)
    {
        if (string.IsNullOrWhiteSpace(request.name)) return Error("GameObject 이름이 필요합니다.");
        GameObject parent = null;if (!string.IsNullOrEmpty(request.parent_id)) { parent = ResolveGameObject(request.parent_id);if (parent == null) return Error("부모 GameObject를 찾을 수 없습니다."); }
        var gameObject = new GameObject(request.name.Trim());Undo.RegisterCreatedObjectUndo(gameObject, "School Code Create GameObject");
        if (parent != null) gameObject.transform.SetParent(parent.transform, false);
        Selection.activeGameObject = gameObject;return new Response { ok = true, game_object_id = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString() };
    }

    private static Response SaveScene() { EditorSceneManager.SaveOpenScenes();return new Response { ok = true, scene = SceneManager.GetActiveScene().path }; }
    private static string HierarchyPath(Transform transform) { var names = new List<string>();while (transform != null) { names.Add(transform.name);transform = transform.parent; }names.Reverse();return string.Join("/", names); }
    private static Response Error(string message) { return new Response { ok = false, error = (message ?? "오류").Substring(0, Math.Min(500, (message ?? "오류").Length)) }; }
}
#endif
