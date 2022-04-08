using Konbi.RealsenseID.Services;
using Konbi.RealsenseID.Services.Dto;
using Konbi.RealsenseID.Util;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using rsid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Konbi.RealsenseID
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RealsenseID _realsenseID;
        private RealsenseService _realsenseService;
        private MqttClient _mqttClient;
        private string apiServerAddress = "";
        private string comport = "";
        private string mqttURI = "";
        private string mqttUser = "";
        private string mqttPassword = "";
        private int mqttPort = 1883;
        private bool isAutoDetect = false;
        private int CameraNumber = 0;
        private string _dumpDir;
        private int DatabaseVersion = 0;
        private ProgressBarDialog _progressBar;

        private static readonly Brush ProgressBrush = Application.Current.TryFindResource("ProgressBrush") as Brush;
        private static readonly Brush SuccessBrush = Application.Current.TryFindResource("SuccessBrush") as Brush;
        private static readonly Brush FailBrush = Application.Current.TryFindResource("FailBrush") as Brush;
        private static readonly float _updateLoopInterval = 0.05f;
        private static readonly float _userFeedbackDuration = 3.0f;

        private byte[] _previewBuffer = new byte[0]; // store latest frame from the preview callback
        private object _previewMutex = new object();

        private WriteableBitmap _previewBitmap;
        private rsid.Preview _preview;
        private DeviceState _deviceState;
        private FrameDumper _frameDumper;
        private bool _dumpEnabled = false;
        
        private bool _previewEnabled = true;
        private float _userFeedbackTime = 0;
        private rsid.AuthStatus _lastAuthHint = rsid.AuthStatus.Serial_Ok; // To show only changed hints. 
        private bool _authloopRunning = false;
        private bool _cancelWasCalled = false;

        //private string[] _userList = new string[0]; // latest user list that was queried from the device

        private List<(rsid.FaceRect, rsid.AuthStatus?, string userId)> _detectedFaces = new List<(rsid.FaceRect, rsid.AuthStatus?, string)>();
        private List<User> _userToShow = new List<User>();
        private List<User> _userToDeleteOnServer = new List<User>();
        private List<(Faceprints, string)> _userModification = new List<(Faceprints, string)>();

        private ConsoleWindow _consoleWindow;
        private int _fps = 0;
        private System.Diagnostics.Stopwatch _fpsStopWatch = new System.Diagnostics.Stopwatch();

        //mqtt config
        
        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += MainWindow_ContentRendered;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(_updateLoopInterval * 1000);
            timer.Tick += Timer_Tick;
            timer.Start();
            _fpsStopWatch.Start();
            if (_previewEnabled == false)
                LabelPreview.Visibility = Visibility.Collapsed;
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            
            GetConfigInfor();
            _realsenseID = new RealsenseID(this.comport, DatabaseVersion);
            _realsenseService = new RealsenseService(this.apiServerAddress);
            _mqttClient = new MqttClient(mqttURI, mqttUser, mqttPassword, mqttPort);
            LoadUserFromLocalDevice();
            //_realsenseID.GetUserFaceprintFromDevice();
            OnStartSession(string.Empty, false);
            ClearTitle();
            ThreadPool.QueueUserWorkItem(InitialSession);
            _mqttClient.ConnectAsync().Wait();
            _mqttClient.client.UseConnectedHandler(ev =>
            {
                Helper.WriteToFile("[MQTT] - Connected successfully with MQTT Brokers.");
            });
        }
        private void MainWindow_Closing(object sender, EventArgs e)
        {
            _consoleWindow?.Disable();
            if (_authloopRunning)
            {
                try
                {
                    _realsenseID.cancelDevice = true;
                    _realsenseID._auth.Cancel();
                    Thread.Sleep(500);
                }
                catch { }
            }
        }
        private void LoadUserFromLocalDevice()
        {
            try
            {
                var UserIds = _realsenseID.GetAllUserIdFromDevice();
                _userToShow.Clear();
                //Helper.WriteToFile($"get-faceprints {JsonConvert.SerializeObject(faceprints)}");
                if (UserIds != null)
                {
                    foreach (var userId in UserIds)
                    {

                        _userToShow.Add(new User(userId));
                    }
                    UpdateDisplayUser();
                }
            }
            catch(Exception ex)
            {
                Helper.WriteToFile($"LoadFaceprintInLocalDevice exception {ex.Message}");
            }
        }
        private void UpdateDisplayUser()
        {
            Dispatcher.Invoke(() => {
                ShowUsersOnUI(_userToShow);
                UsersTab.Header = $"List of User ({_userToShow.Count})";
                EnrollPanel.Visibility = Visibility.Collapsed;
            });
        }

        private void GetConfigInfor()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;


            try
            {
                //realsenseID config
                apiServerAddress = settings["api_server_address"].Value;
                isAutoDetect = bool.Parse(settings["AutoDetect"].Value);
                CameraNumber = int.Parse(settings["CameraNumber"].Value);
                DatabaseVersion = int.Parse(settings["DatabaseVersion"].Value);
                _dumpDir = settings["DumpDir"].Value;

                //auto detect comport

                if (isAutoDetect)
                {
                    Helper.WriteToFile($"Start auto detecting port");
                    var enumerator = new DeviceEnumerator();
                    var enumeration = enumerator.Enumerate();

                    if (enumeration.Count == 0)
                    {
                        Helper.WriteToFile($"Could not detect device. Please reconnect the device and try again.");
                        throw new Exception("Connection Error");
                    }
                    else if (enumeration.Count > 1)
                    {
                        Helper.WriteToFile($"More than one device detected. Please make sure only one device is connected and try again.");
                        throw new Exception("Connection Error");
                    }

                    comport = enumeration[0].port;
                }
                else
                {
                    comport = settings["Comport"].Value;
                }

                //mqtt config
                mqttURI = settings["mqtt_url"].Value;
                mqttUser = settings["mqtt_user"].Value;
                mqttPassword = settings["mqtt_password"].Value;
                if (string.IsNullOrEmpty(settings["mqtt_port"].Value))
                {
                    mqttPort = 1883;
                }
                else
                {
                    mqttPort = int.Parse(settings["mqtt_port"].Value);
                }

                //Log config info
                Helper.WriteToFile($"[Appsetting] - apiServerAddress: {apiServerAddress}");
                Helper.WriteToFile($"[Appsetting] - realsenseidComport: {comport}");
                Helper.WriteToFile($"[Appsetting] - mqttURI: {mqttURI}");
                Helper.WriteToFile($"[Appsetting] - mqttUser: {mqttUser}");
                Helper.WriteToFile($"[Appsetting] - mqttPassword: {mqttPassword}");
                Helper.WriteToFile($"[Appsetting] - mqttPort: {mqttPort}");
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"Cannot initial config {ex.ToString()}");
            } 
        }
        private void OnStartSession(string title, bool activateDumps)
        {
            Dispatcher.Invoke(() =>
            {
                ShowLogTitle(title);
                SetUIEnabled(false);
                RedDot.Visibility = Visibility.Visible;
                _cancelWasCalled = false;
                _lastAuthHint = rsid.AuthStatus.Serial_Ok;
                ResetDetectedFaces();
                try
                {
                    _frameDumper = activateDumps ? new FrameDumper(_dumpDir) : null;
                }
                catch (Exception ex)
                {
                    HandleDumpException(ex);
                }
            });
        }
        private void OnStopSession()
        {
            Dispatcher.Invoke(() =>
            {
                _frameDumper = null;
                SetUIEnabled(true);
                RedDot.Visibility = Visibility.Hidden;
            });

        }
        /**
         * 
         * Event handler
         * 
         * **/
        private void EnrollButton_Click(object sender, RoutedEventArgs e)
        {

            var enrollInput = new EnrollInput();
            if (ShowWindowDialog(enrollInput) == true)
            {
                if (ShowWindowDialog(new EnrollInstructions()) == true)
                {
                    //EnrollPanel.Visibility = Visibility.Visible;

                    ThreadPool.QueueUserWorkItem(EnrollJob, enrollInput.Username);
                }
            }

            
            
        }
        private void EnrollJob(Object data)
        {
            var userId = data as string;
            try
            {
                Dispatcher.Invoke(() =>
                {
                    _progressBar = new ProgressBarDialog { DialogTitle = { Text = "Starting checking user..." } };
                    _progressBar.Show();
                });
                //check user in the local device
                bool checkExistedUser = false;
                foreach (var user in _userToShow)
                {
                    if (user.Name == userId)
                    {
                        checkExistedUser = true;
                        bool check = _userToShow.Remove(user);
                        break;
                    }
                }
                bool checkDeleteOnDevice = false;
                if (checkExistedUser)
                {
                    Helper.WriteToFile($"[Enroll] - User {userId} already existed in the local device");
                    Dispatcher.Invoke(() =>
                    {
                        _progressBar.DialogTitle.Text = "Deleteing existing user...";
                    });
                    checkDeleteOnDevice = _realsenseID.RemoveUserFromDevice(userId);
                }

                //show the way to enroll face
                Dispatcher.Invoke(() =>
                {
                    _progressBar.DialogTitle.Text = "Enrolling...";
                });
                _realsenseID.EnrollExtract();
                if (_realsenseID.onEnrollExtractStatus == EnrollStatus.Success)
                {
                    Helper.WriteToFile($"[Enroll] - User {userId} enrolled extraction successful");

                    //step 1. push faceprint to server then next step 2
                    string faceprintString = JsonConvert.SerializeObject(_realsenseID.currentEnrollFaceprint);
                    string faceprintEncode = Helper.Base64Encode(faceprintString);
                    var faceprintEnrollReq = new EnrollFaceprintRequest(
                            userId,
                            faceprintEncode,
                            true
                        );
                    string statusCode = "";
                    bool checkDirectlySync = false;
                    
                    Dispatcher.Invoke(() => {
                        _progressBar.DialogTitle.Text = "Syncing to server...";
                        checkDirectlySync = _realsenseService.EnrollFaceprint(faceprintEnrollReq, out statusCode);
                    });
                    if (statusCode == "0")
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var alert = new Alert("Alert", "Call api to server timeout");
                            ShowWindowDialog(alert);
                            _progressBar.Hide();
                        });
                        return;
                    }
                    else if(statusCode == "Unauthorized")
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var alert = new Alert("Alert", "Unautherized");
                            ShowWindowDialog(alert);
                            _progressBar.Hide();
                        });
                        return;
                    }
                    if (checkDirectlySync)
                    {
                        Helper.WriteToFile($"[Enroll] - Push user {userId} to server successful");
                        //step 2. import to local device
                        var faceprintsArray = new List<(Faceprints, string)>();
                        Helper.WriteToFile($"[Enroll] - Import user {userId} version database: {_realsenseID.currentEnrollFaceprint.version}");
                        faceprintsArray.Add((_realsenseID.currentEnrollFaceprint, userId));
                        Dispatcher.Invoke(() =>
                        {
                            _progressBar.DialogTitle.Text = "Importing to server...";
                        });
                        var checkImport = _realsenseID.ImportFaceprintToDevice(faceprintsArray);
                        if (checkImport)
                        {
                            //3. update to display on UI
                            Helper.WriteToFile($"[Enroll] - Import user {userId} successful");
                            _userToShow.Add(new User(userId));
                            Dispatcher.Invoke(() => {
                                UpdateDisplayUser();
                            });
                            if (checkDeleteOnDevice)
                            {
                                List<string> lsUser = new List<string>();
                                lsUser.Add(userId);
                                //MQTT
                                var payload = new
                                {
                                    user = lsUser,
                                    count = 1
                                };
                                Dispatcher.Invoke(() =>{
                                    _mqttClient.PublishAsync(MqttTopic.MESSAGE_OVERRIDE_FACEPRINT_TOPIC, JsonConvert.SerializeObject(payload)).Wait();
                                    var alert = new Alert("Alert", $"Override user {userId} successful, synced to server");
                                    ShowWindowDialog(alert);
                                    _progressBar.Hide();
                                });
                            }
                            else
                            {
                                Dispatcher.Invoke(() => {
                                    var alert = new Alert("Alert", $"Enroll new user {userId} successful, synced to server");
                                    ShowWindowDialog(alert);
                                    _progressBar.Hide();
                                });
                            }
                        }
                        else
                        {
                            Helper.WriteToFile($"[Enroll] - Import user {userId} falied");
                            Dispatcher.Invoke(() => {
                                var alert = new Alert("Alert", $"Push user {userId} to server falied");
                                ShowWindowDialog(alert);
                                _progressBar.Hide();
                            });
                        }
                    }
                    else
                    {
                        Helper.WriteToFile($"[Enroll] - Push user {userId} to server falied");
                        Dispatcher.Invoke(() =>{
                            var alert = new Alert("Alert", $"Push user {userId} to server falied");
                            ShowWindowDialog(alert);
                            _progressBar.Hide();
                        });
                    }
                }
                else
                {
                    Helper.WriteToFile($"[Enroll] - User {userId} enrolled extraction falied");
                    Dispatcher.Invoke(() => {
                        var alert = new Alert("Alert", $"Enrolled user {userId} falied: {_realsenseID.onEnrollExtractStatus}");
                        ShowWindowDialog(alert);
                        _progressBar.Hide();
                    });
                }
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"Enroll exception {ex.ToString()}");
            }
            finally
            {
                _realsenseID._auth.Disconnect();
                HideEnrollingLabelPanel();
            }
        }
        private void HideEnrollingLabelPanel()
        {
            Dispatcher.Invoke(() =>
            {
                EnrollPanel.Visibility = Visibility.Collapsed;
            });
        }
        private void AuthenticateButton_Click(object sender, RoutedEventArgs e)
        {
            _realsenseID.Authenticate();
            if(_realsenseID.onAuthResult == AuthStatus.Success)
            {
                var alert = new Alert("Alert", $"Authenticate successful user: {_realsenseID.userIdDetected}");
                ShowWindowDialog(alert);
            }
            else
            {
                //no face detected
                var alert = new Alert("Alert", $"Authenticate failed: {_realsenseID.onAuthResult}");
                ShowWindowDialog(alert);
            }
        }
        private void SelectAllUsersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UsersListView.SelectAll();
        }
        private void SelectAllUsersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UsersListView.UnselectAll();
        }
        private void SetInstructionsToRefreshUsers(bool isRefresh)
        {
            string text = isRefresh ? "Updating users list" : "Press Enroll to add users";
            Dispatcher.Invoke(() =>
            {
                InstructionsEnrollUsers.Text = text;
            });
        }
        private void UsersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = UsersListView.SelectedItems.Count > 0;
        }
        private void ShowUsersOnUI(List<User> users)
        {
            Dispatcher.Invoke(() => {
                //UsersListView.ItemsSource = users;
                // Remove and clear source [1]
                UsersListView.ItemsSource = null;
                UsersListView.Items.Clear();
                // The list<> has been updated so reload the listview [2]
                UsersListView.ItemsSource = users;
                // Select the first item and focus the control [3]
                UsersListView.SelectedIndex = users.Count();
                UsersListView.ScrollIntoView(UsersListView.SelectedItem);
                UsersListView.Focus();
            });
        }
        private bool? ShowWindowDialog(Window window)
        {
            SetUIEnabled(false);
            bool? returnOK = window.ShowDialog();
            SetUIEnabled(true);
            return returnOK;
        }
        private void SetUIEnabled(bool isEnabled)
        {
            DeleteButton.IsEnabled = isEnabled && UsersListView.SelectedItems.Count > 0;
            EnrollButton.IsEnabled = isEnabled;
            AuthenticateButton.IsEnabled = isEnabled;
            UsersListView.IsEnabled = isEnabled;
            SelectAllUsersCheckBox.IsEnabled = isEnabled && _userToShow?.Count > 0;
        }

        /**
         * 
         * Sync/backup/enroll by image to Server
         * 
         * **/
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(DeleteUser, null);
        }
        private void DeleteUser(Object data)
        {
            try
            {
                List<User> usersDelete = new List<User>();
                Dispatcher.Invoke(() =>
                {
                    usersDelete = UsersListView.SelectedItems.Cast<User>().ToList();
                    SetInstructionsToRefreshUsers(true);
                });
                int deleteCount = 0;
                List<string> lsUser = new List<string>();
                bool checkDeleteOnDevice = false;
                //progress bar initial
                int total = 0;
                float progress = 0;
                Dispatcher.Invoke(() =>
                {
                    _progressBar = new ProgressBarDialog { DialogTitle = { Text = "Deleting user" } };
                    _progressBar.Show();
                });
                total = usersDelete.Count();

                if(total == _userToShow.Count)
                {
                    var status = _realsenseID.RemoveAllUserInDevice();
                    if(status == Status.Ok)
                    {
                        Helper.WriteToFile($"[DELETE] - deleted all faceprint on device successful");
                        _userToShow.Clear();
                    }
                    else
                    {
                        Helper.WriteToFile($"[DELETE] - deleted all faceprint on device falied");
                    }
                }
                else
                {
                    foreach (var user in usersDelete)
                    {
                        checkDeleteOnDevice = _realsenseID.RemoveUserFromDevice(user.Name);
                        if (checkDeleteOnDevice)
                        {
                            //delete from wordpress site
                            Helper.WriteToFile($"[DELETE] - deleted faceprint on device {user.Name}");
                            _userToDeleteOnServer.Add(new User(user.Name));
                            deleteCount++;
                            //remove out of _userToShow
                            _userToShow.Remove(user);
                        }
                        progress = ((float)deleteCount / total) * 100;
                        Dispatcher.Invoke(() =>
                        {
                            _progressBar.Update(progress);
                        });
                    }
                }
                
                Dispatcher.Invoke(() =>
                {
                    UpdateDisplayUser();
                    _progressBar.Hide();
                });
            }
            catch (Exception ex)
            {

                Helper.WriteToFile($"Delete exception {ex.ToString()}");
            }
        }
        private void PullServerButton_Click(object sender, RoutedEventArgs e){

            //LabelPreview.Content = "Pulling data from server ...";
            ThreadPool.QueueUserWorkItem(PullFromServer, sender);

        }
        private void PullFromServer(Object sender)
        {
            try
            {
                var serverData = new List<UserDataResponse>();
                List<UserDataResponse> serverFaceprint = new List<UserDataResponse>();
                var arrFaceprints = new List<(Faceprints, string)>();

                Dispatcher.Invoke(() =>
                {
                    _progressBar = new ProgressBarDialog { DialogTitle = { Text = "Pulling from server..." } };
                    _progressBar.Show();
                });

                string statusCode = "";
                serverData = _realsenseService.GetListFaceprints(out statusCode);
                if (statusCode == "0")
                {
                    Dispatcher.Invoke(() =>
                    {
                        var alert = new Alert("Alert", "Please check network internet");
                        ShowWindowDialog(alert);
                        _progressBar.Hide();
                    });
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    _progressBar.DialogTitle.Text = "Starting to import ...";
                });
                int deleteCount = 0;
                int updateCount = 0;
                var localUserId = "";

                //filter and delete user that has faceprint is less than 20 charecters
                foreach (var item in serverData)
                {
                    if (item.ccw_id2.Length > 20)
                    {
                        serverFaceprint.Add(item);
                    }
                    else
                    {
                        var checkDelete = _realsenseID.RemoveUserFromDevice(item.username);
                        if (checkDelete)
                        {
                            deleteCount++;
                            Helper.WriteToFile($"[PullFromServer] - Delete faceprints that ess than 20 charecters: {item.username}");
                        }
                        
                    }
                }
                //initial progress bar 
                float progress = 0;

                Helper.WriteToFile($"[PullFromServer] - Number item of raw faceprints from server : {serverData.Count}");
                Helper.WriteToFile($"[PullFromServer] - Number item of serverFaceprint: {serverFaceprint.Count}");
                Helper.WriteToFile($"[PullFromServer] - Number item of localFaceprint: {_userToShow.Count}");
                
                //delete items in localFaceprint on device that no longer existed when compare to serverFaceprint
                foreach (var localItem in _userToShow)
                {
                    localUserId = "";
                    var count = 0;
                    localUserId = localItem.Name;
                    foreach (var serverItem in serverFaceprint)
                    {
                        if (localUserId == serverItem.username)
                        {
                            count++;
                        }
                    }
                    if (count <= 0)
                    {
                        bool checkDelete = _realsenseID.RemoveUserFromDevice(localUserId);
                        if (checkDelete)
                        {
                            deleteCount++;
                        }
                    }
                }
                //update item
                foreach (var serverItem in serverFaceprint)
                {
                    //initial case
                    if (_userToShow.Count <= 0)
                    {
                        string userId = "";
                        try
                        {
                            string faceprintEncode = serverItem.ccw_id2; //ccw_id2 is faceprint
                            string faceprintDecode = Helper.Base64Decode(faceprintEncode);
                            Faceprints faceprint = JsonConvert.DeserializeObject<Faceprints>(faceprintDecode);
                            userId = serverItem.username;
                            arrFaceprints.Add((faceprint, userId));
                        }
                        catch (Exception ex)
                        {
                            Helper.WriteToFile($"[PullFromServer] exception with user {userId}: {ex.ToString()} ");
                        }
                    }
                    //update or add new case
                    else
                    {
                        int localItemCount = 0;
                        foreach (var localItem in _userToShow)
                        {
                            localItemCount++;
                            bool isUpdate = false;
                            try
                            {
                                //just count number update or override
                                if (serverItem.username == localItem.Name)
                                {
                                    //update new (default we update all -> because do not know which faceprint is override or not)
                                    isUpdate = true;
                                    break;
                                }
                                //
                                if (localItemCount == _userToShow.Count)
                                {
                                    //insert new
                                    string faceprintEncode = serverItem.ccw_id2; //ccw_id2 is faceprint
                                    string faceprintDecode = Helper.Base64Decode(faceprintEncode);
                                    Faceprints faceprint = JsonConvert.DeserializeObject<Faceprints>(faceprintDecode);
                                    string userId = serverItem.username;
                                    arrFaceprints.Add((faceprint, userId));
                                }
                            }
                            catch (Exception ex)
                            {
                                Helper.WriteToFile($"PullFromServer exception: {ex.ToString()} ");
                            }
                        }
                    }
                }
                progress = 50;
                Dispatcher.Invoke(() =>
                {
                    _progressBar.Update(progress);
                });
                //start to check then import
                var lsImportUsers = new List<(Faceprints, string)>();
                Helper.WriteToFile($"[PullFromServer] - Number of faceprint to import in arrFaceprints: {arrFaceprints.Count}");
                foreach (var (faceprint, userId) in arrFaceprints)
                {
                    Helper.WriteToFile($"[PullFromServer] - Faceprint of {userId} with database version {faceprint.version}");
                    //delete all the faceprints that not compatible with current Firmware of realsenseID device on the cloud

                    if (faceprint.version != DatabaseVersion)
                    {
                        var checkDelete = _realsenseService.DeleteFaceprint(new DeleteFaceprintRequest(userId));
                        if (checkDelete)
                        {
                            Helper.WriteToFile($"[PullFromServer] - Faceprint of {userId} is deleted on the cloud because wrong database version {faceprint.version}");
                        }
                    }
                    else
                    {
                        lsImportUsers.Add((faceprint, userId));
                    }
                }
                Helper.WriteToFile($"[PullFromServer] - Number of faceprint to import in lsImportUsers: {lsImportUsers.Count}");
                var check = _realsenseID.ImportFaceprintToDevice(lsImportUsers);
                if (check)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoadUserFromLocalDevice();
                        ShowWindowDialog(
                            new Alert("Alert",
                            "Number of added new faceprint is: " + lsImportUsers.Count() + "\n" +
                            "Number of updated faceprint is: " + updateCount + "\n" +
                            "Number of deleted faceprint is: " + deleteCount));
                        _progressBar.Hide();
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoadUserFromLocalDevice();
                        ShowWindowDialog(new Alert("Alert", "Import finish"));
                        _progressBar.Hide();
                    });
                }
            }
            catch (Exception ex)
            {

                Helper.WriteToFile($"PullFromServer exception: {ex.ToString()}");
            }
        }
        private void PushServerButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(PushToServer, null);
        }
        private void PushToServer(Object faceprints)
        {
            try
            {
                var faceprintData = new List<(Faceprints, string)>();
                //1. GET ALL THE FACEPRINT IN THE LOCAL DEVICE THEN ENCODE THE FACEPRINT
                Dispatcher.Invoke(() =>
                {
                    _progressBar = new ProgressBarDialog { DialogTitle = { Text = "Starting get faceprint local..." } };
                    _progressBar.Show();
                    faceprintData = _realsenseID.GetUserFaceprintFromDevice();
                    _progressBar.Hide();
                });

                //initial progress bar 
                int total = faceprintData.Count();
                float progress = 0;
                bool isFlagUpdate = false, isFlagDete = false;
                int countUpdate = 0;

                Dispatcher.Invoke(() =>
                {
                    _progressBar = new ProgressBarDialog { DialogTitle = { Text = "Pushing faceprints to server..." } };
                    _progressBar.Show();
                    faceprintData = _realsenseID.GetUserFaceprintFromDevice();
                });
                //update to server
                if (faceprints != null)
                {
                    string faceprintString = "";
                    string faceprintEncode = "";
                    bool checkResult = false;

                    List<string> lsUser = new List<string>();
                    foreach (var (faceprintsLocal, userIdLocal) in faceprintData)
                    {
                        faceprintString = JsonConvert.SerializeObject(faceprintsLocal);
                        faceprintEncode = Helper.Base64Encode(faceprintString);
                        //2. call api to to insert all of them
                        var enroll = new EnrollFaceprintRequest(
                                userIdLocal,
                                faceprintEncode,
                                true
                            );
                        string statusCode = "";
                        checkResult = _realsenseService.EnrollFaceprint(enroll, out statusCode);
                        if (checkResult)
                        {
                            countUpdate++;
                            lsUser.Add(userIdLocal);
                            progress = ((float)countUpdate / total) * 100;
                            Dispatcher.Invoke(() =>
                            {
                                _progressBar.Update(progress);
                            });
                        }
                    }
                    //MQTT
                    var payload = new
                    {
                        user = lsUser,
                        count = countUpdate
                    };
                    _mqttClient.PublishAsync(MqttTopic.MESSAGE_CREATE_FACEPRINT_TOPIC, JsonConvert.SerializeObject(payload)).Wait();
                    isFlagUpdate = true;
                }

                //notify
                Dispatcher.Invoke(() =>
                {
                    ShowWindowDialog(new Alert("Alert", $"Number of updated facepints: {countUpdate}"));
                    _progressBar.Hide();
                });
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"PushToServer exception {ex.ToString()}");
            }

        }
        private void EnrollByImage_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\Images";
            try
            {
                string[] filePaths = Directory.GetFiles(path);
                Task enrollByImage = new Task(
                    () => { EnrollImageJob(filePaths); }
                    );
                enrollByImage.Start();

                //or this below way
                //Task.Run(() => EnrollImageJob(filePaths));
            }
            catch(Exception ex)
            {
                var alert = new Alert("Alert", $"Exception : {ex.Message}");
                ShowWindowDialog(alert);
            }
        }
        private void EnrollImageJob(string[] filePaths)
        {
            try
            {
                int imgHeight = 0;
                int imgWidth = 0;
                string imgName = "";
                int countSuccessEnroll = 0;
                int countFailedEnroll = 0;
                int total = 0;
                int count = 0;
                float progress = 0;

                Dispatcher.Invoke(() =>
                {
                    _progressBar = new ProgressBarDialog { DialogTitle = { Text = "Enrolling by image" } };
                    _progressBar.Show();
                });


                foreach (var file in filePaths)
                {
                    total = filePaths.Length;
                    count++;
                    Helper.WriteToFile("Enroll_By_Image file: " + file);
                    System.Drawing.Image imgInfo = System.Drawing.Image.FromFile(file);

                    byte[] imgByteArr = Helper.ImageToByteArray(imgInfo);
                    imgName = Path.GetFileNameWithoutExtension(file);
                    imgWidth = imgInfo.Width;
                    imgHeight = imgInfo.Height;
                    //
                    const int maxImageSize = 900 * 1024;
                    var (buffer, w, h, bitmap) = ImageHelper.ToBgr(file, maxImageSize);
                    bool checkDeleteOnDevice = false;
                    int countEndLoop = 0;
                    //enroll by image
                    foreach (var (faceprint, userId) in _realsenseID.faceprintsArray)
                    {
                        countEndLoop++;
                        if (userId == imgName)
                        {
                            //delete ole one
                            checkDeleteOnDevice = _realsenseID.RemoveUserFromDevice(imgName);
                            if (checkDeleteOnDevice)
                            {
                                Helper.WriteToFile($"[DELETE] delete old faceprint then override by image enrollment success: {imgName}");
                            }
                        }
                        if (countEndLoop == _realsenseID.faceprintsArray.Count)
                        {
                            //override or add new
                            var status = _realsenseID.EnrollImage(imgName, buffer, w, h);
                            if (status == EnrollStatus.Success)
                            {
                                Helper.WriteToFile($"Enroll_By_Image success: {imgName}");
                                countSuccessEnroll++;
                            }
                            else
                            {
                                Helper.WriteToFile($"Enroll_By_Image failed: username - {imgName}, width - {imgWidth} height - {imgHeight}, reason - {status}");
                                countFailedEnroll++;
                            }
                            progress = ((float)count / total) * 100;

                            Dispatcher.Invoke(() =>
                            {
                                _progressBar.Update(progress);
                            });
                            //if add new
                            if (!checkDeleteOnDevice)
                            {
                                _userToShow.Add(new User(imgName));
                            }
                        }
                    }

                }
                Dispatcher.Invoke(() =>
                {
                    UpdateDisplayUser();
                    var alert = new Alert("Alert", $"Enrolling by image information \n Success: {countSuccessEnroll}\n Failed: {countFailedEnroll}");
                    ShowWindowDialog(alert);
                    _progressBar.Hide();
                });
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"[EnrollImageJob] exception {ex.ToString()}");
            }
            
        }

        /**
         * Cancel event
         * **/
        private void CancelEnrollButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(CancelJob);
            EnrollPanel.Visibility = Visibility.Collapsed;
        }
        private void CancelJob(Object threadContext)
        {
            try
            {
                _realsenseID.cancelDevice = true;
                var status = _realsenseID._auth.Cancel();
                Thread.Sleep(500); // give time to device to cancel before exiting
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"[CancelJob] exception {ex.ToString()}");
            }
        }
        /**
         * 
         * Realtime camera
         * 
         * **/
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_userFeedbackTime > 0)
            {
                _userFeedbackTime -= _updateLoopInterval;
                if (_userFeedbackTime < 2.0f)
                {
                    UserFeedbackContainer.Opacity -= _updateLoopInterval * 0.5;
                    PreviewCanvas.Opacity -= _updateLoopInterval * 0.5;
                }
                if (_userFeedbackTime <= 0)
                {
                    ClearTitle();
                    PreviewCanvas.Visibility = Visibility.Hidden;
                }
            }
        }
        private void ResetDetectedFaces()
        {
            RenderDispatch(() =>
            {
                _detectedFaces.Clear();
                PreviewCanvas.Children.Clear();
                PreviewCanvas.Visibility = Visibility.Visible;
                PreviewCanvas.Opacity = 1.0;
            });
        }
        private void ClearTitle()
        {
            BackgroundDispatch(() =>
            {
                UserFeedbackContainer.Visibility = Visibility.Collapsed;
            });
        }
        private void BackgroundDispatch(Action action)
        {
            Dispatcher.BeginInvoke(action, DispatcherPriority.Background, null);
        }
        private void OnPreview(rsid.PreviewImage image, IntPtr ctx)
        {
            string previewLabel = null;
            rsid.PreviewImage preview_image = new rsid.PreviewImage();
            lock (_previewMutex)
            {
                // dump original data
                if (_dumpEnabled)
                {
                    DumpFrame(image);
                }

                // preview image is allways RGB24
                preview_image.size = image.width * image.height * 3;
                if (_previewBuffer.Length != preview_image.size)
                {
                    Console.WriteLine("Creating preview buffer");
                    _previewBuffer = new byte[preview_image.size];
                }
                // convert raw to rgb for preview
                if (_deviceState.PreviewConfig.previewMode == rsid.PreviewMode.RAW10_1080P)
                {
                    if (RawPreviewHandler(ref image, ref preview_image) == false)
                        return;
                }
                else
                {
                    preview_image = image;
                    Marshal.Copy(preview_image.buffer, _previewBuffer, 0, preview_image.size);
                }

                /// calculate FPS
                _fps++;
                if (_fpsStopWatch.Elapsed.Seconds >= 1)
                {
                    var dumpsLabel = _frameDumper != null ? " (dumps enabled)" : string.Empty;
                    previewLabel = $"{image.width}x{image.height}  {_fps} FPS {dumpsLabel}";
                    _fps = 0;
                    _fpsStopWatch.Restart();
                }
            }
            RenderDispatch(() =>
            {
                if (previewLabel != null)
                    LabelPreviewInfo.Content = previewLabel;
                UIHandlePreview(preview_image);
            });
        }

        private void OnSnapshot(PreviewImage image, IntPtr ctx)
        {
            try
            {
                lock (_previewMutex)
                {
                    if (_deviceState.DeviceConfig.dumpMode == DeviceConfig.DumpMode.CroppedFace)
                        DumpImage(image);
                }
            }
            catch (Exception ex)
            {
                HandleDumpException(ex);
            }
        }
        private void DumpImage(PreviewImage image)
        {
            if (_frameDumper != null)
            {
                try
                {
                    if (_deviceState.PreviewConfig.previewMode == PreviewMode.RAW10_1080P)
                        _frameDumper.DumpRawImage(image);
                    else
                        _frameDumper.DumpPreviewImage(image);
                }
                catch (Exception ex)
                {
                    HandleDumpException(ex);
                }
            }
        }

        private void UIHandlePreview(rsid.PreviewImage image)
        {
            var targetWidth = (int)PreviewImage.Width;
            var targetHeight = (int)PreviewImage.Height;

            //create writable bitmap if not exists or if image size changed
            if (_previewBitmap == null || targetWidth != image.width || targetHeight != image.height)
            {
                PreviewImage.Width = image.width;
                PreviewImage.Height = image.height;
                Console.WriteLine($"Creating new WriteableBitmap preview buffer {image.width}x{image.height}");
                Helper.WriteToFile($"Creating new WriteableBitmap preview buffer {image.width}x{image.height}");
                _previewBitmap = new WriteableBitmap(image.width, image.height, 96, 96, PixelFormats.Rgb24, null);
                PreviewImage.Source = _previewBitmap;
            }
            Int32Rect sourceRect = new Int32Rect(0, 0, image.width, image.height);
            lock (_previewMutex)
            {
                _previewBitmap.WritePixels(sourceRect, _previewBuffer, image.stride, 0);
            }
        }
        private void DumpFrame(rsid.PreviewImage image)
        {
            if (_dumpEnabled && _frameDumper != null)
            {
                try
                {
                    if (_deviceState.PreviewConfig.previewMode == rsid.PreviewMode.RAW10_1080P)
                        _frameDumper.DumpRawImage(image);
                    else
                        _frameDumper.DumpPreviewImage(image);
                }
                catch (Exception ex)
                {
                    HandleDumpException(ex);
                }
            }
        }
        private void HandleDumpException(Exception ex)
        {
            _dumpEnabled = false;
            _frameDumper = null;
            RenderDispatch(() =>
            {
                ShowErrorMessage("Dump failed", ex.Message + "\nDump stopped..");
            });
        }
        private bool RawPreviewHandler(ref rsid.PreviewImage raw_image, ref rsid.PreviewImage preview_image)
        {
            if (raw_image.metadata.sensor_id != 0) // preview only left sensor
            {
                return false;
            }
            if (PreviewImage.Visibility != Visibility.Visible)
                InvokePreviewVisibility(Visibility.Visible);
            preview_image.buffer = Marshal.AllocHGlobal(preview_image.size);
            if (!_preview.RawToRgb(ref raw_image, ref preview_image))
            {
                Marshal.FreeHGlobal(preview_image.buffer);
                return false;
            }
            Marshal.Copy(preview_image.buffer, _previewBuffer, 0, preview_image.size);
            Marshal.FreeHGlobal(preview_image.buffer);
            return true;
        }
        private void InvokePreviewVisibility(Visibility visibility)
        {
            Dispatcher.Invoke(() => SetPreviewVisibility(visibility));
        }
        private void SetPreviewVisibility(Visibility visibility)
        {
            var previewMode = _deviceState.PreviewConfig.previewMode;
            LabelPreview.Content = $"Camera Preview\n({previewMode.ToString().ToLower()} preview mode)";
            PreviewImage.Visibility = visibility;
        }
        private void PreviewImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_deviceState.PreviewConfig.previewMode == rsid.PreviewMode.RAW10_1080P || !_deviceState.IsOperational)
                return;
            LabelPlayStop.Visibility = LabelPlayStop.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            if (LabelPlayStop.Visibility == Visibility.Hidden)
            {
                _preview.Start(OnPreview);
                TogglePreviewOpacity(true);
            }
            else
            {
                _preview.Stop();
                TogglePreviewOpacity(false);
            }
        }
        private void TogglePreviewOpacity(bool isActive)
        {
            RenderDispatch(() =>
            {
                PreviewImage.Opacity = isActive ? 1.0 : 0.85;
                LabelPreviewInfo.Opacity = isActive ? 0.66 : 0.3;
            });
        }
        private void RenderDispatch(Action action)
        {
            try
            {
                Dispatcher.BeginInvoke(action, DispatcherPriority.Render, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RenderDispatch: " + ex.Message);
            }
        }

        private void ShowLogTitle(string title)
        {
            if (string.IsNullOrEmpty(title) == false)
            {
                //LogTextBox.Text += $"\n{title}\n===========\n";
                OutputText.Text = title;
            }
            //LogScroll.ScrollToEnd();
        }
        private void ShowLog(string message)
        {
            BackgroundDispatch(() =>
            {
                // add log line
                LogTextBox.Text += message + "\n";
                OutputText.Text = message;
            });
        }
        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            ClearLog();
        }
        private void ClearLog()
        {
            LogTextBox.Text = "";
            OutputText.Text = "";
            LogScroll.ScrollToEnd();
        }
        private void OpenConsoleToggle_Click(object sender, RoutedEventArgs e)
        {
            _consoleWindow?.ToggleVisibility();
            ToggleConsoleAsync(OpenConsoleToggle.IsChecked.GetValueOrDefault());
        }
        private void ToggleConsoleAsync(bool show)
        {
            const int SW_HIDE = 0;
            const int SW_SHOW = 5;
            ShowWindow(GetConsoleWindow(), show ? SW_SHOW : SW_HIDE);
        }

        private void InitialSession(Object threadContext)
        {
            ShowProgressTitle("Connecting...");

            // show host library version
            var hostVersion = rsid.Authenticator.Version();
            ShowLog("Host: v" + hostVersion + "\n");
            var title = $"RealSenseID v{hostVersion}";

            try
            {
                _deviceState = DetectDevice();
            }
            catch (Exception ex)
            {
                OnStopSession();
                ShowErrorMessage("Connection Error", ex.Message);
                return;
            }

            // is in loader
            if (!_deviceState.IsOperational)
            {
                OnStopSession();

                var compatibleVersion = rsid.Authenticator.CompatibleFirmwareVersion();

                ShowFailedTitle("Device Error");
                var msg = $"Device failed to respond. Please reconnect the device and try again." +
                    $"\nIf the the issue persists, flash firmware version { compatibleVersion } or newer.\n";
                ShowLog(msg);
                ShowErrorMessage("Device Error", msg);

                return;
            }

            if (!_deviceState.IsCompatible)
            {
                OnStopSession();

                var compatibleVersion = rsid.Authenticator.CompatibleFirmwareVersion();

                ShowFailedTitle("FW Incompatible");
                var msg = $"Firmware version is incompatible.\nPlease update to version { compatibleVersion } or newer.\n";
                ShowLog(msg);
                ShowErrorMessage("Firmware Version Error", msg);

                return;
            }

            try
            {
                if (!_realsenseID.ConnectAuth())
                {
                    throw new Exception("Connection failed");
                }

                bool isPaired = PairDevice();
                if (!isPaired)
                {
                    ShowErrorMessage("Pairing Error", "Device pairing failed.\nPlease make sure the device wasn't previously paired and try again.");
                    throw new Exception("Pairing failed");
                }

                UpdateAdvancedMode();

                // start preview
                _deviceState.PreviewConfig.cameraNumber = CameraNumber;
                _deviceState.PreviewConfig.portraitMode = _deviceState.DeviceConfig.cameraRotation == DeviceConfig.CameraRotation.Rotation_0_Deg || _deviceState.DeviceConfig.cameraRotation == DeviceConfig.CameraRotation.Rotation_180_Deg;
                if (_preview == null)
                    _preview = new rsid.Preview(_deviceState.PreviewConfig);
                else
                    _preview.UpdateConfig(_deviceState.PreviewConfig);

                _preview.Start(OnPreview, OnSnapshot);
                RefreshUserList();
                ShowSuccessTitle("Connected");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Authenticator Error", ex.Message);
                ShowFailedTitle(ex.Message);
            }
            finally
            {
                OnStopSession();
                _realsenseID._auth.Disconnect();
            }
        }
        private void UpdateAdvancedMode()
        {
            rsid.DeviceConfig? deviceConfig = QueryDeviceConfig();
            if (!deviceConfig.HasValue)
            {
                var msg = "Failed to query device config";
                ShowLog(msg);
                ShowErrorMessage("QueryDeviceConfig Error", msg);
                throw new Exception("QueryDeviceConfig Error");
            }

            _deviceState.DeviceConfig = deviceConfig.Value;
        }
        private bool PairDevice()
        {
#if RSID_SECURE
            ShowLog("Pairing..");

            IntPtr pairArgsHandle = IntPtr.Zero;
            pairArgsHandle = rsid_create_pairing_args_example(_signatureHelpeHandle);
            var pairingArgs = (rsid.PairingArgs)Marshal.PtrToStructure(pairArgsHandle, typeof(rsid.PairingArgs));

            var rv = _authenticator.Pair(ref pairingArgs);
            if (rv != rsid.Status.Ok)
            {
                ShowLog("Failed\n");
                if (pairArgsHandle != IntPtr.Zero) rsid_destroy_pairing_args_example(pairArgsHandle);
                return false;
            }

            ShowLog("Success\n");
            rsid_update_device_pubkey_example(_signatureHelpeHandle, Marshal.UnsafeAddrOfPinnedArrayElement(pairingArgs.DevicePubkey, 0));
#endif //RSID_SECURE

            return true;
        }
        private DeviceState DetectDevice()
        {
            rsid.SerialConfig config;

            // acquire communication settings
            if (isAutoDetect)
            {
                var enumerator = new DeviceEnumerator();
                var enumeration = enumerator.Enumerate();

                if (enumeration.Count == 0)
                {
                    var msg = "Could not detect device.\nPlease reconnect the device and try again.";
                    ShowErrorMessage("Connection Error", msg);
                    throw new Exception("Connection Error");
                }
                else if (enumeration.Count > 1)
                {
                    var msg = "More than one device detected.\nPlease make sure only one device is connected and try again.";
                    ShowErrorMessage("Connection Error", msg);
                    throw new Exception("Connection Error");
                }

                config.port = enumeration[0].port;
            }
            else
            {
                config.port = this.comport;
            }


            var device = QueryDeviceMetadata(config);
            if (!device.HasValue)
            {
                var msg = "Could not connect to device.\nPlease reconnect the device and try again.";
                ShowErrorMessage("Connection Error", msg);
                throw new Exception("Connection Error");
            }

            return device.Value;
        }
        private DeviceState? QueryDeviceMetadata(rsid.SerialConfig config)
        {
            var device = new DeviceState();
            device.SerialConfig = config;

            using (var controller = new rsid.DeviceController())
            {
                ShowLog($"Connecting to {device.SerialConfig.port}...");
                var status = controller.Connect(device.SerialConfig);
                if (status != rsid.Status.Ok)
                {
                    ShowLog("Failed\n");
                    return null;
                }
                ShowLog("Success\n");

                ShowLog("Firmware:");
                var fwVersion = controller.QueryFirmwareVersion();

                var versionLines = fwVersion.ToLower().Split('|');

                foreach (var v in versionLines)
                {
                    var splitted = v.Split(':');
                    if (splitted.Length == 2)
                    {
                        ShowLog($" * {splitted[0].ToUpper()} - {splitted[1]}");
                        if (splitted[0] == "opfw")
                            device.FirmwareVersion = splitted[1];
                        else if (splitted[0] == "recog")
                            device.RecognitionVersion = splitted[1];
                    }
                }
               ShowLog("");

                var sn = controller.QuerySerialNumber();
                device.SerialNumber = sn;
                ShowLog($"S/N: {device.SerialNumber}\n");

               ShowLog("Pinging device...");

                status = controller.Ping();
                device.IsOperational = status == rsid.Status.Ok;

                ShowLog($"{(device.IsOperational ? "Success" : "Failed")}\n");
            }

            var isCompatible = rsid.Authenticator.IsFwCompatibleWithHost(device.FirmwareVersion);
            device.IsCompatible = isCompatible;
            ShowLog($"Is compatible with host? {(device.IsCompatible ? "Yes" : "No")}\n");

            rsid.DeviceConfig? deviceConfig = null;
            if (_deviceState.IsOperational)
            {
                // device is in operational mode, we continue to query config as usual
                if (!_realsenseID.ConnectAuth())
                {
                    ShowLog("Failed\n");
                    return null;
                }
                deviceConfig = QueryDeviceConfig();
                _realsenseID._auth.Disconnect();
                if (deviceConfig.HasValue)
                {
                    //device.PreviewConfig = new rsid.PreviewConfig { cameraNumber = CameraNumber, previewMode = (rsid.PreviewMode)deviceConfig.Value.previewMode };
                    device.DeviceConfig = deviceConfig.Value;
                    _deviceState.PreviewConfig.portraitMode = device.DeviceConfig.cameraRotation == DeviceConfig.CameraRotation.Rotation_0_Deg || device.DeviceConfig.cameraRotation == DeviceConfig.CameraRotation.Rotation_180_Deg;
                    device.PreviewConfig = new PreviewConfig { cameraNumber = CameraNumber, previewMode = _deviceState.PreviewConfig.previewMode, portraitMode = _deviceState.PreviewConfig.portraitMode };
                }
            }

            return device;
        }
        private rsid.DeviceConfig? QueryDeviceConfig()
        {

            ShowLog("");
            ShowLog("Query device config..");
            rsid.DeviceConfig deviceConfig;
            var rv = _realsenseID._auth.QueryDeviceConfig(out deviceConfig);
            if (rv != rsid.Status.Ok)
            {
                ShowLog("Query error: " + rv.ToString());
                ShowFailedTitle("Query error: " + rv.ToString());
                return null;
            }
            LogDeviceConfig(deviceConfig);
            return deviceConfig;
        }

        private void ShowSuccessTitle(string message)
        {
            ShowTitle(message, SuccessBrush, _userFeedbackDuration);
        }
        private void ShowFailedTitle(string message)
        {
            ShowTitle(message, FailBrush, _userFeedbackDuration);
        }
        private void RefreshUserList()
        {
            // Query users and update the user list display            
            /*ShowLog("Query users..");
            SetInstructionsToRefreshUsers(true);
            string[] users;
            var rv = _realsenseID._auth.QueryUserIds(out users);
            if (rv != rsid.Status.Ok)
            {
                throw new Exception("Query error: " + rv.ToString());
            }

            ShowLog($"{users.Length} users");

            // update the gui and save the list into _userList
            SetInstructionsToRefreshUsers(false);
            BackgroundDispatch(() =>
            {
                UpdateUsersUIList(users);
            });*/
            //_userList = users;
        }
        private void UpdateUsersUIList(string[] users)
        {
            //UsersListView.ItemsSource = users.ToList<string>();
           /* UsersListView.UnselectAll();
            DeleteButton.IsEnabled = false;
            SelectAllUsersCheckBox.IsChecked = false;
            int usersCount = users.Length;
            UsersTab.Header = $"Users ({usersCount})";
            var isEnabled = usersCount > 0;
            InstructionsEnrollUsers.Visibility = isEnabled ? Visibility.Collapsed : Visibility.Visible;
            SelectAllUsersCheckBox.IsEnabled = isEnabled;
            AuthenticateButton.IsEnabled = isEnabled;*/
        }
        private void ShowProgressTitle(string message)
        {
            ShowTitle(message, ProgressBrush);
        }
        private void ShowTitle(string message, Brush color, float duration = 0)
        {

            BackgroundDispatch(() =>
            {
                _userFeedbackTime = duration;
                UserFeedbackText.Text = message;
                UserFeedbackPanel.Background = color;
                UserFeedbackContainer.Visibility = Visibility.Visible;
                UserFeedbackContainer.Opacity = _detectedFaces.Count <= 1 ? 1.0 : 0.0f; // show title only if single face
            });
        }
        private void LogDeviceConfig(rsid.DeviceConfig deviceConfig)
        {
            ShowLog(" * Camera Rotation: " + deviceConfig.cameraRotation.ToString());
            ShowLog(" * Confidence Level: " + deviceConfig.securityLevel.ToString());
            //ShowLog(" * Preview Mode: " + deviceConfig.previewMode.ToString());
            ShowLog(" * Algo Flow: " + deviceConfig.algoFlow);
            ShowLog(" * Face Selection Policy: " + deviceConfig.faceSelectionPolicy);
            ShowLog(" * Dump Mode: " + deviceConfig.dumpMode.ToString());
            //ShowLog(" * Host Mode: " + _flowMode);
            ShowLog(" * Camera Index: " + _deviceState.PreviewConfig.cameraNumber);
            ShowLog("");
        }
        private void ShowErrorMessage(string title, string message)
        {
            Dispatcher.Invoke(() => (ShowWindowDialog(new Alert(title, message))));
        }

        private struct DeviceState
        {
            public string FirmwareVersion;
            public string RecognitionVersion;
            public string SerialNumber;
            public bool IsOperational;
            public bool IsCompatible;
            public SerialConfig SerialConfig;
            public PreviewConfig PreviewConfig;
            public DeviceConfig DeviceConfig;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
    public class User
    {
        public string Name;
        public User(string Name)
        {
            this.Name = Name;
        }
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
