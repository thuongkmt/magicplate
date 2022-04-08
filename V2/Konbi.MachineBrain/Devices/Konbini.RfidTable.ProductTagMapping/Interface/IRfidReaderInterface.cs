using Konbini.RfidFridge.TagManagement.DTO;
using System;
using System.Collections.Generic;

namespace Konbini.RfidFridge.TagManagement.Interface
{
    public interface IRfidReaderInterface
    {
        bool Connect();
        void StartRecord();
        void StopRecord();
        Action<List<TagDTO>> OnTagsRecord { get; set; }
        bool WriteTagsData(List<TagDTO> tags, string code);
        string ReadData(uint blkAddress);
        void ConnectTag(string uuid);
        bool GetReaderInfor();
        void FormClosing();
    }
}
