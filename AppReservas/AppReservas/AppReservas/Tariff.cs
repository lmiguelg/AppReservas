using System;
using System.Collections.Generic;
using System.Text;

namespace AppReservas
{


    public class Tariff
    {
        public string product { get; set; }
        public string availability { get; set; }
        public string standard { get; set; }
        public string name { get; set; }
        public float value { get; set; }
        public string coin { get; set; }
        public int productId { get; set; }
        public int periodId { get; set; }
        public int tariffId { get; set; }
        public int dayPeriodId { get; set; }
        public int availabilityId { get; set; }
        public int minAge { get; set; }
        public int maxAge { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public int periodType { get; set; }
        public string restrictions { get; set; }
        public bool perPerson { get; set; }
        public string paymentConditions { get; set; }
        public string observations { get; set; }
    }

    public class D2
    {
        public string __type { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public List<Tariff> tariffs { get; set; }
        public List<object> suplements { get; set; }
        public int errorCode { get; set; }
    }

    public class RootObject2
    {
        public D2 d { get; set; }
    }

}
