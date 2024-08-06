using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppDomain_Test
{
    internal class AppDomain_Monitor
    {
        public static void MonitorAppDomain(AppDomain defalutAD, AppDomain testAD)
        {
            //开启监视，开启后不可被关闭，尝试设置false会导致报错
            AppDomain.MonitoringIsEnabled = true;

            //获取当前CLR实例控制的所有AppDomain正在使用的字节数，仅在上一次回收时时准确的
            var total_size = AppDomain.MonitoringSurvivedProcessMemorySize;
            //获取特定AppDomain已分配的总字节数，仅在上一次回收时时准确的
            var ad_size1 = testAD.MonitoringTotalAllocatedMemorySize;
            //获取特定AppDomain正在使用的字节数，仅在上一次回收时时准确的
            var ad_size2 = testAD.MonitoringSurvivedMemorySize;
            //获取特定AppDomain的CPU使用时间
            var ad_time = testAD.MonitoringTotalProcessorTime;
        }
    }
}
