namespace Kamaji.Data.Models
{
    using System;

    //public enum ScanScheduleType
    //{
    //    OnDemand = 0, //if it is null
    //    Weekly,//and also Daily, EnumFlags
    //    Monthly,
    //    ExactTimed
    //}


    [Flags]
    public enum DaysOfWeek
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,
    }

    public interface IScanSchedule
    {
        bool IsItTime(DateTime value);
    }

    public interface IScanScheduleModel : IScanSchedule
    {
        object ScanScheduleId { get; set; }

        // ScanScheduleType ScheduleType { get; set; }

        TimeSpan? Time { get; set; }

        DaysOfWeek? Days { get; set; }

        byte? Day { get; set; }

        DateTime? ExactDateTime { get; set; }
    }

    public abstract class ScanScheduleModelBase<Id> : IScanScheduleModel
    {
        public abstract Id ScanScheduleId { get; set; }
        object IScanScheduleModel.ScanScheduleId { get => this.ScanScheduleId; set => this.ScanScheduleId = (Id)value; }

        public abstract TimeSpan? Time { get; set; }
        public abstract DaysOfWeek? Days { get; set; }
        public abstract byte? Day { get; set; }
        public abstract DateTime? ExactDateTime { get; set; }

        public bool IsItTime(DateTime value)
        {
            if (this.Time != null)
            {
                if (this.Days != null)
                    return new WeeklyScanScheduled(this.Time.Value, this.Days.Value).IsItTime(value);
                else if (this.Day != null)
                    return new MonthlyScanScheduled(this.Time.Value, this.Day.Value).IsItTime(value);
                else if (this.ExactDateTime != null)
                    return new ExactTimedScanScheduled(this.ExactDateTime.Value).IsItTime(value);
            }

            return true;
        }

        private abstract class ScanScheduleBase : IScanSchedule
        {
            protected ScanScheduleBase(TimeSpan time)
            {
                this.Time = time;
            }

            public TimeSpan Time { get; }
            public abstract bool IsItTime(DateTime value);

            protected bool IsItTime(ref DateTime value) => this.Time <= value.TimeOfDay;
        }

        private sealed class WeeklyScanScheduled : ScanScheduleBase
        {
            internal WeeklyScanScheduled(TimeSpan time, DaysOfWeek days)
                : base(time)
            {
                this.Days = days;
            }
            public DaysOfWeek Days { get; set; }

            public override bool IsItTime(DateTime value)
            {
                DaysOfWeek days = (DaysOfWeek)Enum.Parse(typeof(DaysOfWeek), value.DayOfWeek.ToString());
                return this.Days.HasFlag(days) && this.IsItTime(ref value);//if the day and the time are match.
            }
        }

        private sealed class MonthlyScanScheduled : ScanScheduleBase
        {
            internal MonthlyScanScheduled(TimeSpan time, byte day)
                  : base(time)
            {
                this.Day = day;
            }
            public byte Day { get; set; }

            public override bool IsItTime(DateTime value)
                => this.Day == value.Day && this.IsItTime(ref value);
        }

        private sealed class ExactTimedScanScheduled : IScanSchedule
        {
            internal ExactTimedScanScheduled(DateTime time)
            {
                this.Time = time;
            }
            public DateTime Time { get; set; }

            public bool IsItTime(DateTime value) => this.Time <= value;
        }
    }
}
