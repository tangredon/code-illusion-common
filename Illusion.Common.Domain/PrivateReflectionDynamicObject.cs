﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Illusion.Common.Domain
{
    /// <summary>
    /// Taken from http://blogs.msdn.com/b/davidebb/archive/2010/01/18/use-c-4-0-dynamic-to-drastically-simplify-your-private-reflection-code.aspx
    /// </summary>
    internal class PrivateReflectionDynamicObject : DynamicObject 
    {
        private static readonly IDictionary<Type, IDictionary<string, IProperty>> PropertiesOnType =
            new ConcurrentDictionary<Type, IDictionary<string, IProperty>>();

        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private object RealObject { get; set; }

        internal static object WrapObjectIfNeeded(object o)
        {
            if (o == null || o.GetType().IsPrimitive || o is string)
            {
                return o;
            }

            return new PrivateReflectionDynamicObject {RealObject = o};
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var prop = this.GetProperty(binder.Name);

            result = prop.GetValue(this.RealObject, index: null);
            result = WrapObjectIfNeeded(result);

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var prop = this.GetProperty(binder.Name);
            prop.SetValue(this.RealObject, value, index: null);

            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var prop = this.GetIndexProperty();

            result = prop.GetValue(this.RealObject, indexes);
            result = WrapObjectIfNeeded(result);

            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            var prop = this.GetIndexProperty();
            prop.SetValue(this.RealObject, value, indexes);

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = InvokeMemberOnType(this.RealObject.GetType(), this.RealObject, binder.Name, args);
            result = WrapObjectIfNeeded(result);

            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = Convert.ChangeType(this.RealObject, binder.Type);
            return true;
        }

        public override string ToString()
        {
            return this.RealObject.ToString();
        }

        private IProperty GetIndexProperty()
        {
            return this.GetProperty("Item");
        }

        private IProperty GetProperty(string propertyName)
        {
            var typeProperties = GetTypeProperties(this.RealObject.GetType());
            IProperty property;

            if (typeProperties.TryGetValue(propertyName, out property))
            {
                return property;
            }

            var propNames = typeProperties.Keys.Where(name => name[0] != '<').OrderBy(name => name);

            throw new ArgumentException(
                string.Format(
                    "The property {0} doesn't exist on type {1}. Supported properties are: {2}",
                    propertyName,
                    this.RealObject.GetType(),
                    string.Join(", ", propNames)));
        }

        private static IDictionary<string, IProperty> GetTypeProperties(Type type)
        {
            IDictionary<string, IProperty> typeProperties;

            if (PropertiesOnType.TryGetValue(type, out typeProperties))
            {
                return typeProperties;
            }

            typeProperties = new ConcurrentDictionary<string, IProperty>();

            foreach (var prop in type.GetProperties(bindingFlags).Where(p => p.DeclaringType == type))
            {
                typeProperties[prop.Name] = new Property {PropertyInfo = prop};
            }

            foreach (var field in type.GetFields(bindingFlags).Where(p => p.DeclaringType == type))
            {
                typeProperties[field.Name] = new Field {FieldInfo = field};
            }

            if (type.BaseType != null)
            {
                foreach (var prop in GetTypeProperties(type.BaseType).Values)
                {
                    typeProperties[prop.Name] = prop;
                }
            }

            PropertiesOnType[type] = typeProperties;

            return typeProperties;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo",
            MessageId =
                "System.Type.InvokeMember(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Object,System.Object[])",
            Justification = "Revised, okay.")]
        private static object InvokeMemberOnType(Type type, object target, string name, object[] args)
        {
            try
            {
                return type.InvokeMember(
                    name,
                    BindingFlags.InvokeMethod | bindingFlags,
                    null,
                    target,
                    args);
            }
            catch (MissingMethodException)
            {
                return type.BaseType != null ? InvokeMemberOnType(type.BaseType, target, name, args) : null;
            }
        }

        /// <summary>
        /// Simple abstraction to make field and property access consistent
        /// </summary>
        interface IProperty
        {
            string Name { get; }

            object GetValue(object obj, object[] index);

            void SetValue(object obj, object val, object[] index);
        }

        /// <summary>
        /// IProperty implementation over a PropertyInfo
        /// </summary>
        class Property : IProperty
        {
            internal PropertyInfo PropertyInfo { private get; set; }

            string IProperty.Name
            {
                get { return this.PropertyInfo.Name; }
            }

            object IProperty.GetValue(object obj, object[] index)
            {
                return this.PropertyInfo.GetValue(obj, index);
            }

            void IProperty.SetValue(object obj, object val, object[] index)
            {
                this.PropertyInfo.SetValue(obj, val, index);
            }
        }

        /// <summary>
        /// IProperty implementation over a FieldInfo
        /// </summary>
        class Field : IProperty
        {
            internal FieldInfo FieldInfo { private get; set; }

            string IProperty.Name
            {
                get { return this.FieldInfo.Name; }
            }


            object IProperty.GetValue(object obj, object[] index)
            {
                return this.FieldInfo.GetValue(obj);
            }

            void IProperty.SetValue(object obj, object val, object[] index)
            {
                this.FieldInfo.SetValue(obj, val);
            }
        }
    }
}