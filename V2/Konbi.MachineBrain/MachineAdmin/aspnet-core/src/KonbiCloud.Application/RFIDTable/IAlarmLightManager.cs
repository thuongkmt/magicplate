using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.RFIDTable
{
    public interface IAlarmLightManager : IDeviceManager, ISingletonDependency
    {
        /// <summary>
        /// Control Alarm light
        /// </summary>
        /// <param name="green"></param>
        /// <param name="red"></param>
        /// <param name="beep"></param>
        /// <param name="blink"></param>
        /// <param name="duration"></param>
        /// <param name="soundIntruction"></param>
        /// <returns></returns>
        bool Switch(bool green, bool red, bool beep, bool blink, int duration, string soundIntruction);
        bool Off();
    }
}
