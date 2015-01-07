/*******************************************************************************************************************************
 * AK.Chore.Domain.Tasks.Recurrence
 * Copyright © 2014-2015 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of CHORE.
 *  
 * CHORE is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * CHORE is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with CHORE.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using AK.Commons.DomainDriven;
using System;
using System.Linq;
using System.Text;

#endregion

namespace AK.Chore.Domain.Tasks
{
    /// <summary>
    /// Represents the domain of a recurrence of a task.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Recurrence : IValueObject<Recurrence>
    {
        public bool IsEnabled { get; protected set; }
        public RecurrenceType Type { get; protected set; }
        public int Interval { get; protected set; }
        public TimeSpan TimeOfDay { get; protected set; }
        public DayOfWeek[] DaysOfWeek { get; protected set; }
        public int DayOfMonth { get; protected set; }
        public int MonthOfYear { get; protected set; }
        public TimeSpan Duration { get; protected set; }

        protected Recurrence()
        {
        }

        public string Summary
        {
            get
            {
                if (this.Type == RecurrenceType.NonRecurring) return "Non-Recurring";

                var builder = new StringBuilder();
                builder.Append("Recurring ");

                if (this.Interval == 1) builder.Append(this.Type.ToString().ToLower());
                else
                {
                    builder.AppendFormat("every {0} ", this.Interval);
                    switch (this.Type)
                    {
                        case RecurrenceType.Hourly:
                            builder.Append("hours");
                            break;
                        case RecurrenceType.Daily:
                            builder.Append("days");
                            break;
                        case RecurrenceType.Weekly:
                            builder.Append("weeks");
                            break;
                        case RecurrenceType.Monthly:
                            builder.Append("months");
                            break;
                        case RecurrenceType.Yearly:
                            builder.Append("years");
                            break;
                    }
                }

                if (!this.IsEnabled) builder.Append(", currently disabled");

                return builder.ToString();
            }
        }

        public static Recurrence NonRecurring()
        {
            return new Recurrence
                {
                    IsEnabled = true,
                    Type = RecurrenceType.NonRecurring
                };
        }

        public static Recurrence Enabled(Recurrence source)
        {
            return new Recurrence
                {
                    IsEnabled = true,
                    Type = source.Type,
                    Interval = source.Interval,
                    TimeOfDay = source.TimeOfDay,
                    DaysOfWeek = source.DaysOfWeek,
                    DayOfMonth = source.DayOfMonth,
                    MonthOfYear = source.MonthOfYear,
                    Duration = source.Duration
                };
        }

        public static Recurrence Disabled(Recurrence source)
        {
            return new Recurrence
                {
                    IsEnabled = false,
                    Type = source.Type,
                    Interval = source.Interval,
                    TimeOfDay = source.TimeOfDay,
                    DaysOfWeek = source.DaysOfWeek,
                    DayOfMonth = source.DayOfMonth,
                    MonthOfYear = source.MonthOfYear,
                    Duration = source.Duration
                };
        }

        public static Recurrence Hourly(int hours, TimeSpan duration = default(TimeSpan))
        {
            if (duration == default(TimeSpan)) duration = new TimeSpan(1, 0, 0);
            if (duration > TimeSpan.FromHours(hours)) throw new ArgumentOutOfRangeException("duration");

            return new Recurrence
                {
                    IsEnabled = true,
                    Type = RecurrenceType.Hourly,
                    Interval = hours,
                    Duration = duration
                };
        }

        public static Recurrence Daily(int days, TimeSpan timeOfDay = default(TimeSpan),
                                       TimeSpan duration = default(TimeSpan),
                                       int dayOfMonthToStart = 0,
                                       int monthOfYearToStart = 0)
        {
            if (timeOfDay == default(TimeSpan)) timeOfDay = new TimeSpan(0, 0, 0);
            if (duration == default(TimeSpan)) duration = new TimeSpan(1, 0, 0);
            if (duration > TimeSpan.FromDays(days)) throw new ArgumentOutOfRangeException("duration");

            if (dayOfMonthToStart == 0) dayOfMonthToStart = DateTime.Today.Day;
            if (monthOfYearToStart == 0) monthOfYearToStart = DateTime.Today.Month;
            if (dayOfMonthToStart < 1 || dayOfMonthToStart > 31)
                throw new ArgumentOutOfRangeException("dayOfMonthToStart");
            if (monthOfYearToStart < 1 || monthOfYearToStart > 12)
                throw new ArgumentOutOfRangeException("monthOfYearToStart");

            return new Recurrence
                {
                    IsEnabled = true,
                    Type = RecurrenceType.Daily,
                    Interval = days,
                    TimeOfDay = timeOfDay,
                    DayOfMonth = dayOfMonthToStart,
                    MonthOfYear = monthOfYearToStart,
                    Duration = duration
                };
        }

        public static Recurrence Weekly(int weeks, DayOfWeek[] daysOfWeek, TimeSpan timeOfDay = default(TimeSpan),
                                        TimeSpan duration = default(TimeSpan),
                                        int dayOfMonthToStart = 0,
                                        int monthOfYearToStart = 0)
        {
            if (timeOfDay == default(TimeSpan)) timeOfDay = new TimeSpan(0, 0, 0);
            if (duration == default(TimeSpan)) duration = new TimeSpan(1, 0, 0);
            if (duration > TimeSpan.FromDays(weeks*7)) throw new ArgumentOutOfRangeException("duration");

            if (dayOfMonthToStart == 0) dayOfMonthToStart = DateTime.Today.Day;
            if (monthOfYearToStart == 0) monthOfYearToStart = DateTime.Today.Month;
            if (dayOfMonthToStart < 1 || dayOfMonthToStart > 31)
                throw new ArgumentOutOfRangeException("dayOfMonthToStart");
            if (monthOfYearToStart < 1 || monthOfYearToStart > 12)
                throw new ArgumentOutOfRangeException("monthOfYearToStart");
            if (daysOfWeek == null || !daysOfWeek.Any()) daysOfWeek = new[] {DateTime.Today.DayOfWeek};

            return new Recurrence
                {
                    IsEnabled = true,
                    Type = RecurrenceType.Weekly,
                    Interval = weeks,
                    Duration = duration,
                    DaysOfWeek = daysOfWeek,
                    TimeOfDay = timeOfDay,
                    DayOfMonth = dayOfMonthToStart,
                    MonthOfYear = monthOfYearToStart
                };
        }

        public static Recurrence Monthly(int months, int dayOfMonth, TimeSpan timeOfDay = default(TimeSpan),
                                         TimeSpan duration = default(TimeSpan), int monthOfYearToStart = 0)
        {
            if (timeOfDay == default(TimeSpan)) timeOfDay = new TimeSpan(0, 0, 0);
            if (duration == default(TimeSpan)) duration = new TimeSpan(1, 0, 0);
            if (duration > TimeSpan.FromDays(30*months)) throw new ArgumentOutOfRangeException("duration");

            if (dayOfMonth < 1 || dayOfMonth > 31)
                throw new ArgumentOutOfRangeException("dayOfMonth");

            if (monthOfYearToStart == 0) monthOfYearToStart = DateTime.Today.Month;
            if (monthOfYearToStart < 1 || monthOfYearToStart > 12)
                throw new ArgumentOutOfRangeException("monthOfYearToStart");

            return new Recurrence
                {
                    IsEnabled = true,
                    Type = RecurrenceType.Monthly,
                    Interval = months,
                    DayOfMonth = dayOfMonth,
                    TimeOfDay = timeOfDay,
                    MonthOfYear = monthOfYearToStart,
                    Duration = duration
                };
        }

        public static Recurrence Yearly(int years, int monthOfYear, int dayOfMonth = 1,
                                        TimeSpan timeOfDay = default(TimeSpan), TimeSpan duration = default(TimeSpan))
        {
            if (timeOfDay == default(TimeSpan)) timeOfDay = new TimeSpan(0, 0, 0);
            if (duration == default(TimeSpan)) duration = new TimeSpan(1, 0, 0);
            if (duration > TimeSpan.FromDays(365*years)) throw new ArgumentOutOfRangeException("duration");

            if (dayOfMonth < 1 || dayOfMonth > 31)
                throw new ArgumentOutOfRangeException("dayOfMonth");
            if (monthOfYear < 1 || monthOfYear > 12)
                throw new ArgumentOutOfRangeException("monthOfYear");

            return new Recurrence
                {
                    IsEnabled = true,
                    Type = RecurrenceType.Yearly,
                    Interval = years,
                    MonthOfYear = monthOfYear,
                    DayOfMonth = dayOfMonth,
                    TimeOfDay = timeOfDay,
                    Duration = duration
                };
        }

        public bool Equals(Recurrence other)
        {
            var thisDaysOfWeek = this.DaysOfWeek ?? new DayOfWeek[0];
            var otherDaysOfWeek = other.DaysOfWeek ?? new DayOfWeek[0];

            return this.IsEnabled == other.IsEnabled &&
                   this.Type == other.Type &&
                   this.Interval == other.Interval &&
                   this.TimeOfDay == other.TimeOfDay &&
                   (thisDaysOfWeek.SequenceEqual(otherDaysOfWeek)) &&
                   this.DayOfMonth == other.DayOfMonth &&
                   this.MonthOfYear == other.MonthOfYear &&
                   this.Duration == other.Duration;
        }

        public override bool Equals(object obj)
        {
            return obj is Recurrence && this.Equals(obj as Recurrence);
        }

        public override int GetHashCode()
        {
            return (new object[]
                {
                    this.Type,
                    this.Interval,
                    this.TimeOfDay,
                    this.DaysOfWeek,
                    this.DayOfMonth,
                    this.MonthOfYear,
                    this.Duration
                }).GetHashCode();
        }
    }
}