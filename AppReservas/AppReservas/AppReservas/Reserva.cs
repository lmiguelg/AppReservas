using System;
using System.Collections.Generic;
using System.Text;

namespace AppReservas
{
  

        public class RootObject3
        {
            public D3 d { get; set; }
        }

        public class D3
        {
            public string __type { get; set; }
            public string startDate { get; set; }
            public string endDate { get; set; }
            public int status { get; set; }
            public string code { get; set; }
            public float fullValue { get; set; }
            public float payValue { get; set; }
            public int errorCode { get; set; }
            public string errorDescription { get; set; }
        }

    }



