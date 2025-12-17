using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using System.IO;
using System.ComponentModel;
using System.Threading;
using Numeria.IO;

namespace GuildQuest
{
    /// <summary>
    /// Логика взаимодействия для Result.xaml
    /// </summary>
    public partial class Archive : Window
    {
        public MainWindow core;
        public List<ResultData> copyData = new List<ResultData>();
        public string tab = "<TabItem Header=\"Clone\" Name=\"Clone\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DataGrid ColumnWidth=\"*\" IsReadOnly=\"True\" SelectionMode=\"Extended\" SelectionUnit=\"CellOrRowHeader\" AutoGenerateColumns=\"False\" HorizontalContentAlignment=\"Stretch\" Name=\"grid\" HorizontalAlignment=\"Stretch\" VerticalAlignment=\"Stretch\" AllowDrop=\"False\"><DataGrid.Columns><DataGridTextColumn CanUserResize=\"True\" Width=\"150\" IsReadOnly=\"True\" CanUserSort=\"True\" Binding=\"{Binding Path=Username}\" Header=\"Member\"/><DataGridTextColumn CanUserResize=\"True\" Width=\"100\" IsReadOnly=\"True\" CanUserSort=\"True\" Binding=\"{Binding Path=Since, StringFormat=\\{0:dd.MM.yyyy\\}}\" Header=\"Since\"/><DataGridTextColumn CanUserResize=\"True\" Width=\"60\" IsReadOnly=\"True\" CanUserSort=\"True\" Binding=\"{Binding Path=Level}\" Header=\"Level\"/><DataGridTextColumn CanUserResize=\"True\" CanUserSort=\"True\" IsReadOnly=\"True\" Binding=\"{Binding Path=Quest}\" Header=\"Quest\"/><DataGridTextColumn CanUserResize=\"True\" CanUserSort=\"True\" IsReadOnly=\"True\" Binding=\"{Binding Path=SubQuest}\" Header=\"SubQuest\"><DataGridTextColumn.ElementStyle><Style TargetType=\"TextBlock\"><Setter Property=\"ToolTip\" Value=\"{Binding Path=SbTrans}\" /></Style></DataGridTextColumn.ElementStyle></DataGridTextColumn><DataGridTextColumn CanUserResize=\"True\" Width=\"100\" IsReadOnly=\"True\" CanUserSort=\"True\" Binding=\"{Binding Path=Status}\" Header=\"Status\"/></DataGrid.Columns></DataGrid></TabItem>";
        private FileDB _DB;
        private List<Guid> _guids = new List<Guid>();
        private int total, pos;
        private bool update = false;

        public Archive()
        {
            InitializeComponent();
            Init();
            comboBox1.SelectionChanged += new SelectionChangedEventHandler(comboBox1_SelectionChanged);
        }

        private void Init()
        {
            _guids.Clear();
            comboBox1.Items.Clear();
            _DB = new FileDB("archive.db", FileAccess.ReadWrite);
            foreach (EntryInfo etr in _DB.ListFiles())
            {
                _guids.Add(etr.ID);
                comboBox1.Items.Add(new ComboBoxItem() { Content = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(double.Parse(etr.FileName.Replace(".", ""))).ToString() });
            }
            total = pos = _guids.Count;
            
            if (total == 0)
            {
                DialogResult = true;
                return;
            }
            page.Text = string.Format("{0}/{1}", pos, total);
            comboBox1.SelectedIndex = pos - 1;
            getData(_guids.Last());
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

        private void Clear()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                List<TabItem> toDel = new List<TabItem>();
                toDel.Clear();
                foreach (TabItem tab in tabControl1.Items)
                {
                    if (tab.Header.ToString() != "Квест " && tab.Header.ToString() != "Clone")
                        toDel.Add(tab);
                }
                foreach (TabItem tab in toDel)
                    tabControl1.Items.Remove(tab);
                tabControl1.Items.Refresh();
            }));
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime pDate = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(unixTimeStamp);
            return pDate;
        }
        public void getData(Guid gid)
        {
            
            MemoryStream stream = new MemoryStream();
            EntryInfo info = _DB.Read(gid, stream);
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                crdate.Text = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(double.Parse(info.FileName.Replace(".", ""))).ToString();
                guid.Text = gid.ToString();
            }));
            string data = UTF8Encoding.UTF8.GetString(ReadFully(stream));
            stream.Close();
            SortedDictionary<int, List<ResultData>> resData = new SortedDictionary<int, List<ResultData>>(new DescendingComparer<int>());
            resData.Clear();
            string decrypted = new Crypt().Decrypt(data, true);
            foreach (string line in decrypted.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] entry = line.Split(new[] { ';' }, StringSplitOptions.None);
                string status, mainquest, subquest, sbtrans = "";
                status = (entry[5] != "") ? getStatus(int.Parse(entry[5])) : "";
                DateTime since = (entry.Count() > 5 && entry[6] != "") ? UnixTimeStampToDateTime(Double.Parse(entry[6])) : new DateTime(1970, 1, 1, 0, 0, 0, 0);
                if (entry[4] == "none")
                {
                    mainquest = "Нет квеста";
                    subquest = "";
                    sbtrans = "Нет информации";
                }
                else
                {
                    mainquest = (Trans.lang.ContainsKey(entry[4].Substring(0, entry[4].Length - 4))) ? Trans.lang[entry[4].Substring(0, entry[4].Length - 4)] : entry[4].Substring(0, entry[4].Length - 4);
                    subquest = (Trans.lang.ContainsKey(entry[4])) ? Trans.lang[entry[4]] : entry[4];
                    sbtrans = (Trans.langSub.ContainsKey(entry[4])) ? Trans.langSub[entry[4]] : "Нет информации";
                }
                ResultData tmp = new ResultData() { qID = int.Parse(entry[0]), Username = entry[1], Since = since, Level = entry[3], Quest = mainquest, SubQuest = subquest, Status = status, SbTrans = sbtrans };
                copyData.Add(tmp);
                if (!resData.ContainsKey(int.Parse(entry[0])))
                {
                    resData.Add(int.Parse(entry[0]), new List<ResultData>() { tmp });
                }
                else
                {
                    resData[int.Parse(entry[0])].Add(tmp);
                }
            }
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                foreach (KeyValuePair<int, List<ResultData>> item in resData)
                {
                    AddNewTab(item.Key.ToString());
                    setSourse(item.Key.ToString(), item.Value);
                }
                selectFirst();
            }));
        }
        #region other
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

        public TabItem GetTabByHeader(string Header)
        {
            IEnumerable<TabItem> main = tabControl1.Items.OfType<TabItem>().Where(tab => tab.Header != null && tab.Header.ToString().ToLowerInvariant().Contains(Header.ToLowerInvariant()));
            return main.First();
        }

        public DataGrid GetGrid(string Header)
        {
            TabItem Tab = GetTabByHeader(Header) as TabItem;
            return GetLogicalChildCollection<DataGrid>(Tab)[0] as DataGrid;
        }

        public void setHeader(string Header)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                TabItem tab = GetTabByHeader(Header);
                tab.Header = Header;
            }));
        }

        public void setSourse(string tab, List<ResultData> data)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                DataGrid grid = GetGrid(tab);
                grid.LoadingRow += new EventHandler<DataGridRowEventArgs>(MyGrid_LoadingRow);
                foreach (ResultData item in data) grid.Items.Add(item);
                applySort(grid);
                foreach (ResultData item in data)
                {
                    GetTabByHeader(tab).Header = tab + " - " + item.Quest;
                    if (item.Quest != "Нет квеста") break;
                }
            }));
        }

        public void setSourse(string tab, ObservableCollection<ResultData> data)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                GetGrid(tab).ItemsSource = data;
                applySort(GetGrid(tab));
                foreach (ResultData item in data)
                {
                    GetTabByHeader(tab).Header = tab + " - " + item.Quest;
                    if (item.Quest != "Нет квеста")
                    {
                        break;
                    }
                }
            }));
        }

        public void AddNewTab(string Header)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {

                TabItem NewTab = XamlReader.Parse(tab) as TabItem;
                NewTab.Header = Header;
                NewTab.Visibility = Visibility.Visible;
                NewTab.IsEnabled = true;
                tabControl1.Items.Add(NewTab);
                GetGrid(Header).CurrentCellChanged += new EventHandler<EventArgs>(grid_CurrentCellChanged);
            }));
        }

        public static List<T> GetLogicalChildCollection<T>(object parent) where T : DependencyObject
        {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
            return logicalCollection;
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            foreach (object child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject)
                {
                    if (child is T) logicalCollection.Add(child as T);
                    GetLogicalChildCollection((DependencyObject)child, logicalCollection);
                }
            }
        }

        public void applySort(DataGrid grid)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(grid.Items);
                view.SortDescriptions.Clear();
                SortDescription sd = new SortDescription("Status", ListSortDirection.Descending);
                view.SortDescriptions.Add(sd);
                sd = new SortDescription("Level", ListSortDirection.Descending);
                view.SortDescriptions.Add(sd);
                sd = new SortDescription("Quest", ListSortDirection.Ascending);
                view.SortDescriptions.Add(sd);
                view.Refresh();
            }));
        }

        public void selectFirst()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                tabControl1.SelectedIndex = 2;
            }));
        }

        [STAThread]
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread clip = new Thread(new ParameterizedThreadStart(setClipBoard)) { IsBackground = true };
            clip.SetApartmentState(ApartmentState.STA);
            string text = "";
            foreach (object item in copyData)
            {
                if (item is GuildQuest.ResultData)
                {
                    GuildQuest.ResultData data = item as GuildQuest.ResultData;
                    text += string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\r\n", data.qID, data.Username, data.Level, data.Quest, data.SubQuest, data.Status);

                }
            }
            clip.Start(text);
        }

        private void MyGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string text = "№ Квеста;Имя пользователя;Уровень;Квест;Подквест;Статус\n";
            foreach (object item in copyData)
            {
                if (item is GuildQuest.ResultData)
                {
                    GuildQuest.ResultData data = item as GuildQuest.ResultData;
                    text += string.Format("{0};{1};{2};{3};{4};{5}\n", data.qID, data.Username, data.Level, data.Quest, data.SubQuest, data.Status);
                }
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Text documents (.csv)|*.csv";
            dlg.FileName = "guildquest_" + DateTime.Now.ToString("dd-MM-yyy_HH-mm");
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                System.IO.File.WriteAllText(filename, text, Encoding.GetEncoding(1251));
                MessageBox.Show("Сохранено");
            }
        }

        private void setClipBoard(object data)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetText(data.ToString());
                    MessageBox.Show("Содержимое помещено в буфер обмена");
                    break;
                }
                catch
                {
                    Thread.Sleep(100); continue;
                }
            }
        }

        private void grid_CurrentCellChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("changed");
            //(sender as DataGrid).ToolTip = "ololo";
            //((sender as DataGrid).ToolTip as ToolTip).IsOpen = true;
            
            //(sender as DataGrid).c
            //textBlock1.Text = (sender as DataGrid).ToolTip.ToString();
        }
        #endregion
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedIndex == -1 || update)
                return;
            Clear();
            pos = comboBox1.SelectedIndex+1;
            page.Text = pos + "/" + total;
            getData(_guids[comboBox1.SelectedIndex]);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            comboBox1.SelectedIndex = -1;
            update = true;
            _DB.Delete(_guids[pos-1]);
            _DB.Shrink();
            _DB.Dispose();
            Clear();
            Init();
            update = false;
        }

        private void grid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}
