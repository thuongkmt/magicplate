using KonbiBrain.WindowServices.RFIDTable.Interfaces;
using KonbiBrain.WindowServices.RFIDTable.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.RFIDTable.Helper
{
    public class UID
    {
         
        public string Port = "";//串口
        public int dev_pid = 0;//打开串口生成的id
        private static readonly string lockid = Guid.NewGuid().ToString();
        private static StringBuilder uid = new StringBuilder(320);//盘子UID
        private static StringBuilder types = new StringBuilder(320);//盘子类型
        static public int Count = 0; //读取数量
        public static int VER = 0;
        public LogService Log = new LogService();
        public static List<CoreDish> List { get; private set; }

        #region 调用动态库dll
        [DllImport("SOV_Dish3rd.dll")]//打开串口
        private static extern int SOV_Open(string pPort, int nBautRate = 38400, int ndefault = 0);
        [DllImport("SOV_Dish3rd.dll")]//关闭串口
        private static extern int SOV_Close(int nComm); 
        [DllImport("SOV_Dish3rd.dll")] // nID = 65535, pUidInfo = "0"
        private static extern bool SOV_ReadUID(int hComm, int nID, StringBuilder pInfo);
         
        [DllImport("SOV_Dish3rd.dll")] // nID = 65535, pUidInfo = "0"
        private static extern int BYRD_WriteTagDataInveUID(int hComm, int nID, string pUid, int nBlockNo, int nBlockNum, string pInfo);
        [DllImport("SOV_Dish3rd.dll")]
        public static extern int BYRD_ReadTagDataInveUID(int nComm, int nId, string puid, int nBlockNo, int nBlockNum, StringBuilder buf);
         
        [DllImport("SOV_Dish3rd.dll")]//获取餐盘出品信息
        public static extern int SOV_Read(int hComm, int nID,[In,Out] DishInfo[] pInfo,out int nCount);

        [DllImport("SOV_Dish3rd.dll")]//清除餐盘出品信息
        public static extern int SOV_Clear(int hComm, int nID, string pUid,ref string oUid, ref int nCount);

        [DllImport("SOV_Dish3rd.dll")]//日志标志
        public static extern bool SOV_SetLog(bool bFlag);

        [DllImport("SOV_Dish3rd.dll")]//批量出品
        public static extern int SOV_BatchProduce(int hComm, int nID, int nKind, int nCode, int nPrice, int nNum, int nMachineNum, ref int nTagCount, ref int nVerifyFail, ref int nWriteOK);

        [DllImport("SOV_Dish3rd.dll")]//显示提示信息
        public static extern bool SOV_Screen_DisplayTitle(int hComm, int nID, int nTitleId);

        [DllImport("SOV_Dish3rd.dll")]//发送菜品列表
        public static extern int SOV_Screen_DisplayProd(int hComm, int nID, int nProdListCount, ProdInfo[] ProdList, ushort Amt, ushort Qty);

        [DllImport("SOV_Dish3rd.dll")]//获取版本信息 , CallingConvention = CallingConvention.StdCall  ,CharSet = CharSet.Auto, CallingConvention = CallingConvention.ThisCall)
        public static extern int SOV_GetProductInfo(int hComm, int nID,ref string pValParam);

        [DllImport("SOV_Dish3rd.dll")] //获取餐盘用户自定义区块数据
        public static extern int SOV_Read_UserData(int hComm, int nID, [In, Out] DishDataInfo[] pInfo, out int nCount);

        [DllImport("SOV_Dish3rd.dll")] //批量写餐盘用户自定义区块数据
        public static extern int SOV_BatchWrite_UserData(int hComm, int nID,string nUserData,ref int nTagCount,ref int nVerifyFail,ref int nWriteOK);

        [DllImport("SOV_Dish3rd.dll")] //清除餐盘用户自定义区块数据
        public static extern int SOV_Clear_UserData(int hComm, int nID, string pUid,out string pInfo,out int nCount);

        #endregion

        #region
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public int Open()
        {
            //检测串口名称是否存在
            if (string.IsNullOrEmpty(Port))
            {
                return 0;
            }
            //dev_pid大于0 代表串口已经打开无需再打开
            if (dev_pid > 0)
            {
                return dev_pid;
            }
            using (System.IO.Ports.SerialPort p = new System.IO.Ports.SerialPort())
            {
                p.PortName = Port;
                try
                {
                    p.Open();
                }
                catch (UnauthorizedAccessException e)
                {
                    p.Close();
                    return -2;
                }
                catch (System.IO.IOException e)
                {
                    p.Close();
                    return -2;
                }
                catch (Exception e)
                {
                    p.Close();
                    return -2;
                }
            }
            try
            {
                lock (lockid)
                {
                    dev_pid = SOV_Open(Port);
                }
            }
            catch (Exception e)
            {
                Log.LogException("Open serial Port：" + e.Message);
            }
            return dev_pid;
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        {
            lock (lockid)
            {
                int i = SOV_Close(dev_pid);
            }
            dev_pid = 0;
        }

        /// <summary>
        /// 读取餐盘UID
        /// </summary>
        /// <returns></returns>
        public CoreDish[] ReadUID()
        {
            try
            {
                List<CoreDish> list = new List<CoreDish>();
                if (Open() <= 0) return list.ToArray();
                int count = 0;
                lock (lockid)
                {
                    bool ret = false;
                    try
                    {
                        ret = SOV_ReadUID(dev_pid, 65535, uid);
                    }
                    catch (Exception a)
                    {
                        string ba = a.ToString();
                    }

                    if (!ret)
                    {
                        return null;
                    }
                    uid.ToString().Splic(16).ToList().ForEach(s =>
                    {
                        list.Add(new CoreDish() { pid = s });
                    });
                    int i = 0;
                    string aa = "";
                    string bb = aa.Hash().Substring(20, 4);
                    types.ToString().Splic(16).ToList().ForEach(s =>
                    {
                        list[i].verify = s.Substring(0, 4);
                        list[i].version = s.Substring(8, 1);
                        if (s.Substring(8, 1) != "A")
                        {
                            VER = 0;
                            list[i].code = s.Substring(4, 4);
                        }
                        else
                        {
                            VER = 1;
                            int color = Int32.Parse(s.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                            int type = Int32.Parse(s.Substring(6, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                            if (color > 99 || type > 99)
                            {
                                string s_color = Convert.ToString(color);
                                s_color = s_color.PadLeft(3, '0');
                                string s_type = Convert.ToString(type);
                                s_type = s_type.PadLeft(3, '0');
                                list[i].code = s_color + s_type;
                            }
                            else
                            {
                                string s_color = Convert.ToString(color);
                                s_color = s_color.PadLeft(2, '0');
                                string s_type = Convert.ToString(type);
                                s_type = s_type.PadLeft(2, '0');
                                list[i].code = s_color + s_type;
                            }
                        }
                        i++;
                    });
                    Count = list.Count;
                    return list.ToArray();
                }
            }
            catch (Exception ex)
            {
               Log.LogException("Reading Tray：" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 获取餐盘出品信息 未完成
        /// </summary>
        /// <returns></returns>
        public List<Dish> Read()
        {
            try
            { 
                if (Open() <= 0) return null;
                List<Dish> list = new List<Helper.Dish>();
                int count = 0;
                var array = new DishInfo[100];
                lock (lockid)
                {
                    int ret = 0;
                    try
                    { 
                        ret = SOV_Read(dev_pid, 65535, array, out count); 
                    }
                    catch (Exception a)
                    {
                        return null;
                    } 
                }
                for (int i = 0; i < count; i++)
                { 
                    Dish info = new Helper.Dish();
                    info.UID= Encoding.Default.GetString(array[i].UID);
                    info.UType = Encoding.Default.GetString(array[i].UType);
                    info.TermID= array[i].TermID;
                    info.TermNum = array[i].TermNum;
                    info.ProdNo = array[i].ProdNo;
                    info.ProdPrice = array[i].ProdPrice;
                    list.Add(info);
                }
                return list;
            }
            catch (Exception ex)
            { 
                return null;
            }
        }

        public List<DishData> ReadData()
        {
            try
            {
                if (Open() <= 0) return null;

                List<DishData> list = new List<Helper.DishData>();
                int count = 0;
                var array = new DishDataInfo[100];

                lock (lockid)
                {
                    int ret = 0;
                    try
                    {
                        var retry_id = SOV_Open(Port);
                        if (retry_id > 0)
                            dev_pid = retry_id;
                        ret = SOV_Read_UserData(dev_pid, 65535, array, out count);
                    }
                    catch (Exception a)
                    {
                        return null;
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    DishData info = new Helper.DishData();
                    info.UID = Encoding.Default.GetString(array[i].UID);
                    info.UType = Encoding.Default.GetString(array[i].UType);
                    info.UData = Encoding.Default.GetString(array[i].UData);
                    //info.TermID = array[i].TermID;
                    //info.TermNum = array[i].TermNum;
                    //info.ProdNo = array[i].ProdNo;
                    //info.ProdPrice = array[i].ProdPrice;
                    list.Add(info);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

         
        /// <summary>
        /// 清除餐盘出品信息
        /// </summary>
        /// <param name="puid">默认：0  全部清除  或指定UID清除 格式：E001020304050607E001020304050608 </param>
        /// <returns>清除失败的UID</returns>
        public int ClearDish(string puid,out List<CoreDish> list)
        {
            list = null;
            if (Open() <= 0) return 0;
            string oUid = "";
            int return_id = 0;
            List<CoreDish> list2 = new List<Helper.CoreDish>();
            lock (lockid)
            {
                int count = 0;
               
                try
                {
                    return_id = SOV_Clear(dev_pid, 65535, puid,ref oUid, ref count);
                }
                catch (Exception a)
                {
                    string ba = a.ToString();
                }
            }
            oUid.ToString().Splic(16).ToList().ForEach(s =>
            {
                list2.Add(new CoreDish() { pid = s });
            });
            list = list2;
            return return_id;
        }

        /// <summary>
        /// 日志标志
        /// </summary>
        /// <param name="islog"></param>
        /// <returns>True:设置成功  false:设置失败</returns>
        public bool SetLog(bool islog)
        {
            if (Open() <= 0) return false;
            bool ret = false;
            lock (lockid)
            { 
                
                try
                {
                    ret = SOV_SetLog(islog);
                }
                catch (Exception a)
                {
                    
                }
            }
            return ret;
        }

        /// <summary>
        /// 批量出品
        /// </summary>
        /// <param name="nCode">输入参数,编码值范围1-9999,不使用请填0</param>
        /// <param name="nPrice">输入参数,以分为单位的价格数值,范围1-9999,不使用请填0</param>
        /// <param name="nNum">输入参数,档口号1-9,不使用请填0</param>
        /// <param name="nMachineNum">输入参数,机号1-9</param>
        /// <returns>0成功,-1失败,-2参数错误</returns>
        public int BatchProduce(int nCode, int nPrice, int nNum, int nMachineNum, out int nTagCount, out int nVerifyFail, out int nWriteOK)
        {
            nTagCount = 0;
            nVerifyFail = 0;
            nWriteOK = 0;
            int return_id = 0;
            if (Open() <= 0) return 0;
            lock (lockid)
            {
                try
                {
                    return_id = SOV_BatchProduce(dev_pid, 65535, 1, nCode, nPrice, nNum, nMachineNum, ref nTagCount, ref nVerifyFail, ref nWriteOK);
                }
                catch
                {
                    return_id = -2;
                }
            }
            return return_id;
        }

        /// <summary>
        /// 在屏幕上显示提示信息,传入不同nTitleId显示对应提示信息。
        /// </summary>
        /// <param name="nTitleId">0 请放托盘 1 请刷卡 2 点菜成功 3 请移走托盘 4 已刷新 5 点菜失败 6 非法餐盘 7 未出品餐盘 8 网络异常 9 未开台  10 点菜未完成  11 此单已结  12 找不到菜品  13 未出品  14 单据已锁定  15 准备中...  16 支付成功  17 支付失败 18 卡不可用  19 休息中 20 现金支付 21 未付款</param>
        /// <returns>TRUE:成功 FALSE:失败。</returns>
        public bool DisplayTitle(int nTitleId)
        {
            if (Open() <= 0) return false;
            bool isdisplay = false;
            lock (lockid)
            {
                try
                {
                    isdisplay = SOV_Screen_DisplayTitle(dev_pid, 65535, nTitleId);
                }
                catch
                {
                    isdisplay = false;
                }
            }
            return isdisplay;
        }

        /// <summary>
        /// 发送菜品列表
        /// </summary>
        /// <param name="nProdListCount">修改菜品信息列表个数。</param>
        /// <param name="ProdList">菜品信息列表，清空信息，菜名传入空字符串。最多可传入12个菜品信息列表。</param>
        /// <param name="Amt">合计金额0~999999 以分为单位</param>
        /// <param name="Qty">合计数量 0~999</param>
        /// <returns></returns>
        public int DisplayProd(int nProdListCount, ProdInfo[] ProdList, ushort Amt, ushort Qty)
        {
            if (Open() <= 0) return 0;
            int isdisplay = 0;
            lock (lockid)
            {
                try
                {
                    isdisplay = SOV_Screen_DisplayProd(dev_pid, 65535, ProdList.Count(), ProdList, Amt, Qty); 
                }
                catch(Exception ex)
                {
                    isdisplay = -1;
                }
            }
            return isdisplay;
        }

        ///// <summary>
        ///// 获取版本信息
        ///// </summary>
        ///// <returns></returns>
        //public string GetVerProductInfo()
        //{
        //    if (Open() <= 0) return "";
        //    int isProduct = 0;
        //    string pValParam = ""; 
        //    lock (lockid)
        //    { 
        //        try
        //        {

        //            isProduct = SOV_GetProductInfo(dev_pid, 65535,ref pValParam);
        //        }
        //        catch(Exception ex)
        //        {
        //            isProduct = -1;
        //        }
        //        GC.Collect();
        //    }
        //    return pValParam;
        //}
        #endregion


        #region 自定义

        /// <summary>
        /// 自定义读取
        /// </summary>
        /// <param name="return_id">读取状态</param>
        /// <returns></returns>
        public List<DishData> SOV_Read_UserData(out int return_id)
        {
            return_id = -2;
            if (Open() <= 0)
            {
                return null;
            }
            int count = 0;
            List<DishData> list_uid = new List<DishData>();
            var array = new DishDataInfo[100];
            lock (lockid)
            {
                try
                {
                    return_id = SOV_Read_UserData(dev_pid, 65535, array,out count);
                    if (return_id == 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            DishData info = new Helper.DishData();
                            info.UData = Encoding.Default.GetString(array[i].UData);
                            info.UID = Encoding.Default.GetString(array[i].UID);
                            info.UType = Encoding.Default.GetString(array[i].UType);
                            list_uid.Add(info);
                        }
                    }
                }
                catch
                {
                    return_id = 3;
                }
            }
            return list_uid;
        }

        /// <summary>
        /// 批量写餐盘用户自定义区块数据
        /// </summary>
        /// <param name="nTagCount">返回标签个数</param>
        /// <param name="nVerifyFail">返回验证失败标签个数</param>
        /// <param name="nWriteOK">返回写成功标签个数</param>
        /// <param name="nUserData">用户自定义区块数据，4个字节数据，按照HEX编码规则写入</param>
        /// <returns></returns>
        public int SOV_BatchWrite_UserData(out int nTagCount,out int nVerifyFail,out int nWriteOK, string nUserData = "01 02 03 04")
        {
            nTagCount = 0;
            nVerifyFail = 0;
            nWriteOK = 0;
            int return_id = -2;
            if (Open() <= 0)
            {
                return 0;
            }
            lock (lockid)
            {
                try
                {
                    return_id = SOV_BatchWrite_UserData(dev_pid, 65535, nUserData, ref nTagCount, ref nVerifyFail, ref nWriteOK);
                }
                catch (Exception ex)
                {
                    return_id = -2;
                }
            }
            return return_id;
        }

        /// <summary>
        /// 清除餐盘用户自定义区块数据。
        /// </summary>
        /// <param name="nCount">返回成功清空的标签个数</param>
        /// <param name="uFail">回清空失败的UID信息</param>
        /// <param name="pUid">指定清空信息的UID 0:不指定芯片uid  设置uid则清除指定uid</param>
        /// <returns>状态</returns>
        public int SOV_Clear_UserData(out int nCount,out string uFail, string pUid="0")
        {
            nCount = 0;
            uFail = "";
            int return_id = -2;
            if (Open() <= 0)
            {
                return 0;
            }
            lock (lockid)
            {
                try
                {
                    return_id = SOV_Clear_UserData(dev_pid, 65535, pUid, out uFail, out nCount);
                }
                catch (Exception ex)
                {
                    return_id = -2;
                }
            }
            return return_id;
        } 
        #endregion






    }
}
