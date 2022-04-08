using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.CotfPad.Hardware
{

    public class TagReportEvent : Object
    {
        public UInt32 aipType;
        public UInt32 tagType;
        public UInt32 antID;
        public Byte dsfid;
        public Byte[] uid;
        TagReportEvent()
        {
            aipType = 0;
            tagType = 0;
            antID = 0;
            dsfid = 0;
            uid = new Byte[8];
        }

    }
    public class CReaderDriverInf
    {
        public string m_catalog;
        public string m_name;
        public string m_productType;
        public UInt32 m_commTypeSupported;
    }

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

    public class CSupportedAirProtocol
    {
        public UInt32 m_ID;
        public string m_name;
        public Boolean m_en;
    }
}
