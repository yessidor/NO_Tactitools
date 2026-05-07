using System;
using System.Reflection;

namespace NO_Tactitools.Core;

public class BindingHelper {
    public abstract class MemberAccessor {
        public abstract object GetValue(object? obj);
        public abstract void SetValue(object? obj, object value);
    }

    public class PropertyAccessor : MemberAccessor {
        private readonly PropertyInfo _prop;

        public PropertyAccessor(PropertyInfo prop) {
            _prop = prop;
        }

        public override object GetValue(object? obj) => _prop?.GetValue(obj);
        public override void SetValue(object? obj, object value) => _prop?.SetValue(obj, value);
    }

    public class FieldAccessor : MemberAccessor {
        private readonly FieldInfo _field;

        public FieldAccessor(FieldInfo field) {
            _field = field;
        }

        public override object GetValue(object? obj) => _field?.GetValue(obj);
        public override void SetValue(object? obj, object value) => _field?.SetValue(obj, value);
    }

    public struct Binding {
        public object TargetObject { get; set; }
        public MemberAccessor Member { get; set; }
        public object ConfigVariable { get; set; }
        public EventInfo SettingChangedEvent { get; set; }

        public Binding(object targetObject, MemberAccessor member, object configVariable, EventInfo settingChangedEvent) {
            TargetObject = targetObject;
            Member = member;
            ConfigVariable = configVariable;
            SettingChangedEvent = settingChangedEvent;
        }

        public Binding(object targetObject, string memberName, object configVariable, string settingChangedEventName = "SettingChanged") {
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
        }
    }

    public static void ApplyBindings(params Binding[] bindings) {
        foreach (var binding in bindings)
        {
            ApplySingleBinding(binding);
        }
    }

    private static void ApplySingleBinding(Binding binding) {
        var valueProperty = binding.ConfigVariable.GetType().GetProperty("Value");

        // Set initial value
        object initialValue = valueProperty?.GetValue(binding.ConfigVariable);
        binding.Member.SetValue(binding.TargetObject, initialValue);

        // Create and subscribe handler
        var handler = CreateHandler(binding.TargetObject, binding.Member, binding.ConfigVariable, valueProperty);
        binding.SettingChangedEvent?.AddMethod?.Invoke(binding.ConfigVariable, new object[] { handler });
    }

    private static MemberAccessor CreateAccessor(Type type, string memberName) {
        System.Reflection.BindingFlags flags = 
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Instance;

        FieldInfo field = type.GetField(memberName, flags);
        if (field is not null)
            return new FieldAccessor(field);

        PropertyInfo property = type.GetProperty(memberName, flags);
        if (property is not null)
            return new PropertyAccessor(property);

        throw new Exception (string.Format("Cannot get either field or property {0} from {1}", memberName, type));
    }

    private static EventHandler CreateHandler(object obj, MemberAccessor member, object configVar, PropertyInfo valueProperty)
    {
        return (sender, args) =>
        {
            object newValue = valueProperty?.GetValue(configVar);
            member.SetValue(obj, newValue);
        };
    }
}
