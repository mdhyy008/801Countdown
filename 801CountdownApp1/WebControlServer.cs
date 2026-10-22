using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace _801CountdownApp1
{
    public class WebControlServer
    {
        private sealed class AuthToken
        {
            public string Password;
            public DateTime ExpiresUtc;
        }

        private const string AuthCookieName = "801auth";
        private static readonly ConcurrentDictionary<string, AuthToken> tokens =
            new ConcurrentDictionary<string, AuthToken>();

        private readonly Form1 _form1;
        private readonly HttpListener _listener;
        private readonly Thread _serverThread;
        public string LastError { get; private set; }

        public WebControlServer(Form1 form1, int port)
        {
            _form1 = form1;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{port}/");
            _serverThread = new Thread(ServerLoop) { IsBackground = true };
        }

        // 在调用线程（UI 线程）上启动监听，端口占用/权限不足时能立刻得到结果
        public bool Start()
        {
            try
            {
                _listener.Start();
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                try { _listener.Close(); } catch { }
                return false;
            }
            _serverThread.Start();
            return true;
        }

        public void Stop()
        {
            try { _listener.Stop(); } catch { }
            try { _listener.Close(); } catch { }
        }

        private void ServerLoop()
        {
            while (true)
            {
                try
                {
                    var context = _listener.GetContext();
                    Task.Run(() => HandleRequest(context));
                }
                catch { break; }
            }
        }

        private static string GetPassword()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1"))
                    return key?.GetValue("WebPassword") as string ?? "";
            }
            catch { return ""; }
        }

        // 令牌同时记住签发时的密码，改密码后旧会话自动失效
        private static bool IsAuthorized(HttpListenerRequest request)
        {
            string password = GetPassword();
            if (string.IsNullOrEmpty(password)) return true;
            var cookie = request.Cookies[AuthCookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value)) return false;
            AuthToken token;
            if (!tokens.TryGetValue(cookie.Value, out token)) return false;
            if (token.ExpiresUtc <= DateTime.UtcNow || token.Password != password)
            {
                tokens.TryRemove(cookie.Value, out _);
                return false;
            }
            return true;
        }

        private static void PurgeExpiredTokens()
        {
            DateTime now = DateTime.UtcNow;
            foreach (var pair in tokens)
            {
                if (pair.Value.ExpiresUtc <= now) tokens.TryRemove(pair.Key, out _);
            }
        }
        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                context.Response.KeepAlive = false;
                string path = context.Request.Url.AbsolutePath.ToLower();
                if (context.Request.HttpMethod == "POST" && path == "/login")
                {
                    HandleLogin(context);
                    return;
                }
                // 页面本身允许加载（未登录时由前端弹出登录框），其余请求一律校验
                bool isPageRequest = context.Request.HttpMethod == "GET" && (path == "/" || path.Length == 0);
                if (!isPageRequest && !IsAuthorized(context.Request))
                {
                    context.Response.StatusCode = 401;
                    byte[] denied = Encoding.UTF8.GetBytes("unauthorized");
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    context.Response.ContentLength64 = denied.Length;
                    context.Response.OutputStream.Write(denied, 0, denied.Length);
                    context.Response.Close();
                    return;
                }
                if (path == "/status")
                {
                    var state = _form1.GetCountdownState();
                    int seconds = _form1.GetRemainingSeconds();
                    int totalSec = _form1.GetTotalSeconds();
                    int minutes = Math.Max(1, seconds / 60);
                    bool isRunning = _form1.IsCountingDown();
                    bool showTimer = _form1.IsTimerVisible();
                    bool flash3 = _form1.IsFlash3Enabled();
                    bool pptAuto = _form1.IsPptAutoEnabled();
                    bool showSlideInfo = _form1.IsShowSlideInfoEnabled();
                    bool sound = _form1.IsSoundEnabled();
                    int opacityVal = _form1.GetOpacityValue();
                    int fontSize = _form1.GetFontSize();
                    bool allowNegative = _form1.IsAllowNegativeEnabled();
                    bool pureSlide = _form1.IsPureSlideModeEnabled();
                    string slideInfo = _form1.GetLastSlideInfo();
                    // 读取权限设置
                    var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
                    int setTime = key?.GetValue("SetTime") is int v1 ? v1 : 1;
                    int startPauseReset = key?.GetValue("StartPauseReset") is int v2 ? v2 : 1;
                    int permWindow = key?.GetValue("RemoteWindowPosition") is int v3 ? v3 : 1;
                    int permAppearance = key?.GetValue("RemoteAppearance") is int v4 ? v4 : 1;
                    int permFlashSound = key?.GetValue("RemoteFlashSound") is int v5 ? v5 : 1;
                    int permSettings = key?.GetValue("RemoteSettings") is int v6 ? v6 : 1;
                    int permPresentation = key?.GetValue("RemotePresentation") is int v7 ? v7 : 1;
                    int permDocPreview = key?.GetValue("RemoteDocPreview") is int v8 ? v8 : 1;
                    key?.Close();
                    string fgInfo = _form1.GetForegroundAppInfo();
                    string presentationApp = _form1.GetPresentationAppName();
                    // 通过 UI 线程获取屏幕信息，避免跨线程问题
                    int screenCount = 1;
                    int screenIndex = 1;
                    try
                    {
                        _form1.Invoke(new Action(() =>
                        {
                            screenCount = Screen.AllScreens.Length;
                            var current = Screen.FromControl(_form1);
                            screenIndex = Array.IndexOf(Screen.AllScreens, current) + 1;
                        }));
                    }
                    catch { }
                    string json =$"{{\"state\":\"{EscapeJson(state)}\",\"seconds\":{seconds},\"totalSeconds\":{totalSec},\"minutes\":{minutes},\"isRunning\":{(isRunning ? "true" : "false")},\"showTimer\":{(showTimer ? "true" : "false")},\"flash3\":{(flash3 ? "true" : "false")},\"pptAuto\":{(pptAuto ? "true" : "false")},\"showSlideInfo\":{(showSlideInfo ? "true" : "false")},\"sound\":{(sound ? "true" : "false")},\"opacity\":{opacityVal},\"fontSize\":{fontSize},\"allowNegative\":{(allowNegative ? "true" : "false")},\"pureSlide\":{(pureSlide ? "true" : "false")},\"slideInfo\":\"{EscapeJson(slideInfo)}\",\"setTime\":{setTime},\"startPauseReset\":{startPauseReset},\"permWindow\":{permWindow},\"permAppearance\":{permAppearance},\"permFlashSound\":{permFlashSound},\"permSettings\":{permSettings},\"permPresentation\":{permPresentation},\"permDocPreview\":{permDocPreview},\"presentationApp\":\"{EscapeJson(presentationApp)}\",\"foregroundInfo\":\"{EscapeJson(fgInfo)}\",\"screenCount\":{screenCount},\"screenIndex\":{screenIndex}}}";
                    byte[] buffff = Encoding.UTF8.GetBytes(json);
                    context.Response.ContentType = "application/json; charset=utf-8";
                    context.Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                    context.Response.ContentLength64 = buffff.Length;
                    context.Response.OutputStream.Write(buffff, 0, buffff.Length);
                    context.Response.Close();
                    return;
                }
                if (context.Request.HttpMethod == "POST")
                {
                    using (var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        string body = reader.ReadToEnd();
                        bool canControl = Allow("StartPauseReset");
                        bool canTime = Allow("SetTime");
                        bool canWindow = Allow("RemoteWindowPosition");
                        bool canAppearance = Allow("RemoteAppearance");
                        bool canFlashSound = Allow("RemoteFlashSound");
                        bool canSettings = Allow("RemoteSettings");
                        bool canPresentation = Allow("RemotePresentation");
                        if (path == "/toggle")
                        {
                            if (canControl) _form1.Invoke(new Action(_form1.ToggleCountdown));
                        }
                        else if (path == "/reset")
                        {
                            if (canControl) _form1.Invoke(new Action(_form1.ResetTimeFromRegistry));
                        }
                        else if (path == "/flash")
                        {
                            if (canFlashSound) _form1.Invoke(new Action(async () => await _form1.FlashWindowAsync()));
                        }
                        else if (path == "/testsound")
                        {
                            if (canFlashSound) _form1.Invoke(new Action(_form1.TestSound));
                        }
                        else if (path == "/settime")
                        {
                            if (canTime && int.TryParse(body, out int seconds))
                                _form1.Invoke(new Action(() => _form1.SetRemainingSeconds(seconds)));
                        }
                        else if (path == "/settotaltime")
                        {
                            if (canTime && int.TryParse(body, out int minutes) && minutes > 0)
                            {
                                if (minutes > int.MaxValue / 60)
                                    minutes = int.MaxValue / 60;
                                _form1.Invoke(new Action(() => _form1.SetTotalTimeMinutes(minutes)));
                            }
                        }
                        else if (path == "/setrighttop")
                        {
                            if (canWindow) _form1.Invoke(new Action(_form1.MoveToRightTop));
                        }
                        else if (path == "/setlefttop")
                        {
                            if (canWindow) _form1.Invoke(new Action(_form1.MoveToLeftTop));
                        }
                        else if (path == "/settopcenter")
                        {
                            if (canWindow) _form1.Invoke(new Action(_form1.MoveToTopCenter));
                        }
                        else if (path == "/moveup")
                        {
                            if (canWindow) _form1.Invoke(new Action(() => _form1.MoveBy(0, -20)));
                        }
                        else if (path == "/movedown")
                        {
                            if (canWindow) _form1.Invoke(new Action(() => _form1.MoveBy(0, 20)));
                        }
                        else if (path == "/moveleft")
                        {
                            if (canWindow) _form1.Invoke(new Action(() => _form1.MoveBy(-20, 0)));
                        }
                        else if (path == "/moveright")
                        {
                            if (canWindow) _form1.Invoke(new Action(() => _form1.MoveBy(20, 0)));
                        }
                        else if (path == "/cyclescreen")
                        {
                            if (canWindow) _form1.Invoke(new Action(_form1.CycleScreen));
                        }
                        else if (path == "/setfontsize")
                        {
                            if (canAppearance && int.TryParse(body, out int size))
                                _form1.Invoke(new Action(() => _form1.SetFontSize(size)));
                        }
                        else if (path == "/setopacity")
                        {
                            if (canAppearance && int.TryParse(body, out int val))
                                _form1.Invoke(new Action(() => _form1.SetOpacity(val)));
                        }
                        else if (path == "/setshowtimer")
                        {
                            if (canSettings) _form1.Invoke(new Action(() => _form1.ToggleTimerVisible()));
                        }
                        else if (path == "/setflash3")
                        {
                            if (canSettings) _form1.Invoke(new Action(() => _form1.ToggleFlash3()));
                        }
                        else if (path == "/togglesound")
                        {
                            if (canSettings) _form1.Invoke(new Action(_form1.ToggleSound));
                        }
                        else if (path == "/togglepptauto")
                        {
                            if (canSettings) _form1.Invoke(new Action(_form1.TogglePptAuto));
                        }
                        else if (path == "/toggleslideinfo")
                        {
                            if (canSettings) _form1.Invoke(new Action(_form1.ToggleShowSlideInfo));
                        }
                        else if (path == "/toggleallownegative")
                        {
                            if (canSettings) _form1.Invoke(new Action(_form1.ToggleAllowNegative));
                        }
                        else if (path == "/togglepureslide")
                        {
                            if (canSettings) _form1.Invoke(new Action(_form1.TogglePureSlideMode));
                        }
                        else if (path == "/startshow")
                        {
                            if (canPresentation) _form1.Invoke(new Action(_form1.StartSlideShow));
                        }
                        else if (path == "/closeshow")
                        {
                            if (canPresentation) _form1.Invoke(new Action(_form1.CloseSlideShowApp));
                        }
                        else if (path == "/setpresentermode")
                        {
                            if (canPresentation && int.TryParse(body, out int mode))
                                _form1.Invoke(new Action(() => _form1.SetPresenterMode(mode)));
                        }
                        else if (path == "/setpodiummode")
                        {
                            if ((canPresentation || Allow("RemoteDocPreview")) && int.TryParse(body, out int podiumMode))
                                _form1.Invoke(new Action(() => _form1.SetPodiumMode(podiumMode)));
                        }
                        else if (path == "/nextslide" || path == "/prevslide" || path == "/gotoslide"
                              || path == "/endshow")
                        {
                            string action = path == "/nextslide" ? "next"
                                : path == "/prevslide" ? "prev"
                                : path == "/gotoslide" ? "goto"
                                : "end";
                            int gotoPage = 0;
                            if (action == "goto") int.TryParse(body, out gotoPage);
                            string cmdError = canPresentation ? null : "放映控制已禁用";
                            bool cmdOk = false;
                            if (canPresentation) cmdOk = _form1.SlideShowControl(action, gotoPage, out cmdError);
                            string cmdJson = $"{{\"ok\":{(cmdOk ? "true" : "false")},\"error\":\"{EscapeJson(cmdError)}\"}}";
                            byte[] cmdBuf = Encoding.UTF8.GetBytes(cmdJson);
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json; charset=utf-8";
                            context.Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                            context.Response.ContentLength64 = cmdBuf.Length;
                            context.Response.OutputStream.Write(cmdBuf, 0, cmdBuf.Length);
                            context.Response.Close();
                            return;
                        }
                    }
                    context.Response.StatusCode = 204;
                    context.Response.Close();
                    return;
                }
                if (context.Request.HttpMethod == "GET" && path == "/preview")
                {
                    if (!Allow("RemoteDocPreview"))
                    {
                        context.Response.StatusCode = 403;
                        context.Response.Close();
                        return;
                    }
                    // 流式输出 NDJSON：先给总页数/当前页，之后每导出一页写一行
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/x-ndjson; charset=utf-8";
                    context.Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                    context.Response.SendChunked = true;
                    var outStream = context.Response.OutputStream;
                    var writeLock = new object();
                    bool writeFailed = false;
                    Action<string> writeLine = line =>
                    {
                        if (writeFailed) return;
                        try
                        {
                            byte[] chunk = Encoding.UTF8.GetBytes(line + "\n");
                            lock (writeLock)
                            {
                                outStream.Write(chunk, 0, chunk.Length);
                                outStream.Flush();
                            }
                        }
                        catch { writeFailed = true; }
                    };
                    _form1.StreamPresentationPreview(
                        p => writeLine($"{{\"total\":{p.Total},\"current\":{p.Current},\"session\":\"{p.Session}\"}}"),
                        (i, src) => writeLine($"{{\"i\":{i},\"src\":\"{src}\"}}"),
                        err => writeLine($"{{\"error\":\"{EscapeJson(err)}\"}}"));
                    writeLine("{\"done\":true}");
                    context.Response.Close();
                    return;
                }
                if (context.Request.HttpMethod == "GET" && path == "/slidecurrent")
                {
                    if (!Allow("RemoteDocPreview") && !Allow("RemotePresentation"))
                    {
                        context.Response.StatusCode = 403;
                        context.Response.Close();
                        return;
                    }
                    int curSlide = _form1.GetCurrentSlideIndex();
                    int totalSlide = 0;
                    string lastInfo = _form1.GetLastSlideInfo();
                    if (!string.IsNullOrEmpty(lastInfo))
                    {
                        string[] infoParts = lastInfo.Split('/');
                        if (infoParts.Length == 2) int.TryParse(infoParts[1], out totalSlide);
                    }
                    int presenterMode = _form1.GetPresenterMode();
                    int podiumMode = _form1.GetPodiumMode();
                    string slideJson = curSlide > 0
                        ? $"{{\"ok\":true,\"current\":{curSlide},\"total\":{totalSlide},\"presenterMode\":{presenterMode},\"podiumMode\":{podiumMode}}}"
                        : $"{{\"ok\":false,\"total\":{totalSlide},\"presenterMode\":{presenterMode},\"podiumMode\":{podiumMode}}}";
                    byte[] slideBuf = Encoding.UTF8.GetBytes(slideJson);
                    context.Response.ContentType = "application/json; charset=utf-8";
                    context.Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                    context.Response.ContentLength64 = slideBuf.Length;
                    context.Response.OutputStream.Write(slideBuf, 0, slideBuf.Length);
                    context.Response.Close();
                    return;
                }
                if (context.Request.HttpMethod == "GET" && path.StartsWith("/presenterslide/"))
                {
                    if (!Allow("RemotePresentation") && !Allow("RemoteDocPreview"))
                    {
                        context.Response.StatusCode = 403;
                        context.Response.Close();
                        return;
                    }
                    string rel = path.Substring("/presenterslide/".Length);
                    string numText = rel.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                        ? rel.Substring(0, rel.Length - 4)
                        : "";
                    int page = 0;
                    bool valid = IsDigits(numText) && int.TryParse(numText, out page) && page > 0;
                    string pFile = null;
                    string pError = "参数错误";
                    if (valid) pFile = _form1.ExportPresenterSlide(page, out pError);
                    if (pFile == null)
                    {
                        byte[] pErrBuf = Encoding.UTF8.GetBytes(pError ?? "导出失败");
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = pErrBuf.Length;
                        context.Response.OutputStream.Write(pErrBuf, 0, pErrBuf.Length);
                        context.Response.Close();
                        return;
                    }
                    byte[] pImage = System.IO.File.ReadAllBytes(pFile);
                    context.Response.ContentType = "image/png";
                    context.Response.AddHeader("Cache-Control", "no-store");
                    context.Response.ContentLength64 = pImage.Length;
                    context.Response.OutputStream.Write(pImage, 0, pImage.Length);
                    context.Response.Close();
                    return;
                }
                if (context.Request.HttpMethod == "GET" && path.StartsWith("/previewfile/"))
                {
                    string relative = path.Substring("/previewfile/".Length);
                    string[] parts = relative.Split('/');
                    bool valid = parts.Length == 2 && IsHexId(parts[0]);
                    string fileName = valid ? System.IO.Path.GetFileName(parts[1]) : "";
                    valid = valid
                        && fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                        && IsDigits(System.IO.Path.GetFileNameWithoutExtension(fileName));
                    string file = valid
                        ? System.IO.Path.Combine(Application.StartupPath, "preview", parts[0], fileName)
                        : null;
                    if (file == null || !System.IO.File.Exists(file))
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                        return;
                    }
                    byte[] image = System.IO.File.ReadAllBytes(file);
                    context.Response.ContentType = "image/png";
                    context.Response.AddHeader("Cache-Control", "max-age=3600");
                    context.Response.ContentLength64 = image.Length;
                    context.Response.OutputStream.Write(image, 0, image.Length);
                    context.Response.Close();
                    return;
                }
                if (context.Request.HttpMethod == "GET" && path.StartsWith("/previewhd/"))
                {
                    if (!Allow("RemoteDocPreview"))
                    {
                        context.Response.StatusCode = 403;
                        context.Response.Close();
                        return;
                    }
                    string relative = path.Substring("/previewhd/".Length);
                    string[] parts = relative.Split('/');
                    bool valid = parts.Length == 2 && IsHexId(parts[0]);
                    string numText = valid ? System.IO.Path.GetFileNameWithoutExtension(parts[1]) : "";
                    valid = valid
                        && parts[1].EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                        && IsDigits(numText);
                    int page = valid ? int.Parse(numText) : 0;
                    string file = null;
                    string error = "参数错误";
                    if (valid) file = _form1.ExportHighResSlide(parts[0], page, out error);
                    if (file == null)
                    {
                        byte[] errBuf = Encoding.UTF8.GetBytes(error ?? "导出失败");
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = errBuf.Length;
                        context.Response.OutputStream.Write(errBuf, 0, errBuf.Length);
                        context.Response.Close();
                        return;
                    }
                    byte[] hdImage = System.IO.File.ReadAllBytes(file);
                    context.Response.ContentType = "image/png";
                    context.Response.AddHeader("Cache-Control", "no-store");
                    context.Response.ContentLength64 = hdImage.Length;
                    context.Response.OutputStream.Write(hdImage, 0, hdImage.Length);
                    context.Response.Close();
                    return;
                }
                // GET: 返回页面
                string html = GetHtml();
                byte[] buf = Encoding.UTF8.GetBytes(html);
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                context.Response.ContentLength64 = buf.Length;
                context.Response.OutputStream.Write(buf, 0, buf.Length);
                context.Response.Close();
            }
            catch { try { context.Response.Close(); } catch { } }
        }
        private static string EscapeJson(string s)
        {
            return (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

        private static bool IsHexId(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != 16) return false;
            foreach (char c in s)
            {
                bool hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!hex) return false;
            }
            return true;
        }

        private static bool IsDigits(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
                if (c < '0' || c > '9') return false;
            return true;
        }

        private static bool Allow(string name)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1"))
                    return key?.GetValue(name) is int v ? v == 1 : true;
            }
            catch { return true; }
        }
        private void HandleLogin(HttpListenerContext context)
        {
            string password = GetPassword();
            string body;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding ?? Encoding.UTF8))
                body = reader.ReadToEnd();

            if (body == password)
            {
                PurgeExpiredTokens();
                string token = Guid.NewGuid().ToString("N");
                tokens[token] = new AuthToken { Password = password, ExpiresUtc = DateTime.UtcNow.AddHours(12) };
                context.Response.AddHeader("Set-Cookie",
                    $"{AuthCookieName}={token}; Path=/; Max-Age=43200; HttpOnly; SameSite=Lax");
                context.Response.StatusCode = 204;
            }
            else
            {
                context.Response.StatusCode = 403;
                byte[] msg = Encoding.UTF8.GetBytes("wrong password");
                context.Response.ContentType = "text/plain; charset=utf-8";
                context.Response.ContentLength64 = msg.Length;
                context.Response.OutputStream.Write(msg, 0, msg.Length);
            }
            context.Response.Close();
        }

        private string GetHtml()
        {
            string template = File.ReadAllText(System.Windows.Forms.Application.StartupPath + "\\webcontrol.html", Encoding.UTF8);
            string state = _form1.GetCountdownState();
            int seconds = _form1.GetRemainingSeconds();
            int minutes = Math.Max(1, seconds / 60);
            bool isRunning = _form1.IsCountingDown();
            bool showTimer = _form1.IsTimerVisible();
            bool flash3 = _form1.IsFlash3Enabled();
            // 读取权限设置
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
            int setTime = key?.GetValue("SetTime") is int v1 ? v1 : 1;
            int startPauseReset = key?.GetValue("StartPauseReset") is int v2 ? v2 : 1;
            key?.Close();
            string fgInfo = _form1.GetForegroundAppInfo();
            string html = template
                .Replace("{{state}}", state)
                .Replace("{{seconds}}", seconds.ToString())
                .Replace("{{minutes}}", minutes.ToString())
                .Replace("{{toggleText}}", isRunning ? "暂停" : "启动")
                .Replace("{{showTimerChecked}}", showTimer ? "checked" : "")
                .Replace("{{flash3Checked}}", flash3 ? "checked" : "")
                .Replace("{{setTime}}", setTime.ToString())
                .Replace("{{startPauseReset}}", startPauseReset.ToString())
                .Replace("{{hostInfo}}", GetHostInfo())
                .Replace("{{needAuth}}", string.IsNullOrEmpty(GetPassword()) ? "false" : "true")
                .Replace("{{foregroundInfo}}", fgInfo)
                .Replace("{{version}}", GetBuildVersion());
            return html;
        }

        private static string GetBuildVersion()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "version.txt");
                if (File.Exists(path))
                {
                    string text = File.ReadAllText(path).Trim();
                    if (text.Length > 0) return text;
                }
            }
            catch { }
            return Application.ProductVersion;
        }
        private string GetHostInfo()
        {
            string ip = "";
            try
            {
                foreach (var host in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList)
                {
                    if (host.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = host.ToString();
                        break;
                    }
                }
            }
            catch { }
            return $"{Environment.UserName} @ {Environment.MachineName}  |  {ip}";
        }
    }
} 