using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsApplication2;
using Serilog;

namespace KonbiBrain.RfidTable.TakeAwayPrice
{
    public partial class FormWritePrice : Form
    {
        public UIntPtr hreader;
        public ArrayList readerDriverInfoList = new ArrayList();

        public ArrayList airInterfaceProtList = new ArrayList();
        public bool _shouldStop;
        public Byte[] AntennaSel = new byte[16];

        public Byte AntennaSelCount = 0;

        public Byte onlyNewTag = 0;
        public Byte enableAFI = 0;
        public Byte AFI = 0;
        Thread InvenThread;

        public Byte inventoryState = 0;
        private string format = "dd/MM/yyy HH:mm:ss";
        private readonly System.Timers.Timer scanningTimer;
        public UIntPtr hTag;
        public UIntPtr hTagIcodeslix;
        public UIntPtr hTagTiHFIPlus;
        public Byte openState = 0;
        private List<ListDataItem> dataItems = new List<ListDataItem>();

        public FormWritePrice()
        {
            InitializeComponent();
            scanningTimer = new System.Timers.Timer();
            scanningTimer.Interval = 500;
            scanningTimer.Enabled = true;
            scanningTimer.Elapsed += ScanningTimer_Elapsed;
            scanningTimer.Start();

            hTag = (UIntPtr) 0;
            hTagIcodeslix = (UIntPtr) 0;
            hTagTiHFIPlus = (UIntPtr) 0;


        }

        private void ScanningTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {


            this.Invoke(new Action(() =>
            {

                for (int i = 0; i < dataItems.Count; i++)
                {
                    var date = dataItems[i].UpdatedDate;
                    var different = DateTime.Now - date;
                    Log.Information($"Plate removed {listView1.Items[i].SubItems[0].Text}");
                    if (different.Milliseconds > 500)
                    {
                        var item = dataItems.First(x => x.UUID == listView1.Items[i].SubItems[0].Text);
                        dataItems.Remove(item);
                        listView1.Items[i].Remove();
                    }

                }

                UpdateTotalPlates();
            }));



        }

        private void savePriceBtn_Click(object sender, EventArgs e)
        {

            try
            {
          
                InvenThread.Abort();
                _shouldStop = true;
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    var uuid = listView1.Items[i].SubItems[0].Text;
                    ConnectTag(uuid);
                    var price = Convert.ToInt32(this.pricceBox.Value * 100);
                    var data = price.ToString("D8");
                    Log.Information($"Set price {price}");
                    WriteData(10, data);
                    DisConnectTag();
                }

                MessageBox.Show("Set price succesfully!");
            }
            catch (Exception exception)
            {
                Log.Error(exception,"");
                MessageBox.Show("There was error, please try again!");
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

        private void InitData()
        {


            /* 
             *  Call required, when application load ,this API just only need to load once
             *  Load all reader driver dll from drivers directory, like "rfidlib_ANRD201.dll"  
             */
            RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers("\\Drivers");

            /*
             * Not call required,it can be Omitted in your own appliation
             * enum and show loaded reader driver 
             */
            UInt32 nCount;
            nCount = RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverCount();
            uint i;
            for (i = 0; i < nCount; i++)
            {
                UInt32 nSize;
                CReaderDriverInf driver = new CReaderDriverInf();
                StringBuilder strCatalog = new StringBuilder();
                strCatalog.Append('\0', 64);

                nSize = (UInt32) strCatalog.Capacity;
                RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_CATALOG,
                    strCatalog, ref nSize);
                driver.m_catalog = strCatalog.ToString();
                if (driver.m_catalog == RFIDLIB.rfidlib_def.RDRDVR_TYPE_READER) // Only reader we need
                {
                    StringBuilder strName = new StringBuilder();
                    strName.Append('\0', 64);
                    nSize = (UInt32) strName.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_NAME,
                        strName, ref nSize);
                    driver.m_name = strName.ToString();

                    StringBuilder strProductType = new StringBuilder();
                    strProductType.Append('\0', 64);
                    nSize = (UInt32) strProductType.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_ID,
                        strProductType, ref nSize);
                    driver.m_productType = strProductType.ToString();

                    StringBuilder strCommSupported = new StringBuilder();
                    strCommSupported.Append('\0', 64);
                    nSize = (UInt32) strCommSupported.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i,
                        RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_COMMTYPESUPPORTED, strCommSupported, ref nSize);
                    driver.m_commTypeSupported = (UInt32) int.Parse(strCommSupported.ToString());

                    readerDriverInfoList.Add(driver);
                }

            }
        }

        private void FormWritePrice_Load(object sender, EventArgs e)
        {
            InitData();

            Byte usbOpenType = (byte) 0;


            Byte readerType = 0;

            int iret = 0;
            UInt32 antennaCount = 0;
            ////checkedListBox1.Items.Clear();
            ////comboBox7.Items.Clear();
            ////button19.Enabled = false;
            ////button2.Enabled = false;

            /*
             * Try to open communcation layer for specified reader 
             */
            int commTypeIdx = 1;
            string readerDriverName = ((CReaderDriverInf) (readerDriverInfoList[readerType])).m_name;
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
                MessageBox.Show("Open reader failed!");

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
                        (UInt32) namebuf.Capacity);

                    CSupportedAirProtocol aip = new CSupportedAirProtocol();
                    aip.m_ID = AIType;
                    aip.m_name = namebuf.ToString();
                    aip.m_en = true;
                    airInterfaceProtList.Add(aip);

                    //checkedListBox2.Items.Add(aip.m_name, true);

                    index++;
                }


            }

            InvenThread = new Thread(DoInventory);
            InvenThread.Start();

            openState = 1;
        }


        public void DoInventory()
        {
            Boolean enISO15693, enISO14443a, enISO18000p3m3;
            delegate_tag_report_handle cbTagReportHandle;

            cbTagReportHandle = new delegate_tag_report_handle(dele_tag_report_handler);
            enISO15693 = enISO14443a = enISO18000p3m3 = false;
            /* check air protocol */
            for (int i = 0; i < airInterfaceProtList.Count; i++)
            {
                CSupportedAirProtocol aip;
                aip = (CSupportedAirProtocol) airInterfaceProtList[i];
                if (aip.m_en)
                {
                    if (aip.m_ID == RFIDLIB.rfidlib_def.RFID_APL_ISO15693_ID)
                    {
                        //create ISO15693 inventory parameter  
                        enISO15693 = true;
                    }
                    else if (aip.m_ID == RFIDLIB.rfidlib_def.RFID_APL_ISO14443A_ID)
                    {
                        //create ISO14443A inventory parameter  
                        enISO14443a = true;
                    }
                    else if (aip.m_ID == RFIDLIB.rfidlib_def.RFID_APL_ISO18000P3M3)
                    {
                        enISO18000p3m3 = true;
                    }
                }
            }

            int iret;
            Byte AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
            if (onlyNewTag == 1)
            {
                AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE; //only new tag inventory 
            }

            while (!_shouldStop)
            {

                UInt32 nTagCount = 0;
                iret = tag_inventory(AIType, AntennaSelCount, AntennaSel, enISO15693, enISO14443a, enISO18000p3m3,
                    enableAFI, AFI, cbTagReportHandle, ref nTagCount);
                if (iret == 0)
                {
                    // inventory ok

                }
                else
                {
                    // inventory error 
                }

                AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
                if (onlyNewTag == 1)
                {
                    AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE; //only new tag inventory 
                }

            }

            object[] pFinishList = { };
            Invoke(new delegateInventoryFinishCallback(InventoryFinishCallback), pFinishList);

            /*
             *  If API RDR_SetCommuImmeTimeout is called when stop, API RDR_ResetCommuImmeTimeout 
             *  must be called too, Otherwise, an error -5 may occurs .
             */
            RFIDLIB.rfidlib_reader.RDR_ResetCommuImmeTimeout(hreader);

        }

        public delegate void delegate_tag_report_handle(UInt32 AIPType, UInt32 tagType, UInt32 antID, Byte[] uid,
            int uidlen);

        public int tag_inventory(
            Byte AIType,
            Byte AntennaSelCount,
            Byte[] AntennaSel,
            bool enable15693,
            bool enable14443A,
            bool enable18000p3m3,
            Byte enableAFI,
            Byte afiVal,
            delegate_tag_report_handle tagReportHandler,
            ref UInt32 nTagCount)
        {

            try
            {
                int iret;
                UIntPtr InvenParamSpecList = UIntPtr.Zero;
                InvenParamSpecList = RFIDLIB.rfidlib_reader.RDR_CreateInvenParamSpecList();
                if (InvenParamSpecList.ToUInt64() != 0)
                {
                    if (enable15693 == true)
                    {
                        RFIDLIB.rfidlib_aip_iso15693.ISO15693_CreateInvenParam(InvenParamSpecList, 0, 0, 0, 0);
                    }

                    if (enable14443A == true)
                    {
                        RFIDLIB.rfidlib_aip_iso14443A.ISO14443A_CreateInvenParam(InvenParamSpecList, 0);
                    }

                    if (enable18000p3m3)
                    {
                        RFIDLIB.rfidlib_aip_iso18000p3m3.ISO18000p3m3_CreateInvenParam(InvenParamSpecList, 0, 0, 0,
                            RFIDLIB.rfidlib_def.ISO18000p6C_Dynamic_Q);
                    }
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

                        /* Parse iso15693 tag report */
                        if (enable15693 == true)
                        {
                            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ParseTagDataReport(TagDataReport, ref aip_id,
                                ref tag_id, ref ant_id, ref dsfid, uid);
                            if (iret == 0)
                            {
                                uidlen = 8;
                                object[] pList = { aip_id, tag_id, ant_id, uid, (int)uidlen };
                                if (!this._shouldStop) Invoke(tagReportHandler, pList);

                                //tagReportHandler(hreader, aip_id, tag_id, ant_id, uid ,8);
                            }
                        }

                        /* Parse Iso14443A tag report */
                        if (enable14443A == true)
                        {
                            iret = RFIDLIB.rfidlib_aip_iso14443A.ISO14443A_ParseTagDataReport(TagDataReport, ref aip_id,
                                ref tag_id, ref ant_id, uid, ref uidlen);
                            if (iret == 0)
                            {
                                object[] pList = { aip_id, tag_id, ant_id, uid, (int)uidlen };
                                if (!this._shouldStop) Invoke(tagReportHandler, pList);
                                //tagReportHandler(hreader, aip_id, tag_id, ant_id, uid, uidlen);
                            }
                        }

                        /* Parse Iso18000-3 mode 3  tag report */
                        if (enable18000p3m3)
                        {
                            UInt32 metaFlags = 0;
                            Byte[] tagData = new Byte[32];
                            UInt32 tagDataLen = (UInt32)tagData.Length;
                            iret = RFIDLIB.rfidlib_aip_iso18000p3m3.ISO18000p3m3_ParseTagDataReport(TagDataReport,
                                ref aip_id, ref tag_id, ref ant_id, ref metaFlags, tagData, ref tagDataLen);
                            if (iret == 0)
                            {
                                object[] pList = { aip_id, tag_id, ant_id, tagData, (int)tagDataLen };
                                if (!this._shouldStop) Invoke(tagReportHandler, pList);
                            }
                        }

                        /* Get Next report from buffer */
                        TagDataReport =
                            RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
                    }

                    if (iret == -21) // stop trigger occur,need to inventory left tags
                    {
                        AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE; //use only-new-tag inventory 
                        goto LABEL_TAG_INVENTORY;
                    }

                    iret = 0;
                }

                if (InvenParamSpecList.ToUInt64() != 0) RFIDLIB.rfidlib_reader.DNODE_Destroy(InvenParamSpecList);
                return iret;
            }
            catch (Exception e)
            {
                Log.Error(e,"");
                return -1;
            }
           
        }

        public void dele_tag_report_handler(UInt32 AIPType, UInt32 tagType, UInt32 antID, Byte[] uid, int uidlen)
        {

            String strUid;

            int iret;
            String strAIPName, strTagTypeName;
            StringBuilder sbAIPName = new StringBuilder();
            sbAIPName.Append('\0', 128);
            UInt32 nSize = (UInt32) sbAIPName.Capacity;
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
            nSize = (UInt32) sbTagName.Capacity;
            iret = RFIDLIB.rfidlib_reader.RDR_GetTagTypeName(hreader, AIPType, tagType, sbTagName, ref nSize);
            if (iret != 0)
            {
                strTagTypeName = "Unknown";
            }
            else
            {
                strTagTypeName = sbTagName.ToString();
            }

            strUid = BitConverter.ToString(uid, 0, (int) uidlen).Replace("-", string.Empty);

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
                ListViewItem lvi = new ListViewItem();
                lvi.Text = strUid;
                //lvi.SubItems.Add(strTagTypeName);
                //lvi.SubItems.Add(strUid);
                //lvi.SubItems.Add("1");
                //lvi.SubItems.Add(antID.ToString());
                //lvi.SubItems.Add(DateTime.Now.ToString("HH:mm:ss.fff"));
                //lvi.SubItems.Add(DateTime.Now.ToString(format));

                ConnectTag(strUid);
                var model = ReadData(4);
                lvi.SubItems.Add(model.Substring(0, 4));
                var customData = ReadData(10);
                float price = 0;
                float.TryParse(customData, out price);
                lvi.SubItems.Add((price / 100).ToString("C"));

                DisConnectTag();


                lvi.SubItems.Add(DateTime.Now.ToString(format));
                listView1.Items.Add(lvi);
                dataItems.Add(new ListDataItem{UUID = strUid,UpdatedDate = DateTime.Now});
                    Log.Information($"Added plate uuid {strUid} model {model} price {customData}");
                    
                UpdateTotalPlates();
            }
            else
            {
                dataItems[i].UpdatedDate=DateTime.Now;
            }
        }

        private delegate void delegateInventoryFinishCallback();

        public void InventoryFinishCallback()
        {
            //button5.Enabled = true;
            //button6.Enabled = false;
            //button3.Enabled = true;
            inventoryState = 0;
        }


        private void DisConnectTag()
        {
            int iret;

            // do disconnection
            iret = RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
            if (iret == 0)
            {
                hTag = (UIntPtr) 0;
            }
            else
            {
                Log.Error("DisConnectTag - failed");
                MessageBox.Show("Disconnect tag failed!");
            }
        }

        private void ConnectTag(string uuid)
        {
            int iret;
            string suid;
            int idx;
            if (hTag != UIntPtr.Zero)
            {
                DisConnectTag();
                MessageBox.Show("Please disconnect tag first");
                return;
            }

            idx = 1;
            if (idx == -1)
            {
                MessageBox.Show("Please select address mode");
                return;
            }


            // set uid

            byte[] uid = StringToByteArrayFastest(uuid);

            //set tag type default is NXP icode sli 
            UInt32 tagType = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
            tagType = 7; //slix


            // set address mode 
            Byte addrMode = (Byte) idx;

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
                        return;
                    }
                }
                /* connect ok */


            }
            else
            {
                 Log.Error($"ConnectTag {uuid}- failed");
                MessageBox.Show($"Connect tag {uuid} failed");
            }
        }

        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            int len = hex.Length >> 1;
            byte[] arr = new byte[len];

            for (int i = 0; i < len; ++i)
            {
                arr[i] = (byte) ((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }


        public static int GetHexVal(char hex)
        {
            int val = (int) hex;
            //For uppercase A-F letters:
            // return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }


        private string ReadData(uint blkAddress)
        {
            int iret;
            int idx;
            //UInt32 blockAddr;
            //UInt32 blockToRead;
            UInt32 blocksRead = 0;

            //blockToRead = (UInt32)(idx + 1);
            UInt32 nSize;
            Byte[] BlockBuffer = new Byte[40];

            nSize = (UInt32) BlockBuffer.GetLength(0);
            UInt32 bytesRead = 0;
            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ReadMultiBlocks(hreader, hTag, 0, blkAddress, 1,
                ref blocksRead, BlockBuffer, nSize, ref bytesRead);
            if (iret == 0)
            {
                //blocksRead: blocks read 
                var txt = BitConverter.ToString(BlockBuffer, 0, (int) bytesRead).Replace("-", string.Empty);

                return txt;
            }
            else
            {
                Log.Error($"ReadData {blkAddress}- failed");
                MessageBox.Show("Read data failed");
                return "";
            }
        }

        private bool WriteData(uint blkAddress, string data)
        {
            int iret;

            byte[] newBlksData = StringToByteArrayFastest(data);

            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_WriteMultipleBlocks(hreader, hTag, blkAddress, 1, newBlksData,
                (uint) newBlksData.Length);
            if (iret == 0)
            {
                //MessageBox.Show("ok !");
                return true;
            }
            else
            {
                   Log.Error($"WriteData {blkAddress} {data}- failed");
                MessageBox.Show("failed!err = " + iret);
                return false;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void UpdateTotalPlates()
        {
            foreach (ListViewItem item in listView1.Items)
            {
                
                Log.Information($"Plate: uuid {item.SubItems[0]} model {item.SubItems[1]} price {item.SubItems[2]}");
            }
            lblTotalPlates.Text = $"Total plate(s): {listView1.Items.Count}";
            
        }

        private void FormWritePrice_FormClosing(object sender, FormClosingEventArgs e)
        {
            _shouldStop = true;
            InvenThread.Abort();
            Thread.Sleep(100);
            if (hTag != UIntPtr.Zero)
                DisConnectTag();



            /*
             * Exit the inventory quickly
             */
            RFIDLIB.rfidlib_reader.RDR_SetCommuImmeTimeout(hreader);


            int iret = 0;
            if (hTag != (UIntPtr) 0)
            {
                MessageBox.Show("disconnect from tag first!");
                return;
            }

            /*
             *  Close reader driver ,this API is required
             */
            iret = RFIDLIB.rfidlib_reader.RDR_Close(hreader);
            if (iret == 0)
            {
                hreader = (UIntPtr) 0;

            }
            else
            {
                MessageBox.Show("Form closing failed");
            }


            //if (inventoryState > 0)
            //{
            //    MessageBox.Show("stop inventory first!");
            //    e.Cancel = true;
            //    return;
            //}
            //if (openState > 0)
            //{
            //    MessageBox.Show("close reader driver first!");
            //    e.Cancel = true;
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _shouldStop = true;
            InvenThread.Abort();

            Application.Exit();
        }
    }
}
