using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Interfaces
{
    public interface IAlarm
    {
        void StartAlarmRTCWakeUp(TimeSpan hourToTrigger, int alarmCode, TimeSpan interval );
        void CancelAlarm(int alarmCode);
    }
}
