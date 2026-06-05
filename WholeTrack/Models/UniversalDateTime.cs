using System;
using System.Runtime.Serialization;

namespace WholeTrack.Models
{
  [DataContract]
  public class UniversalDateTime : IComparable<UniversalDateTime>
  {
    [DataMember]
    public bool IsUnknown { get; set; }

    [DataMember]
    public bool IsBC { get; set; }

    [DataMember]
    public int? Year { get; set; }

    [DataMember]
    public int? Month { get; set; }

    [DataMember]
    public int? Day { get; set; }

    public static UniversalDateTime Unknown => new UniversalDateTime { IsUnknown = true };

    public static UniversalDateTime FromDateTime(DateTime dateTime, bool isBC = false)
    {
      return new UniversalDateTime
      {
        IsUnknown = false,
        IsBC = isBC,
        Year = dateTime.Year,
        Month = dateTime.Month,
        Day = dateTime.Day
      };
    }

    public static UniversalDateTime FromYear(int year, bool isBC = false)
    {
      return new UniversalDateTime
      {
        IsUnknown = false,
        IsBC = isBC,
        Year = Math.Abs(year),
        Month = 1,
        Day = 1
      };
    }

    public int SortYear
    {
      get
      {
        if (IsUnknown || !Year.HasValue)
          return 0;

        return IsBC ? -Year.Value : Year.Value;
      }
    }

    public DateTime? ToDateTime()
    {
      if (IsUnknown || IsBC || !Year.HasValue)
        return null;

      return new DateTime(Year.Value, Month ?? 1, Day ?? 1);
    }

    public override string ToString()
    {
      if (IsUnknown || !Year.HasValue)
        return "Inconnue";

      var day = Day ?? 1;
      var month = Month ?? 1;
      var year = Year.Value;
      var prefix = IsBC ? " av. J.-C." : string.Empty;
      return $"{day:00}/{month:00}/{year:0000}{prefix}";
    }

    public int CompareTo(UniversalDateTime other)
    {
      if (other == null)
        return 1;

      if (IsUnknown && other.IsUnknown)
        return 0;
      if (IsUnknown)
        return 1;
      if (other.IsUnknown)
        return -1;

      var yearComparison = SortYear.CompareTo(other.SortYear);
      if (yearComparison != 0)
        return yearComparison;

      var monthComparison = (Month ?? 1).CompareTo(other.Month ?? 1);
      if (monthComparison != 0)
        return monthComparison;

      return (Day ?? 1).CompareTo(other.Day ?? 1);
    }
  }
}
