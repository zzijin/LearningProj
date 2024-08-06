using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppDomain_Test
{
    internal class AppDomain_Create
    {
        public static void CreateAppDomain()
        {
            AppDomain adCallingThreadDomain = Thread.GetDomain();

            //默认AppDomain的友好名称
            string callingDomainName = adCallingThreadDomain.FriendlyName;
            Console.WriteLine($"此线程默认AppDomain friendlyName:{callingDomainName}");

            //默认AppDomain的入口程序集名称
            string defalutEntryAssembly = Assembly.GetEntryAssembly().FullName;
            Console.WriteLine($"默认Assembly FullName:{defalutEntryAssembly}");

            AppDomain ad2 = null;
            Console.WriteLine($" {Environment.NewLine}{ad2} Demo #1");

            //创建AppDomain并配置其权限集和配置信息,无则新AppDomain继承创建它的AppDomain。
            ad2 = AppDomain.CreateDomain("AD #2", null, null);
        }

        //创建AppDomain并执行一个WPF应用
        public static void CreateAppDomainAndExecuteWPF()
        {
            AppDomain adCallingThreadDomain = Thread.GetDomain();

            //默认AppDomain的友好名称
            string defalutADName = adCallingThreadDomain.FriendlyName;
            Console.WriteLine($"此线程默认AppDomain friendlyName:{defalutADName}");

            //默认AppDomain的入口程序集名称
            string defalutEntryAssembly = Assembly.GetEntryAssembly().FullName;
            Console.WriteLine($"默认Assembly FullName:{defalutEntryAssembly}");

            AppDomain ad2 = null;
            Console.WriteLine($" {Environment.NewLine}{ad2} Demo #1");

            //创建AppDomain并配置其权限集和配置信息,无则新AppDomain继承创建它的AppDomain。
            ad2 = AppDomain.CreateDomain("AD #2", null, null);
            string ad2Name = adCallingThreadDomain.FriendlyName;

            ad2.DomainUnload += (sender, e) =>
            {
                Console.WriteLine($"此线程默认AppDomain friendlyName:{Thread.GetDomain().FriendlyName}");
                var appdomain = sender as AppDomain;
                //不可使用ad2，ad2为默认AppDomain中的对象
                Console.WriteLine($"{appdomain.FriendlyName}AppDomain被卸载!");
            };

            Thread thread = new Thread(() =>
            {
                Console.WriteLine($"执行WPF程序!");
                //此方法为同步方法
                ad2.ExecuteAssembly("WpfApplication1.exe");
                Console.WriteLine($"执行WPF程序完成，即将卸载!");
                AppDomain.Unload(ad2);
            });

            //启动UI需要线程为STA
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
