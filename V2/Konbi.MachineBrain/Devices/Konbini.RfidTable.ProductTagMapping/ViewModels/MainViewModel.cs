using Caliburn.Micro;
using Konbini.Messages.Services;
using Konbini.RfidFridge.TagManagement.DTO;
using Konbini.RfidFridge.TagManagement.Interface;
using Konbini.RfidFridge.TagManagement.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using static Konbini.RfidFridge.TagManagement.DTO.PlateCategoryDTO;
using MessageBox = System.Windows.Forms.MessageBox;
using Screen = Konbini.RfidFridge.TagManagement.Enums.Screen;

namespace Konbini.RfidFridge.TagManagement.ViewModels
{
    public class MainViewModel : StateViewModel
    {
        public MainViewModel(IEventAggregator events, ShellViewModel shellView) : base(events, shellView)
        {
            CanWrite = false;
            Tags = new List<TagDTO>();
            IsPlate = true;
        }

        #region Private fields
        private List<TagDTO> tags;
        #endregion

        #region Services

        private IMbCloudService _service;
        public IMbCloudService MbCloudService
        {
            get
            {
                return _service;
            }
            set
            {
                _service = value;
                GetPlateOrTray();
            }
        }
        public IRfidReaderInterface RfidReaderInterface { get; set; }
        public ArrayList airInterfaceProtList = new ArrayList();
        public System.Timers.Timer timer;
        #endregion

        #region Properties

        public List<PlateDTO> Plates { get; set; }
        public List<PlateCategory> Categories { get; set; }
        public List<TagDTO> Tags
        {
            get => tags; set
            {
                tags = value;
                NotifyOfPropertyChange(() => Tags);
                CheckIfCanWrite();
            }
        }

        private PlateDTO _selectedPlate;
        public PlateDTO SelectedPlate
        {
            get => _selectedPlate;
            set
            {
                _selectedPlate = value;
                if (value != null && Tags != null && Tags.Any())
                {
                    //Update tags
                    foreach (var tag in Tags)
                    {
                        tag.PlateModel = value.Code;
                    }
                    NotifyOfPropertyChange(() => Tags);
                }
                NotifyOfPropertyChange(() => SelectedPlate);
                CheckIfCanWrite();
            }
        }
        private PlateCategory _selectedCategory;
        public PlateCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                if (value != null)
                {
                    Plates = value.Plates.ToList();
                    NotifyOfPropertyChange(() => Plates);
                }
                NotifyOfPropertyChange(() => SelectedCategory);
                CheckIfCanWrite();
            }
        }
        private bool _canWrite;

        public bool CanWrite
        {
            get => _canWrite;
            set
            {
                _canWrite = value;
                NotifyOfPropertyChange(() => CanWrite);
            }
        }

        private bool _isPlate;
        public bool IsPlate
        {
            get => _isPlate;
            set
            {
                _isPlate = value;
                NotifyOfPropertyChange(() => IsPlate);

                SelectedPlate = null;
                Plates = new List<PlateDTO>();
                NotifyOfPropertyChange(() => Plates);
                ShowCategory = value ? Visibility.Visible : Visibility.Collapsed;
                PlateTrayTitle = value ? "Plate" : "Tray";
                if (MbCloudService != null)
                {
                    GetPlateOrTray();
                }
            }
        }

        private Visibility _showCategory;
        public Visibility ShowCategory
        {
            get => _showCategory;
            set
            {
                _showCategory = value;
                NotifyOfPropertyChange(() => ShowCategory);
            }
        }
        private string _plateTrayTitle;
        public string PlateTrayTitle
        {
            get => _plateTrayTitle;
            set
            {
                _plateTrayTitle = value;
                NotifyOfPropertyChange(() => PlateTrayTitle);
            }
        }
        public string Message { get; set; }
        #endregion

        protected override void OnInitialize()
        {
            base.OnInitialize();
            timer = new Timer(1 * 1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var ok = RfidReaderInterface.GetReaderInfor();
            if (!ok)
            {
                GetConnection();
            }
        }

        private async Task GetConnection()
        {
            await Task.Run(async () =>
            {
                if (!RfidReaderInterface.Connect())
                {
                    //Execute.OnUIThread(() =>
                    //{
                    //    ShowMessageDialog("Failed to connect to hardware!");
                    //});

                    return;
                }
                else
                {

                    RfidReaderInterface.StartRecord();
                    if (RfidReaderInterface.OnTagsRecord == null)
                    {
                        RfidReaderInterface.OnTagsRecord = OnReaderTagsRecord;

                    }
                    if (Categories == null || !Categories.Any())
                    {
                        try
                        {
                            await GetPlateOrTray();
                        }
                        catch (Exception ex)
                        {
                            SeriLogService.LogError(ex.ToString());
                            //Execute.OnUIThread(() =>
                            //{
                            //    ShowMessageDialog("Cannot get data from Cloud");
                            //});
                        }
                    }

                }
            });
        }

        private async Task GetPlateOrTray()
        {
            if (IsPlate)
            {
                Categories = await MbCloudService.GetPlateCategories();
                NotifyOfPropertyChange(() => Categories);
            }
            else
            {
                var data = await MbCloudService.GetAllTray();
                if (data != null)
                {
                    Plates = data.Select(x => x.Plate).ToList();
                    NotifyOfPropertyChange(() => Plates);
                }
            }
        }

        public async Task ClearData()
        {
            Tags = new List<TagDTO>();
            SelectedPlate = null;
            SelectedCategory = null;
            try
            {
                await GetPlateOrTray();
            }
            catch (Exception ex)
            {
                SeriLogService.LogError(ex.ToString());
                Execute.OnUIThread(() =>
                {
                    ShowMessageDialog("Cannot get data from Cloud");
                });
            }
        }

        public void OnReaderTagsRecord(List<TagDTO> tags)
        {

            if (tags == null || !tags.Any())
            {
                Tags = new List<TagDTO>();
                NotifyOfPropertyChange(() => Tags);
                return;
            }

            bool isChanged = false;

            var removed = Tags.Where(x => !tags.Any(t => t.UID == x.UID && t.PlateModel == x.PlateModel)).ToList();
            if (removed.Any())
            {
                for (int i = 0; i < removed.Count(); i++)
                {
                    SeriLogService.LogInfo($"Removed tag {removed[i].UID}");
                    Tags.Remove(removed[i]);
                    isChanged = true;
                }
            }

            var added = tags.Where(x => !Tags.Any(t => t.UID == x.UID)).ToList();
            if (added.Any())
            {
                for (int i = 0; i < added.Count(); i++)
                {
                    Tags.Add(new TagDTO { UID = added[i].UID, PlateModel = added[i].PlateModel });
                    SeriLogService.LogInfo($"Added tag {added[i].UID}");
                    isChanged = true;
                }
            }

            if (isChanged)
            {
                Tags = new List<TagDTO>(Tags);
                NotifyOfPropertyChange(() => Tags);
                SeriLogService.LogInfo($"Tags read result: {string.Join(", ", tags)}");
            }
        }

        public void WriteTags()
        {
            //var confirmMessage = "Are you sure want to write data to tags?";
            //var confirm = System.Windows.MessageBox.Show(confirmMessage, "Confirm", MessageBoxButton.OKCancel);
            //if (confirm != MessageBoxResult.OK)
            //{
            //    return;
            //}
            try
            {
                var result = RfidReaderInterface.WriteTagsData(Tags, SelectedPlate.Code);
                if (result)
                {
                    if (IsPlate)
                    {
                        ShowMessageDialog($"Plate code: {SelectedPlate.Code} has been written to tags succesfully.\nPlease remove your tags!");
                    }
                    else
                    {
                        ShowMessageDialog($"Tray code: {SelectedPlate.Code} has been written to tags succesfully.\nPlease remove your tags!");
                    }
                }
                else
                {
                    ShowMessageDialog("Error.\nPlease try again!");
                }
            }
            catch (Exception exception)
            {
                SeriLogService.LogError($"{exception.Message}\n{exception.StackTrace}");
                ShowMessageDialog("Error.\nPlease try again!");
            }
        }

        public void CheckIfCanWrite()
        {
            CanWrite = SelectedPlate != null && Tags != null && Tags.Any();
        }
        public override void Handle(AppMessage message)
        {
            if (ShellView.ToggleMenu || ShellView.CurrentScreen != Screen.Main) return;
            if (message is ClosingFormMessage)
            {
                RfidReaderInterface.FormClosing();
            }
        }
    }
    public class CSupportedAirProtocol
    {
        public UInt32 m_ID;
        public string m_name;
        public Boolean m_en;
    }
}