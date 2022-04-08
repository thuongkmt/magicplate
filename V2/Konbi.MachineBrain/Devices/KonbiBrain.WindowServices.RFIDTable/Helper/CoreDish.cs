using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.RFIDTable.Helper
{
    public class CoreDish
    {
        /// <summary>
        /// 芯片UID
        /// </summary>
        public string pid { get; set; }

        public string verify { get; set; }
        public string version { get; set; }

        public string code { get; set; }
         
    }

    public class Dish
    {
        /// <summary>
        /// 餐盘标签UID,16进制
        /// </summary> 
        public string UID;
        /// <summary>
        /// 餐盘类型,10进制
        /// </summary> 
        public string UType;
        /// <summary>
        /// 结算器ID,10进制
        /// </summary>
        public int TermID;
        /// <summary>
        /// 天线ID,10进制
        /// </summary>
        public int TermNum;
        /// <summary>
        /// 菜品编码,10进制
        /// </summary>
        public int ProdNo;
        /// <summary>
        /// 菜品价格，价格以分为单位,10进制
        /// </summary>
        public int ProdPrice;
    }

     
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DishInfo
    {
        /// <summary>
        /// 餐盘标签UID,16进制
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] UID;
        /// <summary>
        /// 餐盘类型,10进制
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] UType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] UPrice;
        /// <summary>
        /// 结算器ID,10进制
        /// </summary>
        public int TermID;
        /// <summary>
        /// 天线ID,10进制
        /// </summary>
        public int TermNum;
        /// <summary>
        /// 菜品编码,10进制
        /// </summary>
        public int ProdNo;
        /// <summary>
        /// 菜品价格，价格以分为单位,10进制
        /// </summary>
        public int ProdPrice;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ProdInfo
    {
        /// <summary>
        /// 菜名18个半角字符串,9全角字符串
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 19)]
        public string Name;

        /// <summary>
        /// 菜品价格，价格以分为单位10进制 0-999999
        /// </summary> 
        public int Price;

        /// <summary>
        /// 数量 10进制 0-999
        /// </summary> 
        public int Qty;

        /// <summary>
        /// 状态 0无状态 1已点 2已付
        /// </summary> 
        public int Stat;
    }
     

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DishDataInfo
    {
        /// <summary>
        /// 餐盘标签UID,16进制
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] UID;
        /// <summary>
        /// 餐盘类型,10进制
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] UType;

        /// <summary>
        /// 用户自定义数据块,16进制
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] UData;

    }


    public class DishData
    {
        /// <summary>
        /// 餐盘标签UID,16进制
        /// </summary>
        public string UID;
        /// <summary>
        /// 餐盘类型,10进制
        /// </summary>
        public string UType;

        /// <summary>
        /// 用户自定义数据块,16进制
        /// </summary>
        public string UData;

    }







}
