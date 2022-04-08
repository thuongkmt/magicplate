using KonbiBrain.WindowServices.CotfPad.Hardware;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Threading;

namespace KonbiBrain.WindowServices.CotfPad
{
    public class HardwareUtil
    {
        private ArrayList readerDriverInfoList = new ArrayList();
        private UIntPtr hreader;
        private ArrayList airInterfaceProtList = new ArrayList();
        public bool IsConnected { get; internal set; }
        private Thread invenThread;
        private Thread reconnectThread;
        private Byte onlyNewTag = 0;
        private bool _shouldStop;
        private bool _shouldReconnect;
        private Byte antennaSelCount = 0;
        private Byte[] antennaSel = new byte[16];
        private Byte enableAFI = 0;
        private Byte AFI = 0;
        private UIntPtr hTag;

        private Dictionary<string, TagDto> existedTags = new Dictionary<string, TagDto>();
        //private List<TagDto> currentTags = new List<TagDto>();
        public Action<TagDto> TagDetected { get; set; }
        public Action<List<TagDto>> TagsDetected { get; set; }

        public void ClearTags()
        {
            existedTags = new Dictionary<string, TagDto>();
        }
        public void Disconnect()
        {
            ClearTags();
            _shouldReconnect = false;
            _shouldStop = true;

            invenThread?.Abort();
            reconnectThread?.Abort();
            Thread.Sleep(100);
            if (hTag != UIntPtr.Zero)
                DisConnectTag();

            /*
             * Exit the inventory quickly
             */
            RFIDLIB.rfidlib_reader.RDR_SetCommuImmeTimeout(hreader);
            int iret = 0;
            if (hTag != (UIntPtr)0)
            {
                return;
            }

            iret = RFIDLIB.rfidlib_reader.RDR_Close(hreader);
            if (iret == 0)
            {
                hreader = (UIntPtr)0;
            }
        }

        public void Connect(bool shouldReconnect=true)
        {
            ClearTags();
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
                RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_CATALOG,
                    strCatalog, ref nSize);
                driver.m_catalog = strCatalog.ToString();
                if (driver.m_catalog == RFIDLIB.rfidlib_def.RDRDVR_TYPE_READER) // Only reader we need
                {
                    StringBuilder strName = new StringBuilder();
                    strName.Append('\0', 64);
                    nSize = (UInt32)strName.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_NAME,
                        strName, ref nSize);
                    driver.m_name = strName.ToString();

                    StringBuilder strProductType = new StringBuilder();
                    strProductType.Append('\0', 64);
                    nSize = (UInt32)strProductType.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_ID,
                        strProductType, ref nSize);
                    driver.m_productType = strProductType.ToString();

                    StringBuilder strCommSupported = new StringBuilder();
                    strCommSupported.Append('\0', 64);
                    nSize = (UInt32)strCommSupported.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i,
                        RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_COMMTYPESUPPORTED, strCommSupported, ref nSize);
                    driver.m_commTypeSupported = (UInt32)int.Parse(strCommSupported.ToString());

                    readerDriverInfoList.Add(driver);
                }

            }
            Byte usbOpenType = (byte)0; //usb
            Byte readerType = 0;  //M201

            int iret = 0;
            UInt32 antennaCount = 0;

            /*
             * Try to open communcation layer for specified reader 
             */
            int commTypeIdx = 1;
            string readerDriverName = ((CReaderDriverInf)(readerDriverInfoList[readerType])).m_name;
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
                Log.Error($"Error when call DLIB.rfidlib_reader.RDR_Open {connstr}");
            }
            else
            {

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
                IsConnected = true;
                _shouldStop = false;
                invenThread = new Thread(DoInventory);
                invenThread.Start();
                
            }

            if (shouldReconnect)
            {
                if (reconnectThread == null || !reconnectThread.IsAlive)
                {
                    reconnectThread = new Thread(Reconnect);
                    reconnectThread.Start();
                }
            }
            
        }

        #region private functions
        private void Reconnect()
        {
            while(true)
           
            {                
                Thread.Sleep(500);
                if(!IsConnected)
                    Connect(false);
            }
        }
        private void DoInventory()
        {
            Boolean enISO15693, enISO14443a, enISO18000p3m3;
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
                AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE; //only new tag inventory 
            }

            while (!_shouldStop)
            {

                UInt32 nTagCount = 0;
                iret = TagInventory(AIType, antennaSelCount, antennaSel, enISO15693, enISO14443a, enISO18000p3m3, enableAFI, AFI, ref nTagCount);

                if (iret != 0)
                {
                    IsConnected = false;
                    _shouldStop = true;
                }
                    

                AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
                if (onlyNewTag == 1)
                {
                    AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE; //only new tag inventory 
                }


                Thread.Sleep(250);//sleep 100 milli seconds, increased to 250ms

            }
            object[] pFinishList = { };


            /*
             *  If API RDR_SetCommuImmeTimeout is called when stop, API RDR_ResetCommuImmeTimeout 
             *  must be called too, Otherwise, an error -5 may occurs .
             */
            RFIDLIB.rfidlib_reader.RDR_ResetCommuImmeTimeout(hreader);

        }

        private void ProcessTags(List<TagDto> currentTags)
        {
            bool isChanged = false;
            var removed = existedTags.Where(x => !currentTags.Any(t => t.TagId == x.Key)).ToList();
            if (removed.Any())
            {
                for (int i = 0; i < removed.Count(); i++)
                {
                    Console.WriteLine($"Removed tag {removed[i].Key}");
                    Log.Information($"Removed tag {removed[i].Key}");
                    existedTags.Remove(removed[i].Key);
                    isChanged = true;
                }
            }

            var added = currentTags.Where(x => !existedTags.Any(t => t.Key == x.TagId)).ToList();
            if (added.Any())
            {
                for (int i = 0; i < added.Count(); i++)
                {
                    existedTags.Add(added[i].TagId, added[i]);
                    TagDetected?.Invoke(added[i]);
                    isChanged = true;
                }
            }

            if (isChanged)
                TagsDetected?.Invoke(existedTags.Select(x => x.Value).ToList());
        }

        private int TagInventory(
         Byte AIType,
         Byte AntennaSelCount,
         Byte[] AntennaSel,
         bool enable15693,
         bool enable14443A,
         bool enable18000p3m3,
         Byte enableAFI,
         Byte afiVal,
         ref UInt32 nTagCount)
        {
            try
            {
                int iret;
                UIntPtr InvenParamSpecList = UIntPtr.Zero;
                InvenParamSpecList = RFIDLIB.rfidlib_reader.RDR_CreateInvenParamSpecList();

                if (InvenParamSpecList.ToUInt64() != 0)
                {
                    if (enable15693)
                    {
                        RFIDLIB.rfidlib_aip_iso15693.ISO15693_CreateInvenParam(InvenParamSpecList, 0, 0, 0, 0);
                    }

                    if (enable14443A)
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
                iret = RFIDLIB.rfidlib_reader.RDR_TagInventory(hreader, AIType, AntennaSelCount, AntennaSel, InvenParamSpecList);

                if (iret == 0 || iret == -21)
                {
                    nTagCount += RFIDLIB.rfidlib_reader.RDR_GetTagDataReportCount(hreader);
                    UIntPtr TagDataReport;
                    TagDataReport = (UIntPtr) 0;
                    TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_FIRST); //first
                    var currentTags = new List<TagDto>();

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
                                if (!this._shouldStop)
                                {
                                    var tag = ProcessTagData(aip_id, tag_id, ant_id, uid, (int)uidlen);
                                    if (tag != null)
                                    {
                                        currentTags.Add(tag);
                                    }
                                }

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
                                if (!this._shouldStop)
                                {
                                    var tag = ProcessTagData(aip_id, tag_id, ant_id, uid, (int)uidlen);
                                    if (tag != null)
                                    {
                                        currentTags.Add(tag);
                                    }
                                }
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
                                if (!this._shouldStop)
                                {
                                    var tag = ProcessTagData(aip_id, tag_id, ant_id, uid, (int)uidlen);
                                    if (tag != null)
                                    {
                                        currentTags.Add(tag);
                                    }
                                }
                            }
                        }

                        /* Get Next report from buffer */
                        TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
                    }

                    if (iret == -21) // stop trigger occur,need to inventory left tags
                    {
                        AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE; //use only-new-tag inventory 
                        goto LABEL_TAG_INVENTORY;
                    }

                    iret = 0;
                    ProcessTags(currentTags);
                }

                if (InvenParamSpecList.ToUInt64() != 0) RFIDLIB.rfidlib_reader.DNODE_Destroy(InvenParamSpecList);
                return iret;
            }
            catch (Exception e)
            {
                Log.Error(e, "");
                return -1;
            }

        }

        private TagDto ProcessTagData(UInt32 AIPType, UInt32 tagType, UInt32 antID, Byte[] uid, int uidlen)
        {
            var strUid = BitConverter.ToString(uid, 0, (int)uidlen).Replace("-", string.Empty);
            //if (existedTags.ContainsKey(strUid)) return;

            int iret;
            String strTagTypeName;
            StringBuilder sbAIPName = new StringBuilder();
            sbAIPName.Append('\0', 128);
            UInt32 nSize = (UInt32)sbAIPName.Capacity;
            iret = RFIDLIB.rfidlib_reader.RDR_GetAIPTypeName(hreader, AIPType, sbAIPName, ref nSize);

            StringBuilder sbTagName = new StringBuilder();
            sbTagName.Append('\0', 128);
            nSize = (UInt32)sbTagName.Capacity;
            iret = RFIDLIB.rfidlib_reader.RDR_GetTagTypeName(hreader, AIPType, tagType, sbTagName, ref nSize);


            ConnectTag(strUid);

            // Get RFID tag model
            var model = ReadData(4, 1);
            Log.Information($"Read model data: {model}");

            if (string.IsNullOrEmpty(model) || model.Length < 8)
            {
                DisConnectTag();
                return null;
            }

            // Get custom price
            var customData = ReadData(10, 1);
            Log.Information($"Read custom data: {customData}");

            var dto = new TagDto
            {
                TagId = strUid,
                Model = model.Substring(4, 4),
                CustomData = customData,
                AddedDate = DateTime.Now
            };

            DisConnectTag();
            return dto;
        }

        private string ReadData(uint blkAddress, int trial)
        {
            if (trial > 10)
            {
                return "";
            }

            int iret;
            int idx;
            UInt32 blocksRead = 0;
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
                Log.Information($"ReadData {blkAddress} - success, Trial: {trial}, Data: {txt}");

                return txt;
            }
            else
            {
                Log.Error($"ReadData {blkAddress} - failed, Trial: {trial}");
                var txt = ReadData(blkAddress, trial + 1);

                if (string.IsNullOrEmpty(txt))
                {
                    return "";
                }

                return txt;
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
                return;
            }

            idx = 1;
            if (idx == -1)
            {
                return;
            }

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
                        RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
                        hTag = UIntPtr.Zero;
                        return;
                    }
                }
                /* connect ok */
            }
            else
            {
                Log.Error($"ConnectTag {uuid} - failed");
            }
        }

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

        private static byte[] StringToByteArrayFastest(string hex)
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

        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            // return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public bool WriteTagsData()
        {
            try
            {
                invenThread.Abort();
                _shouldStop = true;
                DisConnectTag();

                foreach (var tag in existedTags)
                {
                    Log.Information($"Disconnect Tag: {tag.Key}");
                    DisConnectTag();

                    Log.Information($"Connect Tag: {tag.Key}");
                    ConnectTag(tag.Key);

                    Log.Information($"Reset Tag: {tag.Key}");
                    bool _resultWrite = WriteData(10, "00000000");

                    if (_resultWrite)
                    {
                        Console.WriteLine($"Reset price of: {tag.Key} done");
                    }
                    else
                    {
                        Console.WriteLine($"Reset price of: {tag.Key} failed");
                    }

                    DisConnectTag();
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, exception.Message);
                DisConnectTag();
                return false;
            }
            finally
            {
                _shouldStop = false;
                invenThread = new Thread(DoInventory);
                invenThread.Start();
            }
        }

        private bool WriteData(uint blkAddress, string data)
        {
            int iret;
            byte[] newBlksData = StringToByteArrayFastest(data);

            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_WriteMultipleBlocks(hreader, hTag, blkAddress, 1, newBlksData, (uint)newBlksData.Length);
            
            if (iret == 0)
            {
                return true;
            }
            else
            {
                Log.Error($"WriteData {blkAddress} {data} - failed");
                DisConnectTag();
                return false;
            }
        }

        #endregion
    }
}
