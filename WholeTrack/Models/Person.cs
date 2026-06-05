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
    public DateTime BirthDate { get; set; }

    [DataMember]
    public bool IsDead { get; set; }

    [DataMember]
    public DateTime? DeathDate { get; set; }

    [DataMember]
    public string Occupation { get; set; }
  }
}
