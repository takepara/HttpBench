using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace HttpBench
{
    public class HttpSettings : DynamicObject
    {
        [Arg(1, "n", "実行回数")]
        public int Times { get; set; }
        [Arg(2, "c", "同時実行")]
        public int Concurrent { get; set; }
        [Arg(3, "A","BASIC認証(uid:pwd)")]
        public string BasicAuthentication { get; set; }

        [Arg(4, "Rwm", "リクエスト前待ち時間(ms)")]
        public int WaitMilliseconds { get; set; }

        [Arg(5, "Wu", "準備リクエスト回数")]
        public int Warmup { get; set; }

        [Arg(0, "U", "URL")]
        public Uri Url { get; set; }

        private static Dictionary<string, PropertyAccessor> _propertiesAccessor;
        private class PropertyAccessor
        {
            public string Name { get; set; }
            public ArgAttribute Attribute { get; set; }
            public Action<HttpSettings, object> Setter { get; set; }
            public Func<HttpSettings, object> Getter { get; set; }
        }

        public bool IsValid
        {
            get { return Url != null && Times > 0 && Concurrent > 0 && Times >= Concurrent; }
        }

        static HttpSettings()
        {
            var props = typeof(HttpSettings).GetProperties();
            var attrs =
                props.Select(
                    prop =>
                    new
                        {
                            Property = prop,
                            Attribute = prop.GetCustomAttributes(typeof(ArgAttribute), false)
                                       .OfType<ArgAttribute>()
                                       .FirstOrDefault()
                        });


            _propertiesAccessor = attrs.Where(attr => attr.Attribute != null).ToDictionary(
                attr => attr.Attribute.Name,
                attr => new PropertyAccessor
                {
                    Name = attr.Attribute.Name,
                    Attribute = attr.Attribute,
                    Getter = (instance) => attr.Property.GetValue(instance, new object[] { }),
                    Setter = (instance, value) =>
                                 {
                                     var converter = TypeDescriptor.GetConverter(attr.Property.PropertyType);
                                     var setValue = converter.ConvertFrom(value.ToString());
                                     attr.Property.SetValue(instance, setValue, new object[] { });
                                 }
                });
        }

        public static string GetHelp()
        {
            var helps = _propertiesAccessor.Select(kv => kv.Value.Attribute.ToHelp());
            return string.Join("\n", helps);
        }

        public object this[String index]
        {
            get
            {
                return _propertiesAccessor[index].Getter(this);
            }

            set
            {
                _propertiesAccessor[index].Setter(this, value);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!_propertiesAccessor.ContainsKey(binder.Name))
                return base.TryGetMember(binder, out result);

            result = _propertiesAccessor[binder.Name].Getter(this);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!_propertiesAccessor.ContainsKey(binder.Name))
                return base.TrySetMember(binder, value);

            if (binder.Name == "U" && value.GetType() == typeof(string))
                value = new Uri((string)value);

            _propertiesAccessor[binder.Name].Setter(this, value);
            return true;
        }
    }
}
