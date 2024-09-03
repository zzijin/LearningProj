using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadSynchronization_Test
{
    internal class ReaderWriterLockSlimTest
    {
        //1、对于同一把锁、多个线程可同时进入读模式。
        //2、对于同一把锁、同时只允许一个线程进入写模式。
        //3、对于同一把锁、同时只允许一个线程进入可升级的读模式。
        //4、通过默认构造函数创建的读写锁是不支持递归的，若想支持递归 可通过构造 ReaderWriterLockSlim(LockRecursionPolicy) 创建实例。
        //5、对于同一把锁、同一线程不可两次进入同一锁状态（开启递归后可以）
        //6、对于同一把锁、即便开启了递归、也不可以在进入读模式后再次进入写模式或者可升级的读模式（在这之前必须退出读模式）。
        //7、再次强调、不建议启用递归。
        //8、读写锁具有线程关联性，即两个线程间拥有的锁的状态相互独立不受影响、并且不能相互修改其锁的状态。
        //9、升级状态：在进入可升级的读模式 EnterUpgradeableReadLock后，可在恰当时间点通过EnterWriteLock进入写模式。
        //10、降级状态：可升级的读模式可以降级为读模式：即在进入可升级的读模式EnterUpgradeableReadLock后， 通过首先调用读取模式EnterReadLock方法，然后再调用 ExitUpgradeableReadLock 方法。

        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Dictionary<int, string> innerCache = new Dictionary<int, string>();
        public string Read(int key)
        {
            //使线程进入读模式
            cacheLock.EnterReadLock();
            try
            {
                return innerCache[key];
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }
        public void Add(int key, string value)
        {
            //使线程进入写模式
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Add(key, value);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public AddOrUpdateStatus AddOrUpdate(int key, string value)
        {
            //获取可升级为写模式的读锁，注意，同时只能有线程可进入可升级模式的读模式
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                string result = null;
                if (innerCache.TryGetValue(key, out result))
                {
                    if (result == value)
                    {
                        return AddOrUpdateStatus.Unchanged;
                    }
                    else
                    {
                        cacheLock.EnterWriteLock();
                        try
                        {
                            innerCache[key] = value;
                        }
                        finally
                        {
                            cacheLock.ExitWriteLock();
                        }
                        return AddOrUpdateStatus.Updated;
                    }
                }
                else
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        innerCache.Add(key, value);
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                    return AddOrUpdateStatus.Added;
                }
            }
            finally
            {
                //退出读锁
                cacheLock.ExitUpgradeableReadLock();
            }
        }
        public enum AddOrUpdateStatus
        {
            Added,
            Updated,
            Unchanged
        };
    }
}
