using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using FluorineFx;
using FluorineFx.Net;
using FluorineFx.IO;
using FluorineFx.Messaging.Messages;
using FluorineFx.Attributes;
using FluorineFx.Messaging.Api.Service;
using GuildQuest.VO.Guild;
using GuildQuest.VO;
using System.Web.Script.Serialization;
using Numeria.IO;

namespace GuildQuest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string username { get; set; }
        public string password { get; set; }
        public string _authUser { get; set; }
        public string _authToken { get; set; }
        public string _flexEndpoint { get; set; }
        public string bbServer { get; set; }
        public string Nickname { get; set; }
        public int attepts = 0;
        public bool compact = false;
        public bool batchMode = false;
        public bool doLog = false;
        public string csvfile = string.Empty;
        public bool noArch = false;
        public CookieCollection _cookies = new CookieCollection();
        public NetConnection _amfConnection;
        private AmfResponceParcer _parcer = new AmfResponceParcer();
        public string region = "RU";
        private string[] help = new string[] {
            "Доступные агрументы:",
            "  --help     - эта справка",
            "  --user     - логин (сохраняется в settings)",
            "  --pass     - пароль (сохраняется в settings)",
            "  --auto     - автовход",
            "  --batch    - создание scv и выход",
            "  --csvfile  - имя csv файла для записи",
            "  --noarch   - не добавлять данные в архив",
            "  --log        - вести лог в batch режиме\n",
            "Формат csvfile по умолчанию: guildquest_dd-MM-yyy_HH-mm",
            "Пример: \nGuildQuest.exe --user=some@mail.ru --pass=ololo --auto --batch\n",
            "Если в файле settings уже есть сохраненные данные то в аргументах их можно не указывать.",
            "Пример: \nGuildQuest.exe --auto запустит утилиту с автовходом и учетными данными из settings"
        };
        public FileDB _DB;

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            InitializeComponent();
            Registrator();
            System.Net.ServicePointManager.Expect100Continue = false;
            System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
            _parcer.core = this;
            if (File.Exists("settings.dat"))
            {
                string[] settings = new Crypt().Decrypt(File.ReadAllText("settings.dat"), true).Split(new[] { '|' }, StringSplitOptions.None);
                if (settings.Length > 1)
                {
                    user.Text = settings[0].Trim();
                    pass.Password = settings[1].Trim();
                    if (settings.Length > 2)
                    {
                        checkBox1.IsChecked = bool.Parse(settings[2].Trim());
                        compact = bool.Parse(settings[2].Trim());
                    }
                }
            }
            if (App.cmd["help"] != null)
            {
                MessageBox.Show(string.Join("\n", help),"Info", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            }
            if (App.cmd["user"] != null)
                    user.Text = App.cmd["user"];
            if (App.cmd["pass"] != null)
                pass.Password = App.cmd["pass"];
            if (App.cmd["batch"] != null)
                batchMode = true;
            if (App.cmd["noarch"] != null)
                noArch = true;
            if (App.cmd["csvfile"] != null)
                csvfile = App.cmd["csvfile"];
            if (App.cmd["log"] != null)
                doLog = true;
            if (App.cmd["auto"] != null)
                Button_Click(button, null);
            if (!checkArchive())
                AddToRich("Файл архива поврежден. Будет создан новый файл");
            _DB = new FileDB("archive.db", FileAccess.ReadWrite);
            if (_DB.ListFiles().Length > 0)
            {
                arch_over.Visibility = System.Windows.Visibility.Collapsed;
                AddToRich("Записей в архиве " + _DB.ListFiles().Length);
            }
            _DB.Dispose();
         }

        private bool checkArchive()
        {
            bool result = true;
            if (!File.Exists("archive.db")) return true;
            FileStream _fileStream = new FileStream("archive.db", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.None);
            BinaryReader _reader = new BinaryReader((Stream)_fileStream);
            _reader.BaseStream.Seek(0L, SeekOrigin.Begin);
            if (BinaryReaderExtensions.ReadString(_reader, "FileDB".Length) != "FileDB") result = false;
            if ((int)_reader.ReadInt16() != 1) result = false;
            _reader.Close();
            _fileStream.Close();
            if (!result) File.Delete("archive.db");
            return result;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ExceptionDumper.DumpException(e.ExceptionObject as Exception);
        }

        public void GetData()
        {
            _amfConnection = new NetConnection();
            _amfConnection.CookieContainer.Add(_cookies);
            _amfConnection.ObjectEncoding = ObjectEncoding.AMF3;
            _amfConnection.OnDisconnect += onDisconnect;
            //_amfConnection.NetStatus += onStatus;
            _amfConnection.Connect(_flexEndpoint);
            _amfConnection.Call("", string.Empty, "", "5", _parcer, null);
            AddToRich("Соединение с геймсервером");
            int attempt = 0;
            while (!_parcer.connected)
            {
                if (attempt > 5)
                {
                    return;
                }
                attempt++;
                Thread.Sleep(1000);
            }
            AddToRich("Запрос данных");
            _amfConnection.Call("SMC-Endpoint", "GUILD", "com.bluebyte.game.servlet.GuildHandler", "GetGuildOwn", _parcer, getMessage(4014, ""));
        }

        public SettlersServerCall getMessage(int type, object data = null, int ZoneId = 0)
        {
            SettlersServerCall message = new SettlersServerCall(type, data);
            message.dsoAuthUser = int.Parse(_authUser);
            message.dsoAuthToken = _authToken;
            message.zoneID = 0;
            Thread.Sleep(200);
            return message;
        }

        private void onStatus(object sender, NetStatusEventArgs e)
        {
            AddToRich("ERROR!: " + e.Exception.Message + "\n" + e.Exception.StackTrace);
        }

        private void onDisconnect(object sender, EventArgs e)
        {
            AddToRich("AMF Disconnect: " + e.ToString());
        }

        public void MainAuth()
        {
            _cookies = new CookieCollection();
            _authToken = "";
            _authUser = "";
            attepts++;
            try
            {
                PostSubmitter post;
                string res;
                if (attepts > 5)
                {
                    AddToRich("Хватит пытаться :)");
                    setButtonOn();
                    return;
                }
                AddToRich("Попытка авторизации #" + attepts++);

                post = new PostSubmitter
                {
                    Url = "https://public-ubiservices.ubi.com/v3/profiles/sessions",
                    Type = PostSubmitter.PostTypeEnum.Post
                };
                post.HeaderItems.Add("Authorization", "Basic " + Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username.Trim(), password.Trim()))));
                post.ContentType = "application/json";
                post.HeaderItems.Add("Ubi-AppId", "39164658-8187-4bf4-b46c-375f68356e3b");
                res = post.Post(ref _cookies);
                JavaScriptSerializer jc = new JavaScriptSerializer();
                var obj = jc.DeserializeObject(res) as Dictionary<string, object>;
                if (obj == null) throw new Exception("General failure..");
                if ("uplay" == (string)obj["platformType"])
                {
                    AddToRich("Успешная авторизация");
                    post = new PostSubmitter
                    {
                        Url = "https://www.thesettlersonline.ru/ru/api/user/uplay",
                        Type = PostSubmitter.PostTypeEnum.Post
                    };
                    DateTime dt = DateTime.Parse((string)obj["expiration"]).ToUniversalTime();
                    Int32 unixTimestamp = (Int32)(dt.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    post.PostItems.Add("id", (string)obj["userId"]);
                    post.PostItems.Add("ticket", (string)obj["ticket"]);
                    post.PostItems.Add("expiration", unixTimestamp.ToString());
                    post.PostItems.Add("undefined", (string)obj["sessionId"]);
                    post.PostItems.Add("activated", "true");
                    res = post.Post(ref _cookies);
                    if (!res.Contains("OKAY"))
                    {
                        AddToRich("Ошибка авторизации :'(" + res);
                        MainAuth();
                        return;
                    } else { 
                        post = new PostSubmitter
                        {
                            Url = "https://www.thesettlersonline.ru/ru/play",
                            Type = PostSubmitter.PostTypeEnum.Get
                        };
                        res = post.Post(ref _cookies);
                        Match match = Regex.Match(res, "dsoAuthToken=([a-zA-Z0-9]+)", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            _authToken = match.Groups[1].Value.Trim();
                            match = Regex.Match(res, "dsoAuthUser=([0-9]+)", RegexOptions.IgnoreCase);
                            _authUser = match.Groups[1].Value.Trim();
                            if (!prepare(res))
                            {
                                AddToRich("Ошибка получения параметров");
                                MainAuth();
                                return;
                            }
                        } else
                        {
                            AddToRich("Ошибка получения параметров");
                            MainAuth();
                            return;
                        }
                    }
                    if (null == _authToken || null == _authUser)
                    {
                        AddToRich("Не смогли получить печеньки :'(");
                        MainAuth();
                        return;
                    }
                    else
                    {
                        AddToRich("Печеньки получены и съедены :)");
                        CookieCollection nc = new CookieCollection();
                        foreach(Cookie c in _cookies)
                        {
                            c.Secure = false;
                            nc.Add(c);
                        }
                        nc.Add(new Cookie("dsoAuthUser", _authUser, "/", ".thesettlersonline.ru"));
                        nc.Add(new Cookie("dsoAuthToken", _authToken, "/", ".thesettlersonline.ru"));
                        int _endpointWait = 0;
                        while (_endpointWait < 4)
                        {
                            _endpointWait++;
                            post = new PostSubmitter
                            {
                                Url = string.Format("{0}Z{1}", (region == "RU") ? string.Format("https://{0}.thesettlersonline.ru/", bbServer) : string.Format("https://{0}.diesiedleronline.de/", bbServer), GetFlashTime()),
                                Type = PostSubmitter.PostTypeEnum.Post
                            };
                            post.PostItems.Add("dsoAuthUser", _authUser);
                            post.PostItems.Add("dsoAuthToken", _authToken);
                            post.PostItems.Add("zoneID", "0");
                            res = post.Post(ref nc);
                            string cookieName = (region == "RU") ? "thesettlersonline" : "diesiedleronline";
                            if (res.Contains(cookieName))
                            {
                                _flexEndpoint = res.Trim().Replace(":443", "");
                                AddToRich("Адрес геймсервера получен.");
                                _parcer.Nickname = Nickname;
                                try
                                {
                                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                                    {
                                        File.WriteAllText("settings.dat", new Crypt().Encrypt(string.Format("{0}|{1}|{2}", user.Text, pass.Password, checkBox1.IsChecked.ToString()), true));
                                    }));
                                }
                                catch { }
                                GetData();
                                return;
                            }
                            else
                            {
                                AddToRich("Ждем адрес...");
                                Thread.Sleep(1000);
                            }
                        }
                        AddToRich("Не дождались :)");
                        setButtonOn();
                        return;
                    }
                }
                else
                {
                    if (res.Contains("FAILED"))
                    {
                        AddToRich("Логин/пароль неверны.");
                        setButtonOn();
                        return;
                    }
                    if (res.Contains("UPLAY"))
                    {
                        AddToRich("UPLAY не отвечает.");
                        MainAuth();
                    }
                    if (res.Contains("EXCEPTION"))
                    {
                        AddToRich("Ошибка на странице авторизации.");
                        MainAuth();
                    }
                    if (res.Trim() == "") AddToRich("Хм, пустой ответ.. странно");
                    else AddToRich("Ошибка авторизации. ответ - " + res);
                    MainAuth();
                }
            }
            catch (Exception e)
            {
                AddToRich("Общая ошибка " + e.Message);
                setButtonOn();
                return;
            }
            return;
        }

        public bool prepare(string htmlPage)
        {
            AddToRich("Получаем параметры.");
            Match match;
            match = Regex.Match(htmlPage, "loggedInUserName = '([А-Яа-яёЁйЙA-Za-z0-9_-]+)';", RegexOptions.IgnoreCase); 
            if (match.Success)
            {
                Nickname = match.Groups[1].Value.Trim();
                AddToRich("Ник игрока - " + Nickname);
            }
            else return false;
            match = Regex.Match(htmlPage, @"w\d{1,3}bb\d{1,3}", RegexOptions.IgnoreCase);
            if (!match.Success) return false;
            bbServer = match.Groups[0].Value;
            AddToRich("BB сервер получен " + bbServer);
            return true;
        }

        public double GetFlashTime()
        {
            return Math.Round((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds, 0);
        }

        public void AddToRich(string message)
        {
            if (batchMode == true && doLog == true)
            {
                string filename = "guildquest_" + DateTime.Now.ToString("dd-MM-yyy") + ".log";
                using (StreamWriter outputFile = new StreamWriter(filename, true))
                {
                    outputFile.WriteLine(DateTime.Now.ToString("dd-MM-yyy HH:mm") + ": " + message);
                }
            }
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                richTextBox1.AppendText(message + "\r");
            }));
        }

        public void setButtonOn()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                if (batchMode == true)
                    Application.Current.Shutdown(0);
                login_over.Visibility = System.Windows.Visibility.Collapsed;
            }));
        }

        public static int getTimeStamp()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private void richTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            richTextBox1.ScrollToEnd();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            login_over.Visibility = System.Windows.Visibility.Visible;
            string error_msg = string.Empty;
            if (string.IsNullOrEmpty(user.Text.Trim())) error_msg = "Логин пуст.";
            if (string.IsNullOrEmpty(pass.Password.Trim())) error_msg = "Пароль пуст.";
            if (!string.IsNullOrEmpty(error_msg))
            {
                AddToRich(error_msg);
                setButtonOn();
                return;
            }
            username = user.Text;
            password = pass.Password;
            attepts = 0;
            new Thread(MainAuth) { IsBackground = true }.Start();
        }

        public void Registrator()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attrs = type.GetCustomAttributes(typeof(AmfObjectName), false);
                if (null == attrs || 0 == attrs.Length) continue;
                ObjectFactory.AddToLocate(type, AmfObjectName.DefaultPrefix + ((AmfObjectName)attrs[0]).Name);
            }
        }

        private void pass_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                Button_Click(button, null);
        }

        private void ach_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
            Archive arch = new Archive() { WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner, Owner = this };
            arch.ShowDialog();
            this.Visibility = System.Windows.Visibility.Visible;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

    class AmfResponceParcer : IPendingServiceCallback
    {
        public string Nickname { get; set; }
        public bool compact = false;
        public bool connected = false;
        public MainWindow core;
        public int zoneID = 0;
        List<ResultData> data = new List<ResultData>();
        public static Dictionary<int, int> GuildRanks = new Dictionary<int, int>();
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime pDate = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(unixTimeStamp);
            return pDate;
        }
        public void ResultReceived(IPendingServiceCall call)
        {
            // init packet
            if (call.ServiceMethodName == "5")
            {
                connected = true;
                return;
            } 
            foreach (AMFBody Body in call.Result as List<AMFBody>)
            {
                SettlersServerResponse result = (Body.Content as AcknowledgeMessage).body as SettlersServerResponse;
                SettlersServerActionResult action = result.data as SettlersServerActionResult;
                if (!(action.data is SettlersGuildVO))
                {
                    core.AddToRich("Вы не состоите в гильдии");
                    core.setButtonOn();
                    return;
                }
                core.Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    compact = (bool)core.checkBox1.IsChecked;
                }));
                SettlersGuildVO guild = action.data as SettlersGuildVO;
                GuildRanks.Clear();
                foreach (SettlersGuildRankListItemVO rank in guild.ranks) GuildRanks.Add(rank.id, rank.position);
                SortedDictionary<int, List<ResultData>> resData = new SortedDictionary<int, List<ResultData>>(new DescendingComparer<int>());
                SortedDictionary<int, List<ResultData>> archData = new SortedDictionary<int, List<ResultData>>(new AscendingComparer<int>());
                List<int> qList = new List<int>();
                List<int> mainQuest = new List<int>();
                foreach (SettlersGuildPlayerListItemVO player in guild.members)
                {
                    if (player.quest.uniqueID == 0)
                    {
                        core.AddToRich("Странный квест с ID = 0. Пропускаем");
                        continue;
                    }
                    if (!qList.Contains(player.quest.uniqueID)) qList.Add(player.quest.uniqueID);
                    if (!mainQuest.Contains(player.quest.uniqueID) && player.quest.status < 3) mainQuest.Add(player.quest.uniqueID);
                }
                foreach (int qID in qList)
                {
                    foreach (SettlersGuildPlayerListItemVO player in guild.members)
                    {
                        bool found = false;
                        int qstatus = 0;
                        string qName = string.Empty;
                        string since = player.friendSince.ToString().Substring(0, 10);
                        DateTime friendSince = UnixTimeStampToDateTime(Double.Parse(since));
                        if (player.quest.uniqueID == qID)
                        {
                            found = true;
                            qstatus = player.quest.status;
                            qName = player.quest.questname;
                        }
                        ResultData tmp, tmp2;
                        if (found)
                        {
                            string status = getStatus(qstatus);
                            string mainquest = (Trans.lang.ContainsKey(qName.Substring(0, qName.Length - 4))) ? Trans.lang[qName.Substring(0, qName.Length - 4)] : qName.Substring(0, qName.Length - 4);
                            string subquest = (Trans.lang.ContainsKey(qName)) ? Trans.lang[qName] : qName;
                            string sbtrans = (Trans.langSub.ContainsKey(qName)) ? Trans.langSub[qName] : "Нет информации";
                            tmp = new ResultData() { qID = qID, Username = player.username, Since = friendSince, Level = player.playerLevel.ToString(), Quest = mainquest, SubQuest = subquest, Status = status, SbTrans = sbtrans };
                            tmp2 = new ResultData() { qID = qID, Username = player.username, SinceString = since, Level = player.playerLevel.ToString(), Quest = qName, Status = qstatus.ToString() };
                        }
                        else
                        {
                            tmp = new ResultData() { qID = qID, Username = player.username, Since = friendSince, Quest = "Нет квеста", SubQuest = string.Empty, Level = player.playerLevel.ToString(), Status = string.Empty, SbTrans = "Нет информации" };
                            tmp2 = new ResultData() { qID = qID, Username = player.username, SinceString = since, Level = player.playerLevel.ToString(), Quest = "none" };
                        }
                        if (compact && !mainQuest.Contains(qID)) continue;
                        if (!mainQuest.Contains(qID) && tmp.Quest == "Нет квеста")
                                continue;

                        tmp.Active = (player.onlineLast24 == true) ? "Да" : "Нет";
                        tmp2.Active = (player.onlineLast24 == true) ? "Да" : "Нет";
                        data.Add(tmp);
                        
                        if (resData.ContainsKey(qID))
                        {
                            resData[qID].Add(tmp);
                            archData[qID].Add(tmp2);
                        }
                        else
                        {
                            resData.Add(qID, new List<ResultData>() { tmp });
                            archData.Add(qID, new List<ResultData>() { tmp2 });
                        }
                    }
                }
                core.AddToRich("Данные получены");
                if (resData.Count == 0)
                {
                    core.AddToRich("Квесты не найдены. Нечего отображать");
                    core.setButtonOn();
                    return;
                }
                if (core.noArch == false)
                {
                    using (FileDB _DDB = new FileDB("archive.db", FileAccess.ReadWrite))
                    {
                        string text = "";
                        foreach (KeyValuePair<int, List<ResultData>> entry in archData)
                        {
                            foreach (ResultData item in entry.Value)
                                text += string.Format("{0};{1};{2};{3};{4};{5};{6}\n", entry.Key, item.Username, item.Active, item.Level, item.Quest, item.Status, item.SinceString);
                        }
                        string lastData = "";
                        if (_DDB.ListFiles().Length > 0)
                        {
                            MemoryStream stream = new MemoryStream();
                            _DDB.Read(_DDB.ListFiles().Last().ID, stream);
                            lastData = new Crypt().Decrypt(UTF8Encoding.UTF8.GetString(ReadFully(stream)), true);
                            stream.Close();
                        }
                        if (lastData.Length != text.Length)
                        {
                            using (MemoryStream outstream = new MemoryStream())
                            {
                                text = new Crypt().Encrypt(text, true);
                                outstream.Write(UTF8Encoding.UTF8.GetBytes(text), 0, UTF8Encoding.UTF8.GetBytes(text).Length);
                                outstream.Position = 0;
                                _DDB.Store(getTimeStamp().ToString(), outstream);
                            }
                        }
                    }
                }
                if (core.batchMode == true)
                    saveCsv(data);
                core.setButtonOn();
                core.Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    core.Visibility = Visibility.Hidden;
                    Result resw = new Result() { WindowStartupLocation = WindowStartupLocation.CenterScreen, core = core, copyData = data };
                    foreach (KeyValuePair<int, List<ResultData>> item in resData)
                    {
                        resw.AddNewTab(item.Key.ToString());
                        resw.setSourse(item.Key.ToString(), item.Value);
                    }
                    resw.selectFirst();
                    resw.ShowDialog();
                    Environment.Exit(1);
                }));
            }
        }

        public void saveCsv(List<ResultData> dataList)
        {
            string text = "№ Квеста;Имя пользователя;Дата вступления;Уровень;Активен;Квест;Подквест;Статус\n";
            foreach (object item in dataList)
            {
                if (item is GuildQuest.ResultData)
                {
                    GuildQuest.ResultData data = item as GuildQuest.ResultData;
                    text += string.Format("{0};{1};{2};{3};{4};{5};{6};{7}\n", data.qID, data.Username, data.Since.ToString("dd.MM.yyyy"), data.Level, data.Active, data.Quest, data.SubQuest, data.Status);
                }
            }
            string filename = (string.IsNullOrEmpty(core.csvfile)) ? "guildquest_" + DateTime.Now.ToString("dd-MM-yyy_HH-mm") + ".csv" : core.csvfile + ".csv";
            System.IO.File.WriteAllText(filename, text, Encoding.GetEncoding(1251));
        }

        public static byte[] ReadFully(Stream input)
        {
            input.Position = 0;
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static int getTimeStamp()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public string getStatus(int sts)
        {
            string status;
            switch (sts)
            {
                case 0:
                    status = "Предстоящее";
                    break;
                case 1:
                case 2:
                    status = "Выполняется";
                    break;
                case 3:
                    status = "Завершен";
                    break;
                case 4:
                    status = "Не активен";
                    break;
                default:
                    status = "Неизвестно";
                    break;
            }
            return status;
        }
    }
    class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T x, T y)
        {
            return y.CompareTo(x);
        }
    }

    class AscendingComparer<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T x, T y)
        {
            return x.CompareTo(y);
        }
    }
    public class ResultData
    {
        public int qID { get; set; }
        public string Username { get; set; }
        public DateTime Since { get; set; }
        public string SinceString { get; set; }
        public string Level { get; set; }
        public string Quest { get; set; }
        public string SubQuest { get; set; }
        public string Status { get; set; }
        public string SbTrans { get; set; }
        public string Active { get; set; }
    }

    internal static class BinaryReaderExtensions
    {
        public static string ReadString(this BinaryReader reader, int size)
        {
            return Encoding.UTF8.GetString(reader.ReadBytes(size)).Replace(char.MinValue, ' ').Trim();
        }

        public static Guid ReadGuid(this BinaryReader reader)
        {
            return new Guid(reader.ReadBytes(16));
        }

        public static DateTime ReadDateTime(this BinaryReader reader)
        {
            return new DateTime(reader.ReadInt64());
        }

        public static long Seek(this BinaryReader reader, long position)
        {
            return reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }
    }
}
