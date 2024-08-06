using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppDomain_Test
{
    internal class AppDomain_Marshalling
    {
        public static void Marshalling(AppDomain defalutAD, AppDomain testAD)
        {
            string defalutEntryAssembly = Assembly.GetEntryAssembly().FullName;
            Console.WriteLine($"默认Assembly FullName:{defalutEntryAssembly}");

            MarshalByRefType mbrt = null;

            ////按引用封送
            //使用：
            //调用AppDomain.CreateInstanceAndUnwrap()
            //在指定AppDomain(源AppDomain)中创建对象，并将对象封送回来(目标AppDomain)
            //该对象继承MarshalByRefObject
            mbrt = (MarshalByRefType)testAD.CreateInstanceAndUnwrap(defalutEntryAssembly, typeof(MarshalByRefType).FullName);
            Console.WriteLine($"Type={mbrt.GetType()}");

            //原理：
            //源AppDomain：原始对象
            //目标AppDomain：代理对象
            //在实际中，源AppDomain与目标AppDomain中的对象并非同一个对象，因为CLR不允许一个AppDomain中的变量引用另一个AppDomain中创建的对象
            //源AppDomain向目标AppDomain发送/返回对象引用时，CLR会在目标AppDomain的Loader堆定义一个代理类型
            //代理类型根据原始类型的元数据定义，因此看起来：1.原始对象完全一样;2.有完全一样的实例成员(属性、事件、方法)
            //但代理类型中实例成员实际与原始类型的不一致，代理类型中的实例成员指出了真实对象所属的AppDomain及如何咋所属AppDomain中找到真实对象(即代理对象使用了GCHandle实例引用真实对象)
            //性能：
            //原始对象修改实例字段时，会调用FieldSetter和FieldGetter来使用代理对象，这些方法利用反射获取和设置字段值，因此性能很差
            //回收：
            //原始对象在testAD中没有根，会被GC回收，因此使用了租约管理器解决此问题
            //原始对象创建后会保持对象至少存活5分钟，每次调用续订租期，保证其在2分钟之内存活
            //重写MarshalByRefObject.InitializeLifetimeService可自定义租期设定
            //默认租期设定值:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime
            Console.WriteLine($"Is proxy={RemotingServices.IsTransparentProxy(mbrt)}");

            ////按值封送
            //使用：
            //调用AppDomain.CreateInstanceAndUnwrap()
            //在指定AppDomain(源AppDomain)中创建对象，并将对象封送回来(目标AppDomain)
            //该对象被标识Serializable,使用序列化功能传递
            var mbvt = (MarshalByValType)testAD.CreateInstanceAndUnwrap(defalutEntryAssembly, typeof(MarshalByValType).FullName);
            Console.WriteLine($"Type={mbvt.GetType()}");
            //原理：
            //将对象通过序列化封送回来，再反序列化创建对象
            Console.WriteLine($"Is proxy={RemotingServices.IsTransparentProxy(mbvt)}");
        }

        //可跨越AppDomain边界，按引用封送
        public sealed class MarshalByRefType : MarshalByRefObject
        {
            private DateTime m_creationTime = DateTime.Now;

            public MarshalByRefType()
            {
                Console.WriteLine($"{GetType()} .ctor(),运行于{Thread.GetDomain().FriendlyName},创建时间:{m_creationTime:D}");
            }

            public void SomeMethod()
            {
                Console.WriteLine($"{GetType()} SomeMethod(),运行于{Thread.GetDomain().FriendlyName},创建时间:{m_creationTime:D}");
            }

            public MarshalByValType MethodWithReturn()
            {
                Console.WriteLine($"{GetType()} MethodWithReturn(),运行于{Thread.GetDomain().FriendlyName},创建时间:{m_creationTime:D}");
                MarshalByValType t = new MarshalByValType();
                return t;
            }

            public NonMarshalByRefType MethodArgAndReturn(string callingDomainName)
            {
                Console.WriteLine($"{GetType()} MethodWithReturn(),调用于{callingDomainName},运行于{Thread.GetDomain().FriendlyName},创建时间:{m_creationTime:D}");
                NonMarshalByRefType t = new NonMarshalByRefType();
                return t;
            }

        }

        //可跨越AppDomain边界，按值封送
        [Serializable]
        public sealed class MarshalByValType
        {
            private DateTime m_creationTime = DateTime.Now;

            public MarshalByValType()
            {
                Console.WriteLine($"{GetType()} .ctor(),运行于{Thread.GetDomain().FriendlyName},创建时间:{m_creationTime:D}");
            }

            public override string ToString()
            {
                return m_creationTime.ToLongDateString();
            }
        }

        //不可跨AppDomain边界,
        public sealed class NonMarshalByRefType
        {
            private DateTime m_creationTime = DateTime.Now;

            public NonMarshalByRefType()
            {
                Console.WriteLine($"{GetType()} .ctor(),运行于{Thread.GetDomain().FriendlyName},创建时间:{m_creationTime:D}");
            }
        }
    }
}
