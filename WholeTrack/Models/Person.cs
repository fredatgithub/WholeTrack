using System;
using System.Runtime.Serialization;

namespace WholeTrack.Models
{
  [DataContract]
  public class Person
  {
    [DataMember]
    public string FirstName { get; set; }

    [DataMember]
    public string LastName { get; set; }

    [DataMember]
    public UniversalDateTime BirthDate { get; set; } = UniversalDateTime.Unknown;

    [DataMember]
    public bool IsDead { get; set; }

    [DataMember]
    public UniversalDateTime DeathDate { get; set; } = UniversalDateTime.Unknown;

    [DataMember]
    public string Occupation { get; set; }
  }
}
