using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace OptimizedReflection
{
    /// <summary>
    /// 生成动态方法的静态封装
    /// </summary>
    static class ReflectionOptimizationHelper
    {
        /// <summary>
        /// 使用表达式树来生成用于设置目标值的动态方法
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Action<TSource, TValue> SetParamValueByExpression<TSource, TValue>(TSource source, string propertyName)
        {
            var type_source = typeof(TSource);
            if (type_source == null)
                return null;
            var type_value = typeof(TValue);
            var param_this = Expression.Parameter(type_source);
            var param_this_value = Expression.Property(param_this, propertyName);
            var param_value = Expression.Parameter(type_value);
            var assign = Expression.Assign(param_this_value, param_value);
            var lambda = Expression.Lambda<Action<TSource, TValue>>(assign, param_this, param_value).Compile();

            return lambda;
        }

        /// <summary>
        /// 使用EMIT来生成用于设置目标值的动态方法
        /// </summary>
        /// <typeparam name="TSource">目标对象</typeparam>
        /// <typeparam name="TValue">目标参数类型</typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Action<TSource, TValue> SetParamValueByEMIT<TSource, TValue>(TSource source, string propertyName)
        {
            var type_source = typeof(TSource);
            if (type_source == null)
                return null;
            var methodInfo = type_source.GetProperty(propertyName)?.GetSetMethod();//寻找指定属性的Set方法
            if (methodInfo == null)
                return null;
            var type_obj = typeof(object);
            var type_value = typeof(TSource);
            var dynamicMethod = new DynamicMethod($"Set{propertyName}", typeof(void), new Type[] { type_source, type_value }, type_source);//.NET Framework中使用DynamicMethod(String, Type, Type[], Module)指定模块会报错
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, methodInfo);
            il.Emit(OpCodes.Ret);
            var lambda = dynamicMethod.CreateDelegate<Action<TSource, TValue>>();//此泛型方法仅在.NET 5及以上版本提供，更低版本需使用MethodInfo.CreateDelegate(Type)及其重载。

            return lambda;
        }

    }
}
