using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using WindowsApplication2;

namespace KonbiBrain.RfidTable.TakeAwayPrice
{
    public partial class FormWritePriceVer2 : Form
    {
        public UIntPtr hreader;
        public ArrayList readerDriverInfoList = new ArrayList();
        public ArrayList airInterfaceProtList = new ArrayList();

        public Byte[] AntennaSel = new byte[16];
        public Byte AntennaSelCount = 0;

        public Byte onlyNewTag = 1;
        public Byte enableAFI = 0;
        public Byte AFI = 0;
        Thread InvenThread;
        public bool _shouldStop;

        public Byte inventoryState = 0;
        private string format = "dd/MM/yyy HH:mm:ss";
        private readonly System.Timers.Timer scanningTimer;
        public UIntPtr hTag;
        public UIntPtr hTagIcodeslix;
        public UIntPtr hTagTiHFIPlus;
        public Byte openState = 0;

        string inputText = "";
        private List<ListDataItem> dataItems = new List<ListDataItem>();

        // Number of classes to classify
        int NUM_LINE = 0;
        public static bool CotfRunning { get; set; }
        public static bool TagMappingRunning { get; set; }
        /// <summary>
        /// InitializeComponent
        /// </summary>
        public FormWritePriceVer2()
        {
            InitializeComponent();

            try
            {
                Process[] cotfs = Process.GetProcessesByName("KonbiBrain.WindowServices.CotfPad");
                foreach (Process worker in cotfs)
                {
                    CotfRunning = true;
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                }
                var tagMappings = Process.GetProcessesByName("Konbini.RfidTable.ProductTagMapping");
                foreach (Process worker in tagMappings)
                {
                    TagMappingRunning = true;
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                }
            }
            catch (System.Exception ex)
            {

            }

            scanningTimer = new System.Timers.Timer();
            scanningTimer.Interval = 2000;
            scanningTimer.Enabled = true;
            scanningTimer.Elapsed += ScanningTimer_Elapsed;
            scanningTimer.Start();

            hTag = (UIntPtr)0;
            hTagIcodeslix = (UIntPtr)0;
            hTagTiHFIPlus = (UIntPtr)0;
        }

        /// <summary>
        /// ScanningTimer_Elapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanningTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //this.Invoke(new Action(() =>
            //{
            //    bool isConnect = GetReaderInfo();
            //    if (isConnect)
            //    {
            //        label2.Text = "Connected";
            //    }
            //    else
            //    {
            //        label2.Text = "Disconnected";
            //    }
            //}));
        }

        /// <summary>
        /// Form load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormWritePriceVer2_Load(object sender, EventArgs e)
        {
            // Load and create button shortcuts price.
            CreateDynamicButton();

            // Load history.
            readHistory();

            InitData();

            Byte usbOpenType = (byte)0;
            Byte readerType = 0;
            int iret = 0;
            UInt32 antennaCount = 0;

            /*
             * Try to open communcation layer for specified reader 
             */
            int commTypeIdx = 1;
            string readerDriverName = "M201"; //((CReaderDriverInf)(readerDriverInfoList[readerType])).m_name;
            string connstr = "";
            // Build serial communication connection string
            if (commTypeIdx == 1) //usb type
            {
                connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_USB + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDADDRMODE + "=" + usbOpenType.ToString() + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDSERNUM + "=" + "";
            }

            // Call required to open reader driver
            iret = RFIDLIB.rfidlib_reader.RDR_Open(connstr, ref hreader);
            if (iret != 0)
            {
                /*
                 *  Open fail:
                 *  if you Encounter this error ,make sure you has called the API "RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers("\\Drivers")" 
                 *  when application load
                 */
                Log.Error($"Error when call DLIB.rfidlib_reader.RDR_Open {connstr}");
                //MessageBox.Show("Open reader failed!");
                FormMessageBox.showMessageBox(true, "Open reader failed!");

            }
            else
            {
                //MessageBox.Show("ok");
                /*
                 * Open Ok and try to get some information from driver ,and assign value to the correspondding control 
                 */

                // this API is not required in your own application
                // Get antenna count
                antennaCount = RFIDLIB.rfidlib_reader.RDR_GetAntennaInterfaceCount(hreader);

                UInt32 index = 0;
                UInt32 AIType;
                while (true)
                {
                    AIType = 0;
                    iret = RFIDLIB.rfidlib_reader.RDR_GetSupportedAirInterfaceProtocol(hreader, index, ref AIType);
                    if (iret != 0)
                    {
                        break;
                    }

                    StringBuilder namebuf = new StringBuilder();
                    namebuf.Append('\0', 128);
                    RFIDLIB.rfidlib_reader.RDR_GetAirInterfaceProtName(hreader, AIType, namebuf,
                        (UInt32)namebuf.Capacity);

                    CSupportedAirProtocol aip = new CSupportedAirProtocol();
                    aip.m_ID = AIType;
                    aip.m_name = namebuf.ToString();
                    aip.m_en = true;
                    airInterfaceProtList.Add(aip);
                    index++;
                }
            }

            InvenThread = new Thread(DoInventory);
            InvenThread.Start();
            openState = 1;

            // Create task for listen disconnect card reader.
            createTaskListenConnect(iret, antennaCount, connstr);
        }
        private object lockHeaderObject = new object();
        /// <summary>
        /// Create task for listen disconnect card reader.
        /// </summary>
        /// <param name="iret"></param>
        /// <param name="antennaCount"></param>
        /// <param name="connstr"></param>
        private void createTaskListenConnect(int iret, UInt32 antennaCount, string connstr)
        {
            // Create task for listen disconnect card reader.
            Task.Run(async () =>
            {
                while (true)
                {

                    // Check card reader is open.
                    //iret = RFIDLIB.rfidlib_reader.RDR_Open(connstr, ref hreader);
                    //if (iret != 0)
                    //{
                    //    // Get antenna count
                    //    antennaCount = RFIDLIB.rfidlib_reader.RDR_GetAntennaInterfaceCount(hreader);

                    //    UInt32 index = 0;
                    //    UInt32 AIType;
                    //    while (true)
                    //    {
                    //        AIType = 0;
                    //        iret = RFIDLIB.rfidlib_reader.RDR_GetSupportedAirInterfaceProtocol(hreader, index, ref AIType);
                    //        if (iret != 0)
                    //        {
                    //            break;
                    //        }

                    //        StringBuilder namebuf = new StringBuilder();
                    //        namebuf.Append('\0', 128);
                    //        RFIDLIB.rfidlib_reader.RDR_GetAirInterfaceProtName(hreader, AIType, namebuf,
                    //            (UInt32)namebuf.Capacity);

                    //        CSupportedAirProtocol aip = new CSupportedAirProtocol();
                    //        aip.m_ID = AIType;
                    //        aip.m_name = namebuf.ToString();
                    //        aip.m_en = true;
                    //        bool aipExisting = false;
                    //        foreach(CSupportedAirProtocol x in airInterfaceProtList)
                    //        {
                    //            if(x.m_ID == aip.m_ID && x.m_name == aip.m_name && x.m_en == aip.m_en)
                    //            {
                    //                aipExisting = true;
                    //                break;
                    //            }
                    //        }
                    //       if (!aipExisting)
                    //            airInterfaceProtList.Add(aip);
                    //        index++;
                    //    }

                    //}
                    //this.Invoke(new Action(() =>
                    //{
                    try
                    {
                        bool isConnect = GetReaderInfo();
                        if (isConnect)
                        {
                            label2.Invoke((MethodInvoker)delegate { label2.Text = "Connected"; });
                        }
                        else
                        {
                            label2.Invoke((MethodInvoker)delegate { label2.Text = "Disconnected"; });
                            lock (lockHeaderObject)
                            {
                                if(hreader!= UIntPtr.Zero)
                                {
                                    RFIDLIB.rfidlib_reader.RDR_Close(hreader);
                                    hreader = UIntPtr.Zero;
                                    if (listView1.Items.Count > 0)
                                        listView1.Invoke((MethodInvoker)delegate {
                                            listView1.Items.Clear();
                                        });
                                        
                                }
                             
                                RFIDLIB.rfidlib_reader.RDR_Open(connstr, ref hreader);
                            }
                           

                        }
                        if (isConnect)
                        {
                            if (!InvenThread.IsAlive)
                            {
                                InvenThread = new Thread(DoInventory);
                                InvenThread.Start();
                            }

                        }
                        else
                        {
                            InvenThread.Abort();
                        }
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        await Task.Delay(1000); //  reconnect after 1s
                    }
                    //}));

                }
            });
        }

        /// <summary>
        /// labelPrice_KeyPress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void labelPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check keypress.
            if (((char)47 >= e.KeyChar || e.KeyChar >= (char)58) && e.KeyChar != (char)13 && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Is digital.
            if ((char)47 < e.KeyChar && e.KeyChar < (char)58)
            {
                inputText += e.KeyChar;
                labelPrice.Text = string.Format("{0:C}", Convert.ToDecimal(inputText) / 100);
                e.Handled = true;
            }

            // Check click reset.
            if (e.KeyChar == (char)46)
            {
                //listView1.Items.Clear();
                resetClick();
                e.Handled = true;
            }

            // Check click enter.
            if (e.KeyChar == (char)13)
            {
                savePrice();
            }

            // Set focus.
            labelPrice.Focus();
            labelPrice.SelectionStart = labelPrice.Text.Length;
        }

        /// <summary>
        /// Close form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCloseForm_Click(object sender, EventArgs e)
        {
            _shouldStop = true;
            InvenThread.Abort();
            Application.Exit();
        }

        #region Calculator.
        /// <summary>
        /// When click button number.
        /// </summary>
        /// <param name="number">Value of button number.</param>
        private void buttonClick(string number)
        {
            inputText += number;
            labelPrice.Text = string.Format("{0:C}", Convert.ToDecimal(inputText) / 100);
        }

        /// <summary>
        /// When click button reset.
        /// </summary>
        private void resetClick()
        {
            //listView1.Items.Clear();
            labelPrice.Text = "";
            inputText = "";
            labelPrice.Focus();
        }

        /// <summary>
        /// Click button reset.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberReset_Click(object sender, EventArgs e)
        {
            resetClick();
        }

        /// <summary>
        /// Click number 1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberOne_Click(object sender, EventArgs e)
        {
            buttonClick("1");
        }

        /// <summary>
        /// Click number 2.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberTwo_Click(object sender, EventArgs e)
        {
            buttonClick("2");
        }

        /// <summary>
        /// Click number 3.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberThree_Click(object sender, EventArgs e)
        {
            buttonClick("3");
        }

        /// <summary>
        /// Click number 4.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberFour_Click(object sender, EventArgs e)
        {
            buttonClick("4");
        }

        /// <summary>
        /// Click number 5.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberFive_Click(object sender, EventArgs e)
        {
            buttonClick("5");
        }

        /// <summary>
        /// Click number 6.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberSix_Click(object sender, EventArgs e)
        {
            buttonClick("6");
        }

        /// <summary>
        /// Click number 7.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberSeven_Click(object sender, EventArgs e)
        {
            buttonClick("7");
        }

        /// <summary>
        /// Click number 8.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberEight_Click(object sender, EventArgs e)
        {
            buttonClick("8");
        }

        /// <summary>
        /// Click number 9.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberNine_Click(object sender, EventArgs e)
        {
            buttonClick("9");
        }

        /// <summary>
        /// Click number 0.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberZero_Click(object sender, EventArgs e)
        {
            buttonClick("0");
        }

        /// <summary>
        /// Click button dot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberDot_Click(object sender, EventArgs e)
        {
            buttonClick(".");
        }

        /// <summary>
        /// Click button enter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberEnter_Click(object sender, EventArgs e)
        {
            savePrice();
        }

        /// <summary>
        /// Save price.
        /// </summary>
        private void savePrice()
        {
            // Validate empty price.
            if (labelPrice.Text == "")
            {
                FormMessageBox.showMessageBox(true, "Please set price takeaway!");
                return;
            }


            // Check decimal.
            string[] arrayTemp = labelPrice.Text.Split(new char[] { '.' });
            if (labelPrice.Text.IndexOf(".") == -1 || arrayTemp[1].Length == 1)
            {
                FormMessageBox.showMessageBox(true, "Invalid price");
                return;
            }

            if (listView1.Items.Count == 0)
            {
                FormMessageBox.showMessageBox(true, "Please put new takeaway plate!");
                inputText = "";
                return;
            }

            //Save.
            try
            {
                InvenThread.Abort();
                _shouldStop = true;
                var oneTagSaveSuccess = false;
                var allTagSaveSuccess = true;
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    var uuid = listView1.Items[i].SubItems[0].Text;

                    if (ConnectTag(uuid))
                    {
                        var price = Convert.ToInt32(Convert.ToDouble(labelPrice.Text.Substring(1)) * 100);
                        var data = price.ToString("D8");
                        Log.Information($"Set price {price}");
                        var result = WriteData(10, data);
                        if (result)
                            oneTagSaveSuccess = true;
                        else
                        {
                            allTagSaveSuccess = false;
                        }
                        DisConnectTag();
                    }
                    else
                    {
                        allTagSaveSuccess = false;
                    }

                }
                if (allTagSaveSuccess)
                {
                    // Add history.
                    addHistory(listView1);
                    FormMessageBox.showMessageBox(false, "Set price succesfully.\n Please remove your takeaway plate!");
                }
                else
                {
                    if (oneTagSaveSuccess)
                    {
                        FormMessageBox.showMessageBox(true, "Some of tags are not written sucessful. please double check");
                    }
                    else
                    {
                        FormMessageBox.showMessageBox(true, "Cannot write price to tags");
                    }
                }

                // Reset label.
                resetClick();



            }
            catch (Exception exception)
            {
                Log.Error(exception, exception.Message);
                FormMessageBox.showMessageBox(true, "There was error, please try again!");
                DisConnectTag();
            }
            finally
            {
                listView1.Items.Clear();

                dataItems.Clear();
                _shouldStop = false;

                InitData();
                InvenThread = new Thread(DoInventory);
                InvenThread.Start();
            }
        }

        /// <summary>
        /// Add history.
        /// </summary>
        /// <param name="listView1"></param>
        private void addHistory(ListView listView1)
        {
            Log.Information("Start add new history.");
            foreach (var item in listView1.Items)
            {
                history03.Text = history02.Text;
                history02.Text = history01.Text;
                history01.Text = labelPrice.Text + " set at " + DateTime.Now.ToString("hh:mm tt");
            }

            // Create and write history.
            createAndWriteHistory();
        }

        /// <summary>
        /// Create and write history.
        /// </summary>
        private void createAndWriteHistory()
        {
            Log.Information("Start create and write history.");
            string fileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString() + "\\History.txt";
            try
            {
                // Check if file already exists. If yes, delete it.     
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                // Create a new file     
                using (FileStream fs = File.Create(fileName))
                {
                    // Add some text to file    
                    Byte[] title = new UTF8Encoding(true).GetBytes("History");
                    fs.Write(title, 0, title.Length);
                    byte[] author = new UTF8Encoding(true).GetBytes("TrungPQ");
                    fs.Write(author, 0, author.Length);
                }

                // Open the stream and write it back.    
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.WriteLine(history01.Text);
                    writer.WriteLine(history02.Text);
                    writer.WriteLine(history03.Text);
                }
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "Error write history:" + Ex.ToString());
            }
        }

        /// <summary>
        /// Read history.
        /// </summary>
        private void readHistory()
        {
            Log.Information("Start read history.");
            string fileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString() + "\\History.txt";
            // Check if file already exists.   
            if (File.Exists(fileName))
            {
                // Open the stream and read it back.    
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string s = "";
                    int index = 1;
                    while ((s = sr.ReadLine()) != null)
                    {
                        switch (index)
                        {
                            case 1:
                                history01.Text = s;
                                index++;
                                break;
                            case 2:
                                history02.Text = s;
                                index++;
                                break;
                            default:
                                history03.Text = s;
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Merge source code from Mr Ha.
        /// <summary>
        /// tag_inventory.
        /// </summary>
        /// <param name="AIType"></param>
        /// <param name="AntennaSelCount"></param>
        /// <param name="AntennaSel"></param>
        /// <param name="enable15693"></param>
        /// <param name="enable14443A"></param>
        /// <param name="enable18000p3m3"></param>
        /// <param name="enableAFI"></param>
        /// <param name="afiVal"></param>
        /// <param name="tagReportHandler"></param>
        /// <param name="nTagCount"></param>
        /// <returns></returns>
        public int tag_inventory(
            Byte AIType,
            Byte AntennaSelCount,
            Byte[] AntennaSel,
            Byte enableAFI,
            Byte afiVal,
            delegate_tag_report_handle tagReportHandler,
            delegate_multiple_tags_report_handle multipleTagsReportHandler,
            ref UInt32 nTagCount)
        {
            try
            {
               // lock (lockHeaderObject) { 
                Thread.Sleep(100);
                int iret;
                UIntPtr InvenParamSpecList = UIntPtr.Zero;
                var TagList = new List<TagInfoV2>();
                InvenParamSpecList = RFIDLIB.rfidlib_reader.RDR_CreateInvenParamSpecList();
                if (InvenParamSpecList.ToUInt64() != 0)
                {
                    RFIDLIB.rfidlib_aip_iso15693.ISO15693_CreateInvenParam(InvenParamSpecList, 0, 0, 0, 0);
                }

                nTagCount = 0;
            LABEL_TAG_INVENTORY:
                iret = RFIDLIB.rfidlib_reader.RDR_TagInventory(hreader, AIType, AntennaSelCount, AntennaSel,
                    InvenParamSpecList);
                if (iret == 0 || iret == -21)
                {
                    nTagCount += RFIDLIB.rfidlib_reader.RDR_GetTagDataReportCount(hreader);
                    UIntPtr TagDataReport;
                    TagDataReport = (UIntPtr)0;
                    if (TagList.Count > 0)
                        TagList.Clear();
                    TagDataReport =
                        RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_FIRST); //first
                    while (TagDataReport.ToUInt64() > 0)
                    {

                        UInt32 aip_id = 0;
                        UInt32 tag_id = 0;
                        UInt32 ant_id = 0;
                        Byte dsfid = 0;
                        Byte uidlen = 0;
                        Byte[] uid = new Byte[16];

                        iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ParseTagDataReport(TagDataReport, ref aip_id,
                            ref tag_id, ref ant_id, ref dsfid, uid);
                        if (iret == 0)
                        {
                            uidlen = 8;
                            object[] pList = { aip_id, tag_id, ant_id, uid, (int)uidlen };
                            if (!this._shouldStop) Invoke(tagReportHandler, pList);

                            var tag = new TagInfoV2();
                            tag.TagId = BitConverter.ToString(uid, 0, (int)uidlen).Replace("-", string.Empty);
                            // connect to a tag
                            uint tagType = 7;// slix
                            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_Connect(hreader, tagType, 1, uid, ref hTag);

                            if (iret == 0)
                            {
                                /* connect ok */
                                // plate model
                                tag.ModelNumber = ReadData(4).TrimStart('0');
                                //custom price
                                tag.Data.Add(10, ReadData(10));
                                TagList.Add(tag);
                                iret = RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
                                if (iret == 0)
                                {
                                    hTag = (UIntPtr)0;
                                }
                                else
                                {
                                    Log.Error($"DisConnectTag {tag.TagId} - failed");
                                }
                            }
                            else
                            {
                                Log.Error($"ConnectTag {tag.TagId}- failed");
                            }

                        }
                        /* Get Next report from buffer */
                        TagDataReport =
                            RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
                    }
                    if (!this._shouldStop) Invoke(multipleTagsReportHandler, TagList);
                    if (iret == -21) // stop trigger occur,need to inventory left tags
                    {
                        AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE; //use only-new-tag inventory 
                        Thread.Sleep(100);
                        goto LABEL_TAG_INVENTORY;
                    }

                    iret = 0;
                }

                if (InvenParamSpecList.ToUInt64() != 0) RFIDLIB.rfidlib_reader.DNODE_Destroy(InvenParamSpecList);
                return iret;
               // }
            }
            catch (Exception e)
            {
                Log.Error(e, "");
                return -1;
            }
        }

        public int TagInventory(Byte AIType, Byte AntennaSelCount, Byte[] AntennaSel, ref UInt32 nTagCount)
        {

            int iret;
            UIntPtr InvenParamSpecList = UIntPtr.Zero;
            InvenParamSpecList = RFIDLIB.rfidlib_reader.RDR_CreateInvenParamSpecList();
            if (InvenParamSpecList.ToUInt64() != 0)
            {
                RFIDLIB.rfidlib_aip_iso15693.ISO15693_CreateInvenParam(InvenParamSpecList, 0, 0, 0, 0);
            }
            nTagCount = 0;
        LABEL_TAG_INVENTORY:
            iret = RFIDLIB.rfidlib_reader.RDR_TagInventory(hreader, AIType, AntennaSelCount, AntennaSel, InvenParamSpecList);
            if (iret == 0 || iret == -21)
            {
                var tags = new List<string>();
                nTagCount += RFIDLIB.rfidlib_reader.RDR_GetTagDataReportCount(hreader);
                UIntPtr TagDataReport;
                TagDataReport = (UIntPtr)0;
                TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_FIRST); //first
                                                                                                                           //OldHashTags = HashTags;
                                                                                                                           //OldTags = Tags;
                while (TagDataReport.ToUInt64() > 0)
                {
                    UInt32 aip_id = 0;
                    UInt32 tag_id = 0;
                    UInt32 ant_id = 0;
                    Byte dsfid = 0;
                    Byte uidlen = 0;
                    Byte[] uid = new Byte[16];

                    iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ParseTagDataReport(TagDataReport, ref aip_id, ref tag_id, ref ant_id, ref dsfid, uid);
                    if (iret == 0)
                    {
                        uidlen = 8;
                        var tagId = BitConverter.ToString(uid, 0, (int)uidlen).Replace("-", string.Empty);
                        tags.Add(tagId);
                    }
                    TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
                }

                //Tags = tags;
                //var orderedTags = tags.OrderBy(x => x);
                //HashTags = string.Join("|", orderedTags);
                //if (OldHashTags != HashTags)
                //{
                //    OnTagsRecord?.Invoke(Tags);
                //    if (OldHashTags.Length > HashTags.Length)
                //    {

                //        var removed = OldTags.Except(Tags);
                //        Console.WriteLine("Removed: " + string.Join("|", removed));
                //    }
                //    if (OldHashTags.Length < HashTags.Length)
                //    {
                //        var added = Tags.Except(OldTags);
                //        Console.WriteLine("Added: " + string.Join("|", added));
                //    }
                //    if (OldHashTags.Length == HashTags.Length)
                //    {
                //    }
                //}
                if (iret == -21)
                {
                    AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;//use only-new-tag inventory 
                    goto LABEL_TAG_INVENTORY;
                }
                iret = 0;
            }
            if (InvenParamSpecList.ToUInt64() != 0) RFIDLIB.rfidlib_reader.DNODE_Destroy(InvenParamSpecList);
            return iret;
        }

        /// <summary>
        /// UpdateTotalPlates.
        /// </summary>
        private void UpdateTotalPlates()
        {
            foreach (ListViewItem item in listView1.Items)
            {
                Log.Information($"Plate: uuid {item.SubItems[0]} model {item.SubItems[1]} price {item.SubItems[2]}");
            }
            // TODO: Check
            //lblTotalPlates.Text = $"Total plate(s): {listView1.Items.Count}";

        }

        /// <summary>
        /// ReadData
        /// </summary>
        /// <param name="blkAddress"></param>
        /// <returns></returns>
        private string ReadData(uint blkAddress)
        {
            int iret;
            UInt32 blocksRead = 0;

            //blockToRead = (UInt32)(idx + 1);
            UInt32 nSize;
            Byte[] BlockBuffer = new Byte[40];

            nSize = (UInt32)BlockBuffer.GetLength(0);
            UInt32 bytesRead = 0;
            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ReadMultiBlocks(hreader, hTag, 0, blkAddress, 1,
                ref blocksRead, BlockBuffer, nSize, ref bytesRead);
            if (iret == 0)
            {
                //blocksRead: blocks read 
                var txt = BitConverter.ToString(BlockBuffer, 0, (int)bytesRead).Replace("-", string.Empty);

                return txt;
            }
            else
            {
                Log.Error($"ReadData {blkAddress}- failed");
                //MessageBox.Show("Read data failed");
                //FormMessageBox.showMessageBox(true, "Read data failed!");
                return "";
            }
        }
        public class TagInfoV2
        {
            public string TagId { get; set; }
            public string ModelNumber { get; set; }
            /// <summary>
            /// Block index, data
            /// </summary>
            public Dictionary<int, string> Data { get; set; }
            public TagInfoV2()
            {
                Data = new Dictionary<int, string>();
            }
        }
        /// <summary>
        /// delegate_tag_report_handle.
        /// </summary>
        /// <param name="AIPType"></param>
        /// <param name="tagType"></param>
        /// <param name="antID"></param>
        /// <param name="uid"></param>
        /// <param name="uidlen"></param>
        public delegate void delegate_multiple_tags_report_handle(IEnumerable<TagInfoV2> tags);

        public void detect_multiple_tags_report_handler(IEnumerable<TagInfoV2> tags)
        {

            listView1.Items.Clear();
            tags.ToList().OrderBy(el=> el.TagId).ToList().ForEach(tag =>
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = tag.TagId;
                lvi.SubItems.Add(tag.ModelNumber);
                if (tag.Data.TryGetValue(10, out string customData))
                {
                    float price = 0;
                    float.TryParse(customData, out price);
                    lvi.SubItems.Add((price / 100).ToString("C"));

                }
                else
                {
                // default custom price = 0
                lvi.SubItems.Add(0.ToString("C"));
                }

                lvi.SubItems.Add(DateTime.Now.ToString(format));
                listView1.Items.Add(lvi);
            //dataItems.Add(new ListDataItem { UUID = tag.TagId, UpdatedDate = DateTime.Now });
            // Log.Information($"Added plate uuid { tag.TagId} model {tag.ModelNumber} price {customData}");                
        });

        }

        /// <summary>
        /// delegate_tag_report_handle.
        /// </summary>
        /// <param name="AIPType"></param>
        /// <param name="tagType"></param>
        /// <param name="antID"></param>
        /// <param name="uid"></param>
        /// <param name="uidlen"></param>
        public delegate void delegate_tag_report_handle(UInt32 AIPType, UInt32 tagType, UInt32 antID, Byte[] uid,
            int uidlen);

        /// <summary>
        /// dele_tag_report_handler.
        /// </summary>
        /// <param name="AIPType"></param>
        /// <param name="tagType"></param>
        /// <param name="antID"></param>
        /// <param name="uid"></param>
        /// <param name="uidlen"></param>
        public void detect_tag_report_handler(UInt32 AIPType, UInt32 tagType, UInt32 antID, Byte[] uid, int uidlen)
        {
            return;
            // TrungPQ add logic: When start reader then clear listview1.
            //listView1.Items.Clear();

            String strUid;
            int iret;
            String strAIPName, strTagTypeName;
            StringBuilder sbAIPName = new StringBuilder();
            sbAIPName.Append('\0', 128);
            UInt32 nSize = (UInt32)sbAIPName.Capacity;
            iret = RFIDLIB.rfidlib_reader.RDR_GetAIPTypeName(hreader, AIPType, sbAIPName, ref nSize);
            if (iret != 0)
            {
                strAIPName = "Unknown";
            }
            else
            {
                strAIPName = sbAIPName.ToString();
            }

            StringBuilder sbTagName = new StringBuilder();
            sbTagName.Append('\0', 128);
            nSize = (UInt32)sbTagName.Capacity;
            iret = RFIDLIB.rfidlib_reader.RDR_GetTagTypeName(hreader, AIPType, tagType, sbTagName, ref nSize);
            if (iret != 0)
            {
                strTagTypeName = "Unknown";
            }
            else
            {
                strTagTypeName = sbTagName.ToString();
            }

            strUid = BitConverter.ToString(uid, 0, (int)uidlen).Replace("-", string.Empty);

            bool found = false;
            int i;
            for (i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[0].Text == strUid)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                bool checkRead = false;
                ListViewItem lvi = new ListViewItem();
                lvi.Text = strUid;

                ConnectTag(strUid);
                listView1.Items.Clear();
                var model = ReadData(4);
                // TrungPQ add When can't read.
                if (model != "")
                {
                    checkRead = true;
                    lvi.SubItems.Add(model.Substring(4, 4));
                }

                var customData = "";
                if (checkRead)
                {
                    customData = ReadData(10);
                }

                float price = 0;
                float.TryParse(customData, out price);
                lvi.SubItems.Add((price / 100).ToString("C"));
                inputText = "";
                //labelPrice.Text = (price / 100).ToString("C");

                DisConnectTag();

                lvi.SubItems.Add(DateTime.Now.ToString(format));
                listView1.Items.Add(lvi);
                dataItems.Add(new ListDataItem { UUID = strUid, UpdatedDate = DateTime.Now });
                Log.Information($"Added plate uuid {strUid} model {model} price {customData}");

                UpdateTotalPlates();
            }
            else
            {
                dataItems[i].UpdatedDate = DateTime.Now;
            }
        }

        /// <summary>
        /// delegateInventoryFinishCallback.
        /// </summary>
        private delegate void delegateInventoryFinishCallback();

        /// <summary>
        /// InventoryFinishCallback.
        /// </summary>
        public void InventoryFinishCallback()
        {
            inventoryState = 0;
        }
        private delegate_tag_report_handle cbTagReportHandle;
        private delegate_multiple_tags_report_handle cbMultipleTagReportHandle;
        /// <summary>
        /// DoInventory
        /// </summary>
        public void DoInventory()
        {
            if (cbTagReportHandle == null)
                cbTagReportHandle = new delegate_tag_report_handle(detect_tag_report_handler);
            if (cbMultipleTagReportHandle == null)
                cbMultipleTagReportHandle = new delegate_multiple_tags_report_handle(detect_multiple_tags_report_handler);




            int iret;
            Byte AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
            if (onlyNewTag == 1)
            {
                AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE; //only new tag inventory 
            }

            while (!_shouldStop)
            {
                UInt32 nTagCount = 0;
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                iret = tag_inventory(AIType, AntennaSelCount, AntennaSel, enableAFI, AFI, cbTagReportHandle, cbMultipleTagReportHandle, ref nTagCount);
                Debug.WriteLine($"end tag_inventory: {stopWatch.ElapsedMilliseconds}");
                // iret = TagInventory(AIType,AntennaSelCount,AntennaSel ref )
                if (iret == 0)
                {
                    // inventory ok
                }
                else
                {
                    // inventory error 
                }

                AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
                Thread.Sleep(300);
            }

            //object[] pFinishList = { };
            //Invoke(new delegateInventoryFinishCallback(InventoryFinishCallback), pFinishList);

            /*
             *  If API RDR_SetCommuImmeTimeout is called when stop, API RDR_ResetCommuImmeTimeout 
             *  must be called too, Otherwise, an error -5 may occurs .
             */
            RFIDLIB.rfidlib_reader.RDR_ResetCommuImmeTimeout(hreader);
        }

        /// <summary>
        /// InitData.
        /// </summary>
        private void InitData()
        {
            /* 
             *  Call required, when application load ,this API just only need to load once
             *  Load all reader driver dll from drivers directory, like "rfidlib_ANRD201.dll"  
             */
            Log.Information("Start load drivers.");
            RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers("\\Drivers");

            /*
             * Not call required,it can be Omitted in your own appliation
             * enum and show loaded reader driver 
             */
            //UInt32 nCount;
            //nCount = RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverCount();
            //uint i;
            //for (i = 0; i < nCount; i++)
            //{
            //    UInt32 nSize;
            //    CReaderDriverInf driver = new CReaderDriverInf();
            //    StringBuilder strCatalog = new StringBuilder();
            //    strCatalog.Append('\0', 64);

            //    nSize = (UInt32)strCatalog.Capacity;
            //    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_CATALOG,
            //        strCatalog, ref nSize);
            //    driver.m_catalog = strCatalog.ToString();
            //    if (driver.m_catalog == RFIDLIB.rfidlib_def.RDRDVR_TYPE_READER) // Only reader we need
            //    {
            //        StringBuilder strName = new StringBuilder();
            //        strName.Append('\0', 64);
            //        nSize = (UInt32)strName.Capacity;
            //        RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_NAME,
            //            strName, ref nSize);
            //        driver.m_name = strName.ToString();

            //        StringBuilder strProductType = new StringBuilder();
            //        strProductType.Append('\0', 64);
            //        nSize = (UInt32)strProductType.Capacity;
            //        RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_ID,
            //            strProductType, ref nSize);
            //        driver.m_productType = strProductType.ToString();

            //        StringBuilder strCommSupported = new StringBuilder();
            //        strCommSupported.Append('\0', 64);
            //        nSize = (UInt32)strCommSupported.Capacity;
            //        RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i,
            //            RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_COMMTYPESUPPORTED, strCommSupported, ref nSize);
            //        driver.m_commTypeSupported = (UInt32)int.Parse(strCommSupported.ToString());

            //        readerDriverInfoList.Add(driver);
            //    }

            //}
        }

        /// <summary>
        /// WriteData.
        /// </summary>
        /// <param name="blkAddress"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool WriteData(uint blkAddress, string data)
        {
            int iret;
            byte[] newBlksData = StringToByteArrayFastest(data);

            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_WriteMultipleBlocks(hreader, hTag, blkAddress, 1, newBlksData,
                (uint)newBlksData.Length);
            if (iret == 0)
            {
                //MessageBox.Show("ok !");
                return true;
            }
            else
            {
                Log.Error($"WriteData {blkAddress} {data}- failed");
                //MessageBox.Show("failed!err = " + iret);
                FormMessageBox.showMessageBox(true, "Write data false, please try again!");
                DisConnectTag();
                return false;
            }
        }

        /// <summary>
        /// GetHexVal.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        /// <summary>
        /// StringToByteArrayFastest.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            int len = hex.Length >> 1;
            byte[] arr = new byte[len];

            for (int i = 0; i < len; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        /// <summary>
        /// ConnectTag
        /// </summary>
        /// <param name="uuid">uuid dish.</param>
        private bool ConnectTag(string uuid)
        {
            int iret;
            string suid;
            int idx;
            if (hTag != UIntPtr.Zero)
            {
                DisConnectTag();

            }

            idx = 1;
            // set uid

            byte[] uid = StringToByteArrayFastest(uuid);

            //set tag type default is NXP icode sli 
            UInt32 tagType = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
            tagType = 7; //slix


            // set address mode 
            Byte addrMode = (Byte)idx;

            // do connection
            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_Connect(hreader, tagType, addrMode, uid, ref hTag);
            if (iret == 0)
            {
                /* 
                * if select none address mode after inventory need to reset the tag first,because the tag is stay quiet now  
                * if the tag is in ready state ,do not need to call reset
                */
                if (addrMode == 0)
                {
                    iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_Reset(hreader, hTag);
                    if (iret != 0)
                    {
                        MessageBox.Show("reset tag fail");
                        RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
                        hTag = UIntPtr.Zero;
                        return false;
                    }
                }
                /* connect ok */
                return true;

            }
            else
            {
                Log.Error($"ConnectTag {uuid}- failed");

            }
            return false;
        }


        /// <summary>
        /// DisConnectTag.
        /// </summary>
        private void DisConnectTag()
        {
            int iret;

            // do disconnection
            iret = RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
            if (iret == 0)
            {
                hTag = (UIntPtr)0;
            }
            else
            {
                Log.Error("DisConnectTag - failed");

            }
        }
        #endregion

        /// <summary>
        /// Form close.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormWritePriceVer2_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                scanningTimer?.Stop();
                InvenThread.Abort();

            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// GetReaderInfor.
        /// </summary>
        /// <returns></returns>
        private bool GetReaderInfo()
        {
            int iret;
            /*
             * Try to get  serial number and type from device
             */
            StringBuilder devInfor = new StringBuilder();
            devInfor.Append('\0', 128);
            UInt32 nSize;
            nSize = (UInt32)devInfor.Capacity;
            try
            {
                iret = RFIDLIB.rfidlib_reader.RDR_GetReaderInfor(hreader, 0, devInfor, ref nSize);
                if (iret == 0)
                {
                    return true;
                }
               
            }
            catch (Exception ex)
            {
                Log.Error(ex,ex.Message);

            }
            return false;
           
        }

        /// <summary>
        /// Change Mode customize price.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox5_Click(object sender, EventArgs e)
        {
            if (numberOne.Visible)
            {
                HideNumber();
                ShowShortcuts();
            }
            else
            {
                ShowNumber();
                HideShortcuts();
            }
        }
        /// <summary>
        /// Show all number customize price.
        /// </summary>
        private void ShowNumber()
        {
            numberZero.Visible = true;
            numberOne.Visible = true;
            numberTwo.Visible = true;
            numberThree.Visible = true;
            numberFour.Visible = true;
            numberFive.Visible = true;
            numberSix.Visible = true;
            numberSeven.Visible = true;
            numberEight.Visible = true;
            numberNine.Visible = true;
            numberReset.Visible = true;
            numberEnter.Visible = true;
        }

        /// <summary>
        /// Hide all number customize price.
        /// </summary>
        private void HideNumber()
        {
            numberZero.Visible = false;
            numberOne.Visible = false;
            numberTwo.Visible = false;
            numberThree.Visible = false;
            numberFour.Visible = false;
            numberFive.Visible = false;
            numberSix.Visible = false;
            numberSeven.Visible = false;
            numberEight.Visible = false;
            numberNine.Visible = false;
            numberReset.Visible = false;
            numberEnter.Visible = false;
        }

        /// <summary>
        /// Show all shortcuts customize price.
        /// </summary>
        private void ShowShortcuts()
        {
            for (int i = 1; i < NUM_LINE; i++)
            {
                if (panelRight.Controls.ContainsKey("shortcuts" + i))
                {
                    panelRight.Controls["shortcuts" + i].Visible = true;
                }
            }
        }

        /// <summary>
        /// Hide all shortcuts customize price.
        /// </summary>
        private void HideShortcuts()
        {
            for (int i = 1; i < NUM_LINE; i++)
            {
                if (panelRight.Controls.ContainsKey("shortcuts" + i))
                {
                    panelRight.Controls["shortcuts" + i].Visible = false;
                }
            }
        }

        /// <summary>
        /// Save Price From Shortcuts.
        /// </summary>
        /// <param name="value">Value of shortcuts.</param>
        private void SavePriceFromShortcuts(string value)
        {
            labelPrice.Text = value;
            // Save price.
            savePrice();
        }

        /// <summary>
        /// This method creates a Button control at runtime
        /// </summary>
        private void CreateDynamicButton()
        {
            Log.Information("Start create dynamic button.");
            string fileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString() + "\\Shortcuts.txt";
            // Check if file already exists.   
            if (File.Exists(fileName))
            {
                string s = "";
                int index = 0;
                // Open the stream and read it back.    
                using (StreamReader sr = File.OpenText(fileName))
                {
                    while ((s = sr.ReadLine()) != null)
                    {
                        // Create a Button object 
                        Bunifu.Framework.UI.BunifuFlatButton dynamicButton = new Bunifu.Framework.UI.BunifuFlatButton();

                        // Set Button properties
                        dynamicButton.Height = 80;
                        dynamicButton.Width = 325;
                        dynamicButton.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(139)))), ((int)(((byte)(87)))));
                        dynamicButton.BackColor = System.Drawing.Color.SeaGreen;
                        dynamicButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                        dynamicButton.BorderRadius = 0;
                        dynamicButton.ButtonText = s;
                        dynamicButton.Cursor = System.Windows.Forms.Cursors.Hand;
                        dynamicButton.DisabledColor = System.Drawing.Color.Gray;
                        dynamicButton.Iconcolor = System.Drawing.Color.Transparent;
                        //dynamicButton.Iconimage = ((System.Drawing.Image)(resources.GetObject("shortcuts7.Iconimage")));
                        dynamicButton.Iconimage_right = null;
                        dynamicButton.Iconimage_right_Selected = null;
                        dynamicButton.Iconimage_Selected = null;
                        dynamicButton.IconMarginLeft = 0;
                        dynamicButton.IconMarginRight = 0;
                        dynamicButton.IconRightVisible = true;
                        dynamicButton.IconRightZoom = 0D;
                        dynamicButton.IconVisible = true;
                        dynamicButton.IconZoom = 90D;
                        dynamicButton.IsTab = false;
                        dynamicButton.Location = new System.Drawing.Point(12, 10 + (index * 100));
                        dynamicButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
                        dynamicButton.Name = "shortcuts" + (index + 1);
                        dynamicButton.Normalcolor = System.Drawing.Color.SeaGreen;
                        dynamicButton.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(129)))), ((int)(((byte)(77)))));
                        dynamicButton.OnHoverTextColor = System.Drawing.Color.White;
                        dynamicButton.selected = false;
                        //dynamicButton.Size = new System.Drawing.Size(433, 93);
                        dynamicButton.TabIndex = 28;
                        dynamicButton.Text = s;
                        dynamicButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        dynamicButton.Textcolor = System.Drawing.Color.White;
                        dynamicButton.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        dynamicButton.Visible = false;

                        // Add a Button Click Event handler
                        dynamicButton.Click += new EventHandler(DynamicButton_Click);

                        // Add Button to the Form. Placement of the Button

                        // will be based on the Location and Size of button
                        panelRight.Controls.Add(dynamicButton);
                        index++;
                    }
                }
                if (index != 0)
                {
                    NUM_LINE = index + 1;
                }
            }
            else
            {
                File.Create(fileName);
            }
        }

        /// <summary>
        /// Button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DynamicButton_Click(object sender, EventArgs e)
        {
            SavePriceFromShortcuts(((Bunifu.Framework.UI.BunifuFlatButton)sender).ButtonText);
        }

        private void FormWritePriceVer2_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (CotfRunning)
                {
                    var path = ConfigurationManager.AppSettings["COTFPath"].ToString();
                    Process.Start(path);
                }
                if (TagMappingRunning)
                {
                    var path = ConfigurationManager.AppSettings["TagMapping"].ToString();
                    Process.Start(path);
                }
            }
            catch (System.Exception ex)
            {

            }
        }
    }
}
