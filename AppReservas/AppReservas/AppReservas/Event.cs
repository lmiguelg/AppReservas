using System;
using System.Collections.Generic;
using System.Text;

namespace AppReservas
{
    public class Event
    {
        public string day { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public int length { get; set; }
        public int lengthType { get; set; }
        public string startPlace { get; set; }
        public string endPlace { get; set; }
        public string service { get; set; }
        public string product { get; set; }
        public string productName { get; set; }
        public string availabilityName { get; set; }
        public int startPlaceId { get; set; }
        public int endPlaceId { get; set; }
        public int consumptionPeriodId { get; set; }
        public int serviceId { get; set; }
        public int productId { get; set; }
        public int dayPerioId { get; set; }
        public int availabilityId { get; set; }
    }

    public class DayList
    {
        public List<Event> events { get; set; }
        public DateTime day { get; set; }
    }

    public class D
    {
        public string __type { get; set; }
        public List<DayList> dayList { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int errorCode { get; set; }
    }

    public class RootObject
    {
        public D d { get; set; }
    }
}
