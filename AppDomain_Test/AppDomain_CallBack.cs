using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppDomain_Test
{
    internal class AppDomain_CallBack
    {
        public static void AppDomainCallBack(AppDomain defalutAD, AppDomain testAD)
        {
            //加载程序集时通知
            testAD.AssemblyLoad += (sender, e) =>
            {
                var assembly = e.LoadedAssembly;
            };

            //卸载AppDomain时通知
            testAD.DomainUnload += (sender, e) =>
            {

            };

            //发生异常后、查找catch前通知
            testAD.FirstChanceException += (sender, e) =>
            {
                var exception = e.Exception;
            };

            //发生异常且未查找到catch时通知
            testAD.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject;
                var terminating = e.IsTerminating;
            };

            //默认AppDomain的父进程存在时
            testAD.ProcessExit += (sender, e) =>
            {

            };

            //对程序集的解析失败时通知
            testAD.AssemblyResolve += (sender, e) =>
            {
                var name = e.Name;
                var assembly = e.RequestingAssembly;
                return null;
            };

            //
            testAD.ReflectionOnlyAssemblyResolve += (sender, e) =>
            {
                return null;
            };

            //
            testAD.ResourceResolve += (sender, e) =>
            {
                return null;
            };

            //
            testAD.TypeResolve += (sender, e) =>
            {
                return null;
            };
        }
    }
}
