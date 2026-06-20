using System;
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
        private readonly Form1 _form1;
        private readonly HttpListener _listener;
        private readonly Thread _serverThread;
        public WebControlServer(Form1 form1)
        {
            _form1 = form1;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:9051/");
            _serverThread = new Thread(ServerLoop) { IsBackground = true };
        }
        public void Start() => _serverThread.Start();
        public void Stop()
        {
            try { _listener.Stop(); } catch { }
        }
        private void ServerLoop()
        {
            _listener.Start();
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
        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                context.Response.KeepAlive = false;
                string path = context.Request.Url.AbsolutePath.ToLower();
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
                    key?.Close();
                    string fgInfo = _form1.GetForegroundAppInfo();
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
                    string EscapeJson(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
                    string json = $"{{\"state\":\"{EscapeJson(state)}\",\"seconds\":{seconds},\"totalSeconds\":{totalSec},\"minutes\":{minutes},\"isRunning\":{(isRunning ? "true" : "false")},\"showTimer\":{(showTimer ? "true" : "false")},\"flash3\":{(flash3 ? "true" : "false")},\"pptAuto\":{(pptAuto ? "true" : "false")},\"showSlideInfo\":{(showSlideInfo ? "true" : "false")},\"sound\":{(sound ? "true" : "false")},\"opacity\":{opacityVal},\"fontSize\":{fontSize},\"allowNegative\":{(allowNegative ? "true" : "false")},\"pureSlide\":{(pureSlide ? "true" : "false")},\"slideInfo\":\"{EscapeJson(slideInfo)}\",\"setTime\":{setTime},\"startPauseReset\":{startPauseReset},\"foregroundInfo\":\"{EscapeJson(fgInfo)}\",\"screenCount\":{screenCount},\"screenIndex\":{screenIndex}}}";
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
                        if (path == "/toggle") _form1.Invoke(new Action(_form1.ToggleCountdown));
                        else if (path == "/reset") _form1.Invoke(new Action(_form1.ResetTimeFromRegistry));
                        else if (path == "/flash") _form1.Invoke(new Action(async () => await _form1.FlashWindowAsync()));
                        else if (path == "/settime")
                        {
                            if (int.TryParse(body, out int seconds))
                                _form1.Invoke(new Action(() => _form1.SetRemainingSeconds(seconds)));
                        }
                        else if (path == "/settotaltime")
                        {
                            if (int.TryParse(body, out int minutes) && minutes > 0)
                            {
                                if (minutes > int.MaxValue / 60)
                                    minutes = int.MaxValue / 60;
                                _form1.Invoke(new Action(() => _form1.SetTotalTimeMinutes(minutes)));
                            }
                        }
                        else if (path == "/setrighttop") _form1.Invoke(new Action(_form1.MoveToRightTop));
                        else if (path == "/setlefttop") _form1.Invoke(new Action(_form1.MoveToLeftTop));
                        else if (path == "/settopcenter") _form1.Invoke(new Action(_form1.MoveToTopCenter));
                        else if (path == "/setshowtimer") _form1.Invoke(new Action(() => _form1.ToggleTimerVisible()));
                        else if (path == "/setflash3") _form1.Invoke(new Action(() => _form1.ToggleFlash3()));
                        else if (path == "/moveup") _form1.Invoke(new Action(() => _form1.MoveBy(0, -20)));
                        else if (path == "/movedown") _form1.Invoke(new Action(() => _form1.MoveBy(0, 20)));
                        else if (path == "/moveleft") _form1.Invoke(new Action(() => _form1.MoveBy(-20, 0)));
                        else if (path == "/moveright") _form1.Invoke(new Action(() => _form1.MoveBy(20, 0)));
                        else if (path == "/togglesound") _form1.Invoke(new Action(_form1.ToggleSound));
                        else if (path == "/testsound") _form1.Invoke(new Action(_form1.TestSound));
                        else if (path == "/togglepptauto") _form1.Invoke(new Action(_form1.TogglePptAuto));
                        else if (path == "/toggleslideinfo") _form1.Invoke(new Action(_form1.ToggleShowSlideInfo));
                        else if (path == "/setfontsize")
                        {
                            if (int.TryParse(body, out int size))
                                _form1.Invoke(new Action(() => _form1.SetFontSize(size)));
                        }
                        else if (path == "/setopacity")
                        {
                            if (int.TryParse(body, out int val))
                                _form1.Invoke(new Action(() => _form1.SetOpacity(val)));
                        }
                        else if (path == "/toggleallownegative") _form1.Invoke(new Action(_form1.ToggleAllowNegative));
                        else if (path == "/togglepureslide") _form1.Invoke(new Action(_form1.TogglePureSlideMode));
                        else if (path == "/cyclescreen") _form1.Invoke(new Action(_form1.CycleScreen));
                    }
                    context.Response.StatusCode = 204;
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
                .Replace("{{foregroundInfo}}", fgInfo);
            return html;
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