using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Packaging;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OpcUaClient.Entity;
using System.Configuration;
using System.Diagnostics;
using Opc.Ua;
using Opc.Ua.Client;

namespace OpcUaClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpcUaClientApi _uaServer = new OpcUaClientApi();
        Subscription _subscription = null;

        Dictionary<string, List<ReferenceDescription>> _valueNodes = new Dictionary<string, List<ReferenceDescription>>(); 

        private ObservableCollection<TagInfo> _selectedTags = new ObservableCollection<TagInfo>();
        public ObservableCollection<TagInfo> SelectedTags
        {
            get { return _selectedTags; }
        }

        private int MonitorUpdateInterval { get; set; }

        private List<AutoUpdateTimers> ActiveAutoUpdateTimers { get; set; }
        public TagInfo SelectedTagItem { get; set; }
        private string Url { get; set; }

        //public ICommand SetAutoUpdate
        //{
        //    get { return new DelegateCommand(SetAutoUpdateHandler); }
        //}

        public ICommand CancelAutoUpdate
        {
            get { return new DelegateCommand(CancelAutoUpdateHandler); }
        }

        public ICommand SetValueCommand
        {
            get { return new DelegateCommand(UpdateValueHandler); }
        }

        //public ICommand AutoIncrementValue
        //{
        //    get { return new DelegateCommand(AutoIncrementValueHandler); }
        //}

        public ICommand MoveToTopCommand
        {
            get { return new DelegateCommand(MoveItemToTopHandler); }
        }

        public MainWindow()
        {
            InitializeComponent();

            InitUrls();

            ActiveAutoUpdateTimers = new List<AutoUpdateTimers>();
            var updateIntervalStr = ConfigurationManager.AppSettings["UpdateInterval"];
            int updateInterval = 150;
            if (updateIntervalStr != null)
            {
                int.TryParse(updateIntervalStr, out updateInterval);
            }
            MonitorUpdateInterval = updateInterval;

            
            //_client.MonitoredItemChanged += ClientOnMonitoredItemChanged;

            Loaded += (sender, args) =>
            {
                WindowPlacement.RestoreWindowPosition();
            };

            Closing += (sender, args) => 
            {
                WindowPlacement.StoreWindowPlacement();
            };
        }

        private void InitUrls()
        {
            UrlComboBox.Items.Clear();

            var urls = UrlConfigManager.GetUrls();
            foreach (var opcUrl in urls)
            {
                UrlComboBox.Items.Add(opcUrl.Url);
            }
            if (urls.Count > 0)
            {
                UrlComboBox.SelectedIndex = 0;
            }
        }

        private bool _isConnected = false;
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //_uaServer.UnsubscribeAllMonitoredItems();
                _valueNodes.Clear();
                _selectedTags.Clear();
                AliasGroupsTreeView.Items.Clear();

                if (_isConnected)
                {
                    _isConnected = false;
                    ConnectButton.Content = "Connect";
                    return;
                }

                Url = GetUrlText();
                if (string.IsNullOrEmpty((Url)))
                {
                    return;
                }

                UrlConfigManager.AddUrl(Url);

                _uaServer.KeepAliveNotification += (s, kae) =>
                {

                };

                _uaServer.Connect(Url, "None", MessageSecurityMode.None, false, "", "");

                _subscription = _uaServer.Subscribe(200);
                _uaServer.ItemChangedNotification += new MonitoredItemNotificationEventHandler(UaServerValueChanged);
                DiscoverNodes(null, null);

                _isConnected = true;
                ConnectButton.Content = "Disconnect";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect, Exception: " + ex);
            }
        }

        private string GetUrlText()
        {
            string url = null;
            if (UrlComboBox.SelectedIndex >= 0)
            {
                url = UrlComboBox.SelectedValue.ToString();
            }
            else if (!string.IsNullOrEmpty(UrlComboBox.Text))
            {
                url = UrlComboBox.Text;
            }

            return url;
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var url = GetUrlText();
            UrlConfigManager.DeleteUrl(url);

            InitUrls();
        }

        private void DiscoverNodes(TreeViewItem parent, ReferenceDescription nodeId, bool isVariableNode = false)
        {
            if (nodeId != null && nodeId.NodeId.ToString().Contains("Frosta.Static.Recept.Active"))
            {

            }

            ReferenceDescriptionCollection col = null;
            if (parent == null)
            {
                if (nodeId == null)
                {
                    nodeId = new ReferenceDescription();
                    nodeId.NodeId = new NodeId(Objects.RootFolder, 0);
                }
                AliasGroupsTreeView.Items.Clear();
                col = _uaServer.BrowseNode(nodeId);
            }
            else
            {
                if (!isVariableNode)
                    col = _uaServer.BrowseNode(nodeId);
                else
                    col = _uaServer.BrowseNode(nodeId);
            }

            foreach (var node in col)
            {
                var browseVariableAsFolder = false;
                //if (node.NodeClass == NodeClass.Variable && !node.NodeId.Identifier.ToString().Contains("_Hints"))
                //{
                //    var children = _uaServer.BrowseNode(node);
                //    browseVariableAsFolder = children.Count > 1;
                //}

                if (node.TypeDefinition.NamespaceUri == "FolderType" || node.NodeClass == NodeClass.Object || browseVariableAsFolder)
                {
                    TreeViewItem item = new TreeViewItem() {Header = node.BrowseName};
                    item.Tag = node;

                    item.ContextMenu = new ContextMenu();
                    var copyToClipboardItem = new MenuItem() { Header = "Copy Id to clipboard" };
                    copyToClipboardItem.Tag = node.NodeId.Identifier.ToString();
                    copyToClipboardItem.Click += (sender, args) =>
                    {
                        Clipboard.SetText(((MenuItem)sender).Tag.ToString());
                    };
                    item.ContextMenu.Items.Add(copyToClipboardItem);

                    var propertiesItem = new MenuItem() {Header = "Properites"};
                    propertiesItem.Tag = node;
                    propertiesItem.Click += (sender, args) =>
                    {
                        var propDescr = string.Format("Namespace: {0} \nNamespaceIndex: {1}\nIndentifier: {2}\nType: {3}", node.NodeId, node.NodeId.NamespaceIndex, node.NodeId.Identifier, node.TypeId);
                        MessageBox.Show(propDescr);
                    };
                    item.ContextMenu.Items.Add(propertiesItem);

                    if (parent == null)
                    {
                        AliasGroupsTreeView.Items.Add(item);
                    }
                    else
                    {
                        parent.Items.Add(item);
                    }
                    DiscoverNodes(item, node);
                }
                else if (node.NodeClass == NodeClass.Variable)
                {
                    if (!_valueNodes.ContainsKey(nodeId.NodeId.ToString()))
                    {
                        _valueNodes.Add(nodeId.NodeId.ToString(), new List<ReferenceDescription>());
                    }

                    if (
                        _valueNodes[nodeId.NodeId.ToString()].FirstOrDefault(
                            x => x.NodeId.ToString() == node.NodeId.ToString()) == null)
                    {
                        _valueNodes[nodeId.NodeId.ToString()].Add(node);
                    }
                }
            }
        }

        private string GetKeyFromNode(ExpandedNodeId node)
        {
            return node.NamespaceUri + node.NamespaceIndex;
        }

        private List<MonitoredItem> _monitoredItems = null;
        private void AliasGroupsTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_monitoredItems != null)
                {
                    foreach (var monitoredItem in _monitoredItems)
                    {
                        _uaServer.RemoveMonitoredItem(_subscription, monitoredItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while unsubscribing: " + ex.Message);
            }

            _monitoredItems = new List<MonitoredItem>();

            SelectedTags.Clear();
            if (AliasGroupsTreeView.SelectedItem == null)
            {
                return;
            }

            var node = (ReferenceDescription)((TreeViewItem)AliasGroupsTreeView.SelectedItem).Tag;
            if (!_valueNodes.ContainsKey(node.NodeId.ToString()))
                return;

            if (_valueNodes[node.NodeId.ToString()].Count < 1)
                return;

            if (node.NodeId.Identifier.ToString().Contains("._Hints"))
                return;

            string valuesDef = "";
            foreach (var tag in _valueNodes[node.NodeId.ToString()])
            {
                var value = _uaServer.ReadValueAsync((NodeId)tag.NodeId).Result;
                var tagInfo = new TagInfo() { Element = tag, AttributeData = value, Parent = this };
                SelectedTags.Add(tagInfo);

                WriteOutput($"Added selectedTag. {tag.TypeId}, {tag.DisplayName}");

                if (value.Value != null)
                {
                    valuesDef +=
                        string.Format("{0}\t: {1};", tag.DisplayName.ToString(),
                            GetPlcTypeFromType(value.Value.GetType())) +
                        Environment.NewLine;
                }

                if (!StatusCode.IsNotGood(value.StatusCode))
                {
                    var monitoredItem = _uaServer.AddMonitoredItem(_subscription, tag.NodeId.ToString(), tag.DisplayName.ToString(), 100);
                    _monitoredItems.Add(monitoredItem);
                }
                else
                {
                    return;
                }
            }
        }

        private string GetPlcTypeFromType(Type type)
        {
            if (type == typeof (bool))
                return "BOOL";
            if (type == typeof (Int16))
                return "WORD";
            if (type == typeof (string))
                return "STRING";
            return "UNKNOWN";
        }

        private void UaServerValueChanged(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            LogValueChanged(monitoredItem, e);

            string id = monitoredItem.ResolvedNodeId.ToString();

            foreach (var selectedTag in SelectedTags)
            {
                var selectedTagId = selectedTag.Element.NodeId.ToString();
                if (selectedTagId == id)
                {
                    var nv = (MonitoredItemNotification)e.NotificationValue;
                    selectedTag.AttributeData = nv.Value;
                    return;
                }
            }
        }

        private void LogValueChanged(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            var min = (MonitoredItemNotification)e.NotificationValue;
            string alias = monitoredItem.ResolvedNodeId.ToString();

            
            string description = "";

            if (StatusCode.IsGood(min.Value.StatusCode))
            {
                description = string.Format("{0} [{1}] Value: {2}, Type: {3}",
                    min.Value.ServerTimestamp.ToString("hh:mm:ss.fff"),
                    alias,
                    min.Value,
                    min.Value.GetType());
            }
            else
            {
                description = string.Format($"{DateTime.Now.ToString("hh:mm:ss.fff")} [{alias}] Error: ");
            }

            WriteOutput(description);
        }


        private async void BoolValue_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var tagItem = button.DataContext as TagInfo;
                var newValue = !(bool)tagItem.Value;
                var wr = await _uaServer.WriteValueAsync((NodeId)tagItem.Element.NodeId, newValue);
                if (!StatusCode.IsGood(wr.Code))
                {
                    MessageBox.Show("Failed to update bool value");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write bool value, Exception: " + ex.Message);
            }
        }

        public void UpdateValueHandler(object param)
        {
            if (SelectedTagItem == null)
                return;

            try
            {
                if (SelectedTagItem.IsIntValue())
                {
                    int val;
                    if (!int.TryParse(param.ToString(), out val))
                        return;

                    var wr = _uaServer.WriteValueAsync((NodeId)SelectedTagItem.Element.NodeId, param).Result;
                    
                }
                if (SelectedTagItem.IsStringValue())
                {
                    _uaServer.WriteValueAsync((NodeId)SelectedTagItem.Element.NodeId, param.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write value: " + param.ToString() + ", Exception: " + ex.Message);
            }

        }

        //public void AutoIncrementValueHandler(object param)
        //{
        //    if (SelectedTagItem == null)
        //        return;

        //    try
        //    {
        //        var sw = new Stopwatch();
        //        sw.Start();

        //        if (SelectedTagItem.Value is int)
        //        {
        //            int currentValue = (int)SelectedTagItem.Value;
        //            for (int val = currentValue + 1; val < (currentValue + 100); val++)
        //            {
        //                _uaServer.WriteValue(Url, SelectedTagItem.Element.NodeId, val);
        //            }

        //        }
        //        else if (SelectedTagItem.Value is short)
        //        {
        //            short currentValue = (short)SelectedTagItem.Value;
        //            for (int val = currentValue; val >= 0; val--)
        //            {
        //                _uaServer.WriteValue(Url, SelectedTagItem.Element.NodeId, (short)val);
        //            }
        //        }
        //        else if (SelectedTagItem.Value is bool)
        //        {
        //            bool currentValue = (bool) SelectedTagItem.Value;
        //            for (int i = 0; i < 100; i++)
        //            {
        //                currentValue = !currentValue;
        //                _uaServer.WriteValue(Url, SelectedTagItem.Element.NodeId, currentValue);
        //            }
        //        }
        //        else
        //        {
        //            return;
        //        }

        //        sw.Stop();
        //        MessageBox.Show("Finished updating value, took: " + sw.ElapsedMilliseconds + "ms");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Failed to write value: " + param.ToString() + ", Exception: " + ex.Message);
        //    }
        //}

        public void MoveItemToTopHandler()
        {
            if (SelectedTagItem == null)
                return;

            var tag = SelectedTagItem;
            SelectedTags.Remove(tag);
            SelectedTags.Insert(0, tag);
        }

        //public void SetAutoUpdateHandler()
        //{
        //    if (SelectedTagItem == null)
        //        return;
        //    if (SelectedTagItem.AutoUpdateActive)
        //        return;

        //    var dialog = new AutoIncrementTimerDialog();
        //    dialog.IncrementInterval = "1";
        //    dialog.IncrementTime = "10000";
        //    dialog.IncrementWrap = "1000";
        //    if (dialog.ShowDialog().Value)
        //    {
        //        int interval = 1;
        //        int.TryParse(dialog.IncrementInterval, out interval);
        //        int time = 1000;
        //        int.TryParse(dialog.IncrementTime, out time);
        //        int wrap = 1000;
        //        int.TryParse(dialog.IncrementWrap, out wrap);

        //        var autoTimer = new AutoUpdateTimers() { Tag = SelectedTagItem, Timer = new Timer(time) };
        //        autoTimer.Timer.Elapsed += (o, args) =>
        //        {
        //            if (autoTimer.Tag.DataType == typeof(Int16))
        //            {
        //                var next = (((Int16)autoTimer.Tag.Value) + interval) % wrap;
        //                _uaServer.WriteValue(Url, autoTimer.Tag.Element.NodeId, next);
        //            }
        //            else if (SelectedTagItem.DataType == typeof(Int32))
        //            {
        //                var next = (((Int32)autoTimer.Tag.Value) + interval) % wrap;
        //                _uaServer.WriteValue(Url, autoTimer.Tag.Element.NodeId, next);
        //            }
        //        };
        //        ActiveAutoUpdateTimers.Add(autoTimer);
        //        autoTimer.Timer.Start();
        //        SelectedTagItem.AutoUpdateActive = true;
        //    }
        //}

        public void CancelAutoUpdateHandler()
        {
            if (SelectedTagItem == null)
                return;
            if (!SelectedTagItem.AutoUpdateActive)
                return;

            foreach (var activeAutoUpdateTimer in ActiveAutoUpdateTimers)
            {
                if (activeAutoUpdateTimer.Tag.Element == SelectedTagItem.Element)
                {
                    activeAutoUpdateTimer.Timer.Stop();
                    ActiveAutoUpdateTimers.Remove(activeAutoUpdateTimer);
                    break;
                }
            }

            SelectedTagItem.AutoUpdateActive = false;
        }

        
        public class DelegateCommand : ICommand
        {
            public delegate void SimpleEventHandler();
            public delegate void SimpleEventHandlerWithParam(object param);
            private SimpleEventHandler _handler;
            private SimpleEventHandlerWithParam _handlerWithParam;
            private bool _isEnabled = true;

            public event EventHandler CanExecuteChanged;

            public DelegateCommand(SimpleEventHandler handler)
            {
                _handler = handler;
            }

            public DelegateCommand(SimpleEventHandlerWithParam handler)
            {
                _handlerWithParam = handler;
            }

            private void OnCanExecuteChanged()
            {
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
            }

            bool ICommand.CanExecute(object arg)
            {
                return IsEnabled;
            }

            void ICommand.Execute(object arg)
            {
                if (_handler != null)
                {
                    _handler();
                }
                else
                {
                    _handlerWithParam(arg);
                }
            }

            public bool IsEnabled
            {
                get { return _isEnabled; }

                set
                {
                    _isEnabled = value;
                    OnCanExecuteChanged();
                }
            }
        }

        private async void UpdateValue_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tagItem = button.DataContext as TagInfo;
            
            try
            {
                var res = await _uaServer.WriteValueAsync((NodeId)tagItem.Element.NodeId, tagItem.UpdateValue, tagItem.DataType);

                //_uaServer.WriteValues(new List<string> { tagItem.GetUpdateValue().ToString() }, new List<string> { tagItem.Element.NodeId.ToString() });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), $"Failed to update tag: {tagItem.Element.DisplayName}");
            }
        }

        private string _logCache = "";
        private void WriteOutput(string msg)
        {
            if (_logPaused)
            {
                _logCache = RemoveFront(_logCache, 200000);
                _logCache += msg + Environment.NewLine;
                return;
            }

            OutputTextBox.Dispatcher.BeginInvoke((Action) delegate()
            {
                if (_logCache != "")
                {
                    OutputTextBox.Text += _logCache;
                    _logCache = "";
                }

                OutputTextBox.Text = RemoveFront(OutputTextBox.Text, 200000);
                
                OutputTextBox.Text += (msg + Environment.NewLine);
                if (!_logPaused)
                {
                    OutputScrollViewer.ScrollToBottom();
                }
            });
        }

        private string RemoveFront(string str, int maxLength)
        {
            if (str.Length > maxLength)
            {
                str = str.Substring(str.Length - maxLength);
            }
            return str;
        }

        private void ClearLogButton_OnClick(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Dispatcher.BeginInvoke((Action)delegate()
            {
                OutputTextBox.Text = "";
            });
        }

        private bool _logPaused = false;
        private void PauseLogButton_OnClick(object sender, RoutedEventArgs e)
        {
            _logPaused = !_logPaused;
            PauseLogButton.Content = _logPaused ? "Run Log" : "Pause Log";
        }
    }
}
