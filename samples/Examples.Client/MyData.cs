using System;

namespace Examples.Client
{
    [Serializable]
    public class MyData
    {
        public MyData()
        {
            Value = string.Empty;
        }

        public string Value { get; set;  }

        public MyData(string v)
        {
            Value = v;
        }
    }
}