using System;

namespace HttpBench
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgAttribute : Attribute
    {
        public int Order { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public ArgAttribute(int order, string name, string description = "")
        {
            Order = order;
            Name = name;
            Description = description;
        }

        public string ToSample()
        {
            return Order != 0
                ? string.Format(" -{0} n", Name)
                : string.Format(" (http|https)://server");
        }

        public string ToHelp()
        {
            return Order != 0
                ? string.Format(" -{0}:\t{1}",Name,Description) 
                : string.Empty;
        }
    }
}
