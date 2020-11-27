using System;
using System.Reflection;

namespace Panda.DynamicWebApi.Attributes
{
    /// <summary>
    /// 自动注册。可用于类，可用于属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class AutowiredAttribute : Attribute
    {
        /// <summary>
        /// 动态注入类型
        /// </summary>
        public Type InterceptorType { get; set; }

        public AutowiredAttribute()
        {

        }
        /// <summary>
        /// 注入切面
        /// </summary>
        /// <param name="type"></param>
        public AutowiredAttribute(Type type)
        {
            this.InterceptorType = type;
        }
    }

    /// <summary>
    /// 属性注入选择器
    /// </summary>
    public class AutowiredPropertySelector : Autofac.Core.IPropertySelector
    {
        public bool InjectProperty(PropertyInfo propertyInfo, object instance)
        {
            return propertyInfo.GetCustomAttribute<AutowiredAttribute>() != null;
        }
    }
}
