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
using System.ComponentModel;
using System.Threading;

namespace GuildQuest
{
    /// <summary>
    /// Логика взаимодействия для Result.xaml
    /// </summary>
    public partial class Result : Window
    {
        public MainWindow core;
        public List<ResultData> copyData { get; set; }
        public string tab = "<TabItem Header=\"Clone\" Name=\"Clone\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DataGrid ColumnWidth=\"*\" IsReadOnly=\"True\" SelectionMode=\"Extended\" SelectionUnit=\"CellOrRowHeader\" AutoGenerateColumns=\"False\" HorizontalContentAlignment=\"Stretch\" Name=\"grid\" HorizontalAlignment=\"Stretch\" VerticalAlignment=\"Stretch\" AllowDrop=\"False\"><DataGrid.Columns><DataGridTextColumn CanUserResize=\"True\" Width=\"50\" IsReadOnly=\"True\" CanUserSort=\"False\" Binding=\"{Binding Path=qID}\" Header=\"#\"/><DataGridTextColumn CanUserResize=\"True\" Width=\"100\" IsReadOnly=\"True\" CanUserSort=\"True\" Binding=\"{Binding Path=Username}\" Header=\"Member\"/><DataGridTextColumn CanUserResize=\"True\" Width=\"100\" IsReadOnly=\"True\" CanUserSort=\"True\" Binding=\"{Binding Path=Since, StringFormat=\\{0:dd.MM.yyyy\\}}\" Header=\"Since\"/><DataGridTextColumn CanUserResize=\"True\" Width=\"60\" IsReadOnly=\"True\" CanUserSort=\"True\" Binding=\"{Binding Path=Level}\" Header=\"Level\"/><DataGridTextColumn CanUserResize=\"True\" Width=\"60\" IsReadOnly=\"True\" CanUserSort=\"True\" Binding=\"{Binding Path=Active}\" Header=\"Active\"/><DataGridTextColumn CanUserResize=\"True\" CanUserSort=\"True\" IsReadOnly=\"True\" Binding=\"{Binding Path=Quest}\" Header=\"Quest\"/><DataGridTextColumn CanUserResize=\"True\" CanUserSort=\"True\" IsReadOnly=\"True\" Binding=\"{Binding Path=SubQuest}\" Header=\"SubQuest\"><DataGridTextColumn.ElementStyle><Style TargetType=\"TextBlock\"><Setter Property=\"ToolTip\" Value=\"{Binding Path=SbTrans}\" /></Style></DataGridTextColumn.ElementStyle></DataGridTextColumn><DataGridTextColumn CanUserResize=\"True\" Width=\"100\" IsReadOnly=\"True\" CanUserSort=\"True\" Binding=\"{Binding Path=Status}\" Header=\"Status\"/></DataGrid.Columns></DataGrid></TabItem>";

        public Result()
        {
            InitializeComponent();
            
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
        private void MyGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
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
                    if (item.Quest != "Нет квеста")
                        break;
                }
                
            }));
        }

        public void setSourse(string tab, ObservableCollection<ResultData> data)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                GetGrid(tab).ItemsSource = data;
                //GetGrid(tab).Columns[2].ValueType = typeof(DateTime);
                applySort(GetGrid(tab));
                foreach (ResultData item in data)
                {
                    GetTabByHeader(tab).Header = tab + " - " + item.Quest;
                    if (item.Quest != "Нет квеста")
                        break;
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
                    text += string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\r\n", data.qID, data.Username, data.Since.ToString("dd.MM.yyyy"), data.Level, data.Active, data.Quest, data.SubQuest, data.Status);

                }
            }
            clip.Start(text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string text = "№ Квеста;Имя пользователя;Дата вступления;Уровень;Активен;Квест;Подквест;Статус\n";
            foreach (object item in copyData)
            {
                if (item is GuildQuest.ResultData)
                {
                    GuildQuest.ResultData data = item as GuildQuest.ResultData;
                    text += string.Format("{0};{1};{2};{3};{4};{5};{6};{7}\n", data.qID, data.Username, data.Since.ToString("dd.MM.yyyy"), data.Level, data.Active, data.Quest, data.SubQuest, data.Status);
                }
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Text documents (.csv)|*.csv";
            dlg.FileName = "guildquest_"+DateTime.Now.ToString("dd-MM-yyy_HH-mm");
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
                    Clipboard.SetData("UnicodeText", data);
                    MessageBox.Show("Содержимое помещено в буфер обмена");
                    break;
                }
                catch
                {
                    Thread.Sleep(100); continue;
                }
            }
        }
    }
}
