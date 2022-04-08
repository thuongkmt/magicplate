using Konbini.RfidFridge.TagManagement.DTO;
using Konbini.RfidFridge.TagManagement.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Konbini.RfidFridge.TagManagement.Service
{
    public class RfidReaderInterface : IRfidReaderInterface
    {
        public UIntPtr hreader;
        public UIntPtr hTag;
        Thread InvenThread;
        private bool _shouldStop;
        Byte[] AntennaSel = new byte[16];
        Byte AntennaSelCount = 0;
        public Byte enableAFI;
        public Byte AFI;

        public RfidReaderInterface()
        {
            // Load DLL
            RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers("\\Drivers");
            TagRawData = new List<Tuple<string, int>>();
            hTag = (UIntPtr)0;
        }

        public Action OnRecordFinish { get; set; }
        public Action<List<TagDTO>> OnTagsRecord { get; set; }
        public int RecordCount { get; set; }
        public List<Tuple<string, int>> TagRawData { get; set; }
        public bool Connect()
        {
            var connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + "M201" + ";" +
             RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_USB + ";" +
             RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDADDRMODE + "=" + "0" + ";" +
             RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDSERNUM + "=" + "";

            var iret = RFIDLIB.rfidlib_reader.RDR_Open(connstr, ref hreader);
            SeriLogService.LogInfo("Connect to hardware: " + iret);
            return iret == 0;
        }

        public void StartRecord()
        {
            RecordCount = 0;
            InvenThread = new Thread(DoInventory);
            InvenThread.Start();
        }

        public void StopRecord()
        {
            _shouldStop = true;
            RFIDLIB.rfidlib_reader.RDR_SetCommuImmeTimeout(hreader);
        }

        public void DoInventory()
        {
            int iret;
            Byte AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;

            while (!_shouldStop)
            {
                Connect();
                UInt32 nTagCount = 0;
                iret = TagInventory(AIType, AntennaSelCount, AntennaSel, ref nTagCount);
                if (iret == 0)
                {
                    // inventory ok
                }
                else
                {
                    // inventory error 
                }
                AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
            }
            OnRecordFinish?.Invoke();
            RFIDLIB.rfidlib_reader.RDR_ResetCommuImmeTimeout(hreader);

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
                var tags = new List<TagDTO>();
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


                    iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ParseTagDataReport(TagDataReport, ref aip_id, ref tag_id, ref ant_id, ref dsfid, uid);
                    if (iret == 0)
                    {
                        uidlen = 8;
                        object[] pList = { aip_id, tag_id, ant_id, uid, (int)uidlen };
                        if (!this._shouldStop)
                        {
                            tags.Add(ProcessTagData(aip_id, tag_id, ant_id, uid, (int)uidlen));
                        }

                    }
                    TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
                }

                OnTagsRecord?.Invoke(tags);

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

        private TagDTO ProcessTagData(UInt32 AIPType, UInt32 tagType, UInt32 antID, Byte[] uid, int uidlen)
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
            var model = ReadData(4);
            SeriLogService.LogError($"Read data result: {model}");
            var plateModel = model;
            if (!string.IsNullOrEmpty(model) && model.Length >= 8)
            {
                plateModel = model.Substring(4, 4);
            }
            var dto = new TagDTO
            {
                UID = strUid,
                PlateModel = plateModel
            };
            DisConnectTag();
            return dto;
        }

        public string ReadData(uint blkAddress)
        {
            int iret;
            int idx;
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
                if (blkAddress == 4 && txt.Length >= 8)
                {
                    txt = txt.Substring(4, 4);
                }
                return txt;
            }
            else
            {
                SeriLogService.LogError($"ReadData {blkAddress}- failed");
                //MessageBox.Show("Read data failed");
                return "";
            }
        }

        public bool WriteTagsData(List<TagDTO> tags, string code)
        {
            try
            {
                InvenThread.Abort();
                _shouldStop = true;
                foreach (var tag in tags)
                {
                    ConnectTag(tag.UID);
                    bool _resultWrite = WriteData(4, code);
                    if (_resultWrite)
                    {
                        SeriLogService.LogInfo($"Reset price of: {tag.UID} done");
                    }
                    else
                    {
                        SeriLogService.LogError($"Reset price of: {tag.UID} failed");
                    }
                    DisConnectTag();
                }
                return true;
            }
            catch (Exception exception)
            {
                SeriLogService.LogError(exception.Message);
                SeriLogService.LogError("There was error, please try again!");
                DisConnectTag();
                return false;
            }
            finally
            {
                _shouldStop = false;
                InvenThread = new Thread(DoInventory);
                InvenThread.Start();
            }
        }

        private bool WriteData(uint blkAddress, string data)
        {
            int iret;
            var trickData = $"0000{data}";
            byte[] newBlksData = StringToByteArrayFastest(trickData);

            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_WriteMultipleBlocks(hreader, hTag, blkAddress, 4, newBlksData, (uint)newBlksData.Length);
            if (iret == 0)
            {
                //MessageBox.Show("ok !");
                return true;
            }
            else
            {
                SeriLogService.LogError($"WriteData {blkAddress} {data}- failed");
                DisConnectTag();
                return false;
            }
        }

        public void ConnectTag(string uuid)
        {
            SeriLogService.LogError($"Start connect tag {uuid}");

            //if (hTag != UIntPtr.Zero)
            //{
            //    return;
            //}

            var idx = 1;

            // set uid
            byte[] uid = StringToByteArrayFastest(uuid);

            //set tag type default is NXP icode sli 
            UInt32 tagType = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
            tagType = 7; //slix

            // set address mode 
            Byte addrMode = (Byte)idx;

            // do connection
            var iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_Connect(hreader, tagType, addrMode, uid, ref hTag);
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
                        //MessageBox.Show("reset tag fail");
                        RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
                        hTag = UIntPtr.Zero;
                        return;
                    }
                }
                /* connect ok */
            }
            else
            {
                SeriLogService.LogError($"ConnectTag {uuid}- failed");
                //MessageBox.Show($"Connect tag {uuid} failed");
            }
        }

        private void DisConnectTag()
        {
            SeriLogService.LogInfo("Start disconnect tag.");

            int iret;

            // do disconnection
            iret = RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
            if (iret == 0)
            {
                hTag = (UIntPtr)0;
            }
            else
            {
                SeriLogService.LogError("DisConnectTag - failed");
                //MessageBox.Show("Disconnect tag failed!");
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
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public bool GetReaderInfor()
        {
            try
            {
                int iret;
                StringBuilder devInfor = new StringBuilder();
                devInfor.Append('\0', 128);
                UInt32 nSize;
                nSize = (UInt32)devInfor.Capacity;
                iret = RFIDLIB.rfidlib_reader.RDR_GetReaderInfor(hreader, 0, devInfor, ref nSize);
                if (iret == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                SeriLogService.LogError($"GetReaderInfor - failed {ex}");
                return false;
            }
        }

        public void FormClosing()
        {
            _shouldStop = true;
            InvenThread?.Abort();
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

            /*
             *  Close reader driver ,this API is required
             */
            iret = RFIDLIB.rfidlib_reader.RDR_Close(hreader);
            if (iret == 0)
            {
                hreader = (UIntPtr)0;

            }
        }
    }
}
