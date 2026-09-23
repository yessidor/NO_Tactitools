using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace NO_Tactitools.Core;

public class BindingHelper {
    public abstract class MemberAccessor {
        public abstract object GetValue(object obj);
        public abstract void SetValue(object obj, object value);
    }

    public class PropertyAccessor : MemberAccessor {
        private readonly PropertyInfo _prop;

        public PropertyAccessor(PropertyInfo prop) {
            _prop = prop;
        }

        public override object GetValue(object obj) => _prop?.GetValue(obj);
        public override void SetValue(object obj, object value) => _prop?.SetValue(obj, value);
    }

    public class FieldAccessor : MemberAccessor {
        private readonly FieldInfo _field;

        public FieldAccessor(FieldInfo field) {
            _field = field;
        }

        public override object GetValue(object obj) => _field?.GetValue(obj);
        public override void SetValue(object obj, object value) => _field?.SetValue(obj, value);
    }

    public class ArrayElementAccessor : MemberAccessor {
        private readonly MemberAccessor _containerAccessor;
        private readonly int _index;

        public ArrayElementAccessor(MemberAccessor containerAccessor, int index) {
            _containerAccessor = containerAccessor;
            _index = index;
        }

        public override object GetValue(object obj) {
            object container = _containerAccessor.GetValue(obj);
            if (container is Array array) {
                return array.GetValue(_index);
            }
            throw new InvalidOperationException($"Container is not an array");
        }

        public override void SetValue(object obj, object value) {
            object container = _containerAccessor.GetValue(obj);
            if (container is Array array) {
                array.SetValue(value, _index);
            }
            else {
                throw new InvalidOperationException($"Container is not an array");
            }
        }
    }

    public class ListElementAccessor : MemberAccessor {
        private readonly MemberAccessor _containerAccessor;
        private readonly int _index;

        public ListElementAccessor(MemberAccessor containerAccessor, int index) {
            _containerAccessor = containerAccessor;
            _index = index;
        }

        public override object GetValue(object obj) {
            object container = _containerAccessor.GetValue(obj);
            if (container is IList list) {
                return list[_index];
            }
            throw new InvalidOperationException($"Container does not implement IList");
        }

        public override void SetValue(object obj, object value) {
            object container = _containerAccessor.GetValue(obj);
            if (container is IList list) {
                list[_index] = value;
            }
            else {
                throw new InvalidOperationException($"Container does not implement IList");
            }
        }
    }

    public class DictionaryElementAccessor : MemberAccessor {
        private readonly MemberAccessor _containerAccessor;
        private readonly object _key;

        public DictionaryElementAccessor(MemberAccessor containerAccessor, object key) {
            _containerAccessor = containerAccessor;
            _key = key;
        }

        public override object GetValue(object obj) {
            object container = _containerAccessor.GetValue(obj);
            if (container is IDictionary dict) {
                return dict[_key];
            }
            throw new InvalidOperationException($"Container does not implement IDictionary");
        }

        public override void SetValue(object obj, object value) {
            object container = _containerAccessor.GetValue(obj);
            if (container is IDictionary dict) {
                dict[_key] = value;
            }
            else {
                throw new InvalidOperationException($"Container does not implement IDictionary");
            }
        }
    }

    public struct Binding {
        public object TargetObject { get; set; }
        public MemberAccessor Member { get; set; }
        public object ConfigVariable { get; set; }
        public EventInfo SettingChangedEvent { get; set; }
        public Action<object> SettingChangedCallback { get; set; }

        public Binding(object targetObject, MemberAccessor member, object configVariable, EventInfo settingChangedEvent, Action<object> settingChangedCallback = null) {
            TargetObject = targetObject;
            Member = member;
            ConfigVariable = configVariable;
            SettingChangedEvent = settingChangedEvent;
            SettingChangedCallback = settingChangedCallback;
        }

        public Binding(object targetObject, string memberName, object configVariable, string settingChangedEventName = "SettingChanged", Action<object> settingChangedCallback = null) {
            Type type = null;
            TargetObject = targetObject;
            if (targetObject is Type type_) {
                TargetObject = null;
                type = type_;
            }
            else {
                TargetObject = targetObject;
                type = TargetObject.GetType();
            }
            Member = CreateAccessor(type, memberName);
            ConfigVariable = configVariable;
            SettingChangedEvent = ConfigVariable.GetType().GetEvent(settingChangedEventName);
            SettingChangedCallback = settingChangedCallback;
        }

        public Binding(object targetObject, string containerName, object indexOrKey, object configVariable, string settingChangedEventName = "SettingChanged", Action<object> settingChangedCallback = null) {
            Type type = null;
            TargetObject = targetObject;
            if (targetObject is Type type_) {
                TargetObject = null;
                type = type_;
            }
            else {
                TargetObject = targetObject;
                type = TargetObject.GetType();
            }

            //Get the container's type information
            Type containerType = GetAccessorValueType(type, containerName);

            if (containerType == null) {
                throw new InvalidOperationException($"Cannot determine type of container '{containerName}' on type {type.Name}");
            }

            //Create the container accessor
            MemberAccessor containerAccessor = CreateAccessor(type, containerName);

            //Create the appropriate element accessor based on container type and indexOrKey
            Member = CreateElementAccessor(containerType, containerAccessor, indexOrKey);

            ConfigVariable = configVariable;
            SettingChangedEvent = ConfigVariable.GetType().GetEvent(settingChangedEventName);
            SettingChangedCallback = settingChangedCallback;
        }
    }

    public static void ApplyBindings(params Binding[] bindings) {
        foreach (var binding in bindings) {
            ApplySingleBinding(binding);
        }
    }

    public static void ApplyBindings(IEnumerable<Binding> bindings) {
        foreach (var binding in bindings) {
            ApplySingleBinding(binding);
        }
    }

    private static void ApplySingleBinding(Binding binding) {
        //Set initial value
        var valueProperty = binding.ConfigVariable.GetType().GetProperty("Value");
        object initialValue = valueProperty?.GetValue(binding.ConfigVariable);
        binding.Member.SetValue(binding.TargetObject, initialValue);

        //Create and subscribe handler
        var handler = CreateHandler(binding);
        binding.SettingChangedEvent?.AddMethod?.Invoke(binding.ConfigVariable, new object[] { handler });
    }

    private static MemberAccessor CreateAccessor(Type type, string memberName) {
        System.Reflection.BindingFlags flags = 
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Instance;

        //Regular property or field access
        FieldInfo field = type.GetField(memberName, flags);
        if (field is not null)
            return new FieldAccessor(field);

        PropertyInfo property = type.GetProperty(memberName, flags);
        if (property is not null)
            return new PropertyAccessor(property);

        throw new Exception(string.Format("Cannot get either field or property {0} from {1}", memberName, type));
    }

    private static Type GetAccessorValueType(Type type, string memberName) {
        System.Reflection.BindingFlags flags = 
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Instance;

        FieldInfo field = type.GetField(memberName, flags);
        if (field is not null)
            return field.FieldType;

        PropertyInfo property = type.GetProperty(memberName, flags);
        if (property is not null)
            return property.PropertyType;

        return null;
    }

    private static MemberAccessor CreateElementAccessor(Type containerType, MemberAccessor containerAccessor, object indexOrKey) {
        //Check if it's an array
        if (containerType.IsArray) {
            if (!(indexOrKey is int)) {
                throw new InvalidOperationException($"Array element access requires an int index, but got {indexOrKey?.GetType().Name}");
            }
            return new ArrayElementAccessor(containerAccessor, (int)indexOrKey);
        }

        //Check if it's a list (IList)
        if (typeof(IList).IsAssignableFrom(containerType)) {
            if (!(indexOrKey is int)) {
                throw new InvalidOperationException($"List element access requires an int index, but got {indexOrKey?.GetType().Name}");
            }
            return new ListElementAccessor(containerAccessor, (int)indexOrKey);
        }

        //Check if it's a dictionary (IDictionary)
        if (typeof(IDictionary).IsAssignableFrom(containerType)) {
            return new DictionaryElementAccessor(containerAccessor, indexOrKey);
        }

        throw new InvalidOperationException($"Container type '{containerType.Name}' is not an array, list, or dictionary. Supported types: Array, IList, IDictionary");
    }

    private static EventHandler CreateHandler(Binding binding) {
        var obj = binding.TargetObject;
        var member = binding.Member;
        var configVariable = binding.ConfigVariable;
        var valueProperty = configVariable.GetType().GetProperty("Value");
        var settingChangedCallback = binding.SettingChangedCallback;

        return (sender, args) => {
            object newValue = valueProperty?.GetValue(configVariable);
            member.SetValue(obj, newValue);
            if (settingChangedCallback != null)
                settingChangedCallback(newValue);
        };
    }
}

