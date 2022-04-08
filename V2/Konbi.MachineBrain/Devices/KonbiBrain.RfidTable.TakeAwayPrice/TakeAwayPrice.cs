using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KonbiBrain.RfidTable.TakeAwayPrice
{
    public partial class TakeAwayPrice : Form
    {
        #region Declare.
        public UIntPtr hreader;
        public UIntPtr hTag;
        public UIntPtr hTagIcodeslix;
        public UIntPtr hTagTiHFIPlus;
        public Byte enableAFI;
        public Byte AFI;
        public Byte onlyNewTag;
        public bool _shouldStop;
        public Byte openState;
        public Byte inventoryState;
        public Byte readerType;
        public Byte[] AntennaSel;
        public Byte AntennaSelCount;
        public ArrayList readerDriverInfoList;
        public ArrayList airInterfaceProtList;

        string inputText = "";
        Thread InvenThread;
        #endregion

        #region Form action.
        /// <summary>
        /// InitializeComponent.
        /// </summary>
        public TakeAwayPrice()
        {
            InitializeComponent();

            hreader = (UIntPtr)0;
            hTag = (UIntPtr)0;
            hTagIcodeslix = (UIntPtr)0;
            hTagTiHFIPlus = (UIntPtr)0;
            enableAFI = 0;
            AFI = 0;
            onlyNewTag = 0;
            openState = 0;
            inventoryState = 0;
            readerType = 0;
            AntennaSel = new byte[16];
            AntennaSelCount = 0;

            readerDriverInfoList = new ArrayList();
            airInterfaceProtList = new ArrayList();
        }

        /// <summary>
        /// Form load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TakeAwayPrice_Load(object sender, EventArgs e)
        {
            // Load driver for current form.
            LoadDriver();

            // Open reader.
            OpenReader();

            // Start inventory.
            StartInventory();
        }
        #endregion
        
        #region Add source from Example and customise.
        /// <summary>
        /// Load data for current form.
        /// </summary>
        private void LoadDriver()
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

                nSize = (UInt32)strCatalog.Capacity;
                RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_CATALOG, strCatalog, ref nSize);
                driver.m_catalog = strCatalog.ToString();
                if (driver.m_catalog == RFIDLIB.rfidlib_def.RDRDVR_TYPE_READER) // Only reader we need
                {
                    StringBuilder strName = new StringBuilder();
                    strName.Append('\0', 64);
                    nSize = (UInt32)strName.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_NAME, strName, ref nSize);
                    driver.m_name = strName.ToString();

                    StringBuilder strProductType = new StringBuilder();
                    strProductType.Append('\0', 64);
                    nSize = (UInt32)strProductType.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_ID, strProductType, ref nSize);
                    driver.m_productType = strProductType.ToString();

                    StringBuilder strCommSupported = new StringBuilder();
                    strCommSupported.Append('\0', 64);
                    nSize = (UInt32)strCommSupported.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_COMMTYPESUPPORTED, strCommSupported, ref nSize);
                    driver.m_commTypeSupported = (UInt32)int.Parse(strCommSupported.ToString());

                    readerDriverInfoList.Add(driver);
                }
            }
        }

        /// <summary>
        /// Open reader.
        /// </summary>
        private void OpenReader()
        {
            // Declare type.
            Byte usbOpenType = (byte)0;     // Communication type.
            Byte readerType = 0;            // Reader type.
            int iret = 0;
            UInt32 antennaCount = 0;

            /*
             * Try to open communcation layer for specified reader 
             */
            string readerDriverName = ((CReaderDriverInf)(readerDriverInfoList[readerType])).m_name;
            string connstr = "";
            // Build USBHID communication connection string
            connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_USB + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDADDRMODE + "=" + usbOpenType.ToString() + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDSERNUM + "=" + "";

            // Call required to open reader driver
            iret = RFIDLIB.rfidlib_reader.RDR_Open(connstr, ref hreader);
            if (iret != 0)
            {
                /*
                 *  Open fail:
                 *  if you Encounter this error ,make sure you has called the API "RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers("\\Drivers")" 
                 *  when application load
                 */
                Log.Error($"Open reader failed. Error when call DLIB.rfidlib_reader.RDR_Open {connstr}");
                FormMessageBox.showMessageBox(true, "Open reader failed.");
            }
            else
            {
                Log.Error($"Open reader successful.");
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
                    RFIDLIB.rfidlib_reader.RDR_GetAirInterfaceProtName(hreader, AIType, namebuf, (UInt32)namebuf.Capacity);

                    CSupportedAirProtocol aip = new CSupportedAirProtocol();
                    aip.m_ID = AIType;
                    aip.m_name = namebuf.ToString();
                    aip.m_en = true;
                    airInterfaceProtList.Add(aip);

                    index++;
                }

                openState = 1;
            }
        }

        /// <summary>
        /// Start inventory.
        /// </summary>
        private void StartInventory()
        {
            _shouldStop = false;

            // AFI
            enableAFI = 0;

            //Only new tag
            onlyNewTag = 1;

            // antenna selection
            int iCount = 0;
            AntennaSelCount = (Byte)iCount;

            /*
             * Start the thread to inventory tags 
             */
            InvenThread = new Thread(DoInventory);
            InvenThread.Start();

            inventoryState = 1;
            return;
        }

        /// <summary>
        /// Delegate tag report handle.
        /// </summary>
        /// <param name="AIPType"></param>
        /// <param name="tagType"></param>
        /// <param name="antID"></param>
        /// <param name="uid"></param>
        /// <param name="uidlen"></param>
        public delegate void delegate_tag_report_handle(UInt32 AIPType, UInt32 tagType, UInt32 antID, Byte[] uid, int uidlen);

        /// <summary>
        /// Tag report handler.
        /// </summary>
        /// <param name="AIPType"></param>
        /// <param name="tagType"></param>
        /// <param name="antID"></param>
        /// <param name="uid"></param>
        /// <param name="uidlen"></param>
        public void dele_tag_report_handler(UInt32 AIPType, UInt32 tagType, UInt32 antID, Byte[] uid, int uidlen)
        {

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
                if (listView1.Items[i].SubItems[2].Text == strUid && listView1.Items[i].SubItems[4].Text == antID.ToString())
                {
                    found = true;
                    break;
                }

            }
            if (!found)
            {
                listView1.Items.Clear();
                ListViewItem lvi = new ListViewItem();
                lvi.Text = strAIPName;
                lvi.SubItems.Add(strTagTypeName);
                lvi.SubItems.Add(strUid);
                lvi.SubItems.Add("1");
                lvi.SubItems.Add(antID.ToString());
                listView1.Items.Add(lvi);

                tagInfo ti = new tagInfo(strUid, tagType);

                // Connect tag.
                ConnectTag(strUid);

                // Reading tag.
                bool checkRead = false;
                var model = ReadDataTag(4, strUid);
                // TrungPQ add When can't read.
                if (model != "")
                {
                    checkRead = true;
                    lvi.SubItems.Add(model.Substring(4, 4));
                }

                var customData = "";
                if (checkRead)
                {
                    customData = ReadDataTag(10, strUid);
                }

                float price = 0;
                float.TryParse(customData, out price);
                labelPrice.Text = (price / 100).ToString("C");

                // Disconnect tag.
                DisconnectTag(strUid);
            }

            /* you can try to read or write tag below */
            //int iret ;
            //UIntPtr hTag = UIntPtr.Zero ;
            //if (AIPType == RFIDLIB.rfidlib_def.RFID_APL_ISO15693_ID){
            //     /* iso15693 example to read multiple blocks here */
            //    iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_Connect(hreader, tagType, 1, uid, ref hTag);
            //    if(iret == 0){
            //        UInt32 numOfBlksRead ;
            //        UInt32 bytesRead;
            //        Byte[] bufBlocks = new Byte[64];
            //        numOfBlksRead = bytesRead =0 ;
            //        iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ReadMultiBlocks(hreader, hTag, 1, 0, 4, ref numOfBlksRead, bufBlocks,(UInt32) bufBlocks.GetLength(0), ref bytesRead);
            //        if(iret == 0){
            //            /* read multiple blocks ok */
            //        }

            //        RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
            //    }
            //}
            //else if (AIPType == RFIDLIB.rfidlib_def.RFID_APL_ISO14443A_ID && tagType == RFIDLIB.rfidlib_def.RFID_ISO14443A_PICC_NXP_MIFARE_S50_ID)
            //{
            //    /* iso14443a mifare s50 read block example here */
            //    iret = RFIDLIB.rfidlib_aip_iso14443A.MFCL_Connect(hreader, 0/* 0:s50,1:s70*/, uid, ref hTag);
            //    if(iret == 0){
            //        Byte[] block = new Byte[16] ;
            //        iret = RFIDLIB.rfidlib_aip_iso14443A.MFCL_ReadBlock(hreader,hTag,0,block,(UInt32)block.GetLength(0)) ;
            //        if(iret == 0) {
            //            /* read block ok*/
            //        }
            //        RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
            //    }
            //}

        }

        /// <summary>
        /// Tag inventory.
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
                                bool enable15693,
                                bool enable14443A,
                                bool enable18000p3m3,
                                  Byte enableAFI,
                                 Byte afiVal,
                                delegate_tag_report_handle tagReportHandler,
                                 ref UInt32 nTagCount)
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
                    RFIDLIB.rfidlib_aip_iso18000p3m3.ISO18000p3m3_CreateInvenParam(InvenParamSpecList, 0, 0, 0, RFIDLIB.rfidlib_def.ISO18000p6C_Dynamic_Q);
                }
            }
            nTagCount = 0;
            LABEL_TAG_INVENTORY:
            iret = RFIDLIB.rfidlib_reader.RDR_TagInventory(hreader, AIType, AntennaSelCount, AntennaSel, InvenParamSpecList);
            if (iret == 0 || iret == -21)
            {
                nTagCount += RFIDLIB.rfidlib_reader.RDR_GetTagDataReportCount(hreader);
                UIntPtr TagDataReport;
                TagDataReport = (UIntPtr)0;
                TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_FIRST); //first
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
                        iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ParseTagDataReport(TagDataReport, ref aip_id, ref tag_id, ref ant_id, ref dsfid, uid);
                        if (iret == 0)
                        {
                            uidlen = 8;
                            object[] pList = { aip_id, tag_id, ant_id, uid, (int)uidlen };
                            Invoke(tagReportHandler, pList);
                            //tagReportHandler(hreader, aip_id, tag_id, ant_id, uid ,8);
                        }
                    }

                    /* Parse Iso14443A tag report */
                    if (enable14443A == true)
                    {
                        iret = RFIDLIB.rfidlib_aip_iso14443A.ISO14443A_ParseTagDataReport(TagDataReport, ref aip_id, ref tag_id, ref ant_id, uid, ref uidlen);
                        if (iret == 0)
                        {
                            object[] pList = { aip_id, tag_id, ant_id, uid, (int)uidlen };
                            Invoke(tagReportHandler, pList);
                            //tagReportHandler(hreader, aip_id, tag_id, ant_id, uid, uidlen);
                        }
                    }

                    /* Parse Iso18000-3 mode 3  tag report */
                    if (enable18000p3m3)
                    {
                        UInt32 metaFlags = 0;
                        Byte[] tagData = new Byte[32];
                        UInt32 tagDataLen = (UInt32)tagData.Length;
                        iret = RFIDLIB.rfidlib_aip_iso18000p3m3.ISO18000p3m3_ParseTagDataReport(TagDataReport, ref aip_id, ref tag_id, ref ant_id, ref metaFlags, tagData, ref tagDataLen);
                        if (iret == 0)
                        {
                            object[] pList = { aip_id, tag_id, ant_id, tagData, (int)tagDataLen };
                            Invoke(tagReportHandler, pList);
                        }
                    }

                    /* Get Next report from buffer */
                    TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
                }
                if (iret == -21) // stop trigger occur,need to inventory left tags
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
        /// Do inventory.
        /// </summary>
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
                aip = (CSupportedAirProtocol)airInterfaceProtList[i];
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
                AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;  //only new tag inventory 
            }
            while (!_shouldStop)
            {

                UInt32 nTagCount = 0;
                iret = tag_inventory(AIType, AntennaSelCount, AntennaSel, enISO15693, enISO14443a, enISO18000p3m3, enableAFI, AFI, cbTagReportHandle, ref nTagCount);
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
                    AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;  //only new tag inventory 
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

        /// <summary>
        /// Delegate inventory finish callback.
        /// </summary>
        private delegate void delegateInventoryFinishCallback();

        /// <summary>
        /// Inventory finish callback.
        /// </summary>
        public void InventoryFinishCallback()
        {
            inventoryState = 0;
        }

        /// <summary>
        /// Connect to tag.
        /// </summary>
        /// <param name="suid"></param>
        private void ConnectTag(string suid)
        {
            Log.Information($"Start connect tag {suid}.");

            int iret;
            int idx = 1;
            if (hTag != UIntPtr.Zero)
            {
                FormMessageBox.showMessageBox(true, "Please disconnect tag first!");
                return;
            }

            // set uid
            byte[] uid = StringToByteArrayFastest(suid);

            //set tag type default is NXP icode slix.
            UInt32 tagType = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLIX_ID;

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
                        FormMessageBox.showMessageBox(true, "reset tag fail!");
                        RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
                        hTag = UIntPtr.Zero;
                        return;
                    }
                }
                /* connect ok */
                Log.Information($"Connect tag {suid} successful.");
            }
            else
            {
                Log.Error($"Connect tag {suid}- failed");
                FormMessageBox.showMessageBox(true, $"Can't connect to tag {suid}.");
            }
        }

        /// <summary>
        /// Disconnect tag.
        /// </summary>
        /// <param name="suid"></param>
        private void DisconnectTag(string suid)
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
                Log.Error($"DisConnect tag {suid}- failed");
                FormMessageBox.showMessageBox(true, $"Can't disconnect to tag {suid}.");
            }
        }

        /// <summary>
        /// Read data tag.
        /// </summary>
        /// <param name="blkAddress"></param>
        /// <returns></returns>
        private string ReadDataTag(uint blkAddress, string suid)
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
                Log.Error($"Read data of tag {suid}- successful");
                return txt;
            }
            else
            {
                Log.Error($"Read data of tag {blkAddress}- successful");
                FormMessageBox.showMessageBox(true, "Read data failed!");
                return "";
            }
        }

        /// <summary>
        /// Write data tag.
        /// </summary>
        /// <param name="blkAddress"></param>
        /// <param name="suid"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool WriteDataTag(uint blkAddress, string data)
        {
            int iret;
            byte[] newBlksData = StringToByteArrayFastest(data);

            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_WriteMultipleBlocks(hreader, hTag, blkAddress, 1, newBlksData,
                (uint)newBlksData.Length);
            if (iret == 0)
            {
                Log.Information($"Write data {blkAddress} {data}- failed");
                return true;
            }
            else
            {
                Log.Error("failed!err = " + iret);
                Log.Error($"Write data {blkAddress} {data}- failed");
                FormMessageBox.showMessageBox(true, "Write data false, please try again!");
                return false;
            }
        }
        #endregion

        #region Ultility
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
        /// GetHexVal.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            // return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        #endregion

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
        /// Click button enter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberEnter_Click(object sender, EventArgs e)
        {
            savePrice();
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

            if (listView1.Items.Count == 0)
            {
                FormMessageBox.showMessageBox(true, "Please put new takeaway plate!");
                inputText = "";
                return;
            }

            //Save.
            try
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    var uuid = listView1.Items[i].SubItems[0].Text;
                    var price = Convert.ToInt32(Convert.ToDouble(labelPrice.Text.Substring(1)) * 100);

                    var data = price.ToString("D8");
                    Log.Information($"Set price {price}");
                    bool _resultWrite = WriteDataTag(10, data);
                    if (!_resultWrite)
                        return;
                    // Add history.
                    addHistory(listView1);
                }
                // Reset label.
                resetClick();

                FormMessageBox.showMessageBox(false, "Set price succesfully.\n Please remove your takeaway plate!");
                Log.Information("Set price succesfully.");
            }
            catch (Exception ex)
            {
                Log.Error("failed!err = " + ex);
            }
            
        }
        #endregion

        #region History
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
                listView1.Items.Clear();
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
    }

    #region Declare Object.
    /// <summary>
    /// Object reader driver info.
    /// </summary>
    public class CReaderDriverInf
    {
        public string m_catalog;
        public string m_name;
        public string m_productType;
        public UInt32 m_commTypeSupported;
        public ArrayList airInterfaceProtList;
    }

    /// <summary>
    /// Tag info.
    /// </summary>
    public class tagInfo
    {
        public string m_uid;
        public UInt32 m_tagType;
        public tagInfo(string uid, UInt32 tagType)
        {
            m_uid = uid;
            m_tagType = tagType;
        }
        public override string ToString()
        {
            return m_uid.ToString();
        }

    }

    /// <summary>
    /// Air protocol.
    /// </summary>
    public class CSupportedAirProtocol
    {
        public UInt32 m_ID;
        public string m_name;
        public Boolean m_en;
    }
    #endregion
}
