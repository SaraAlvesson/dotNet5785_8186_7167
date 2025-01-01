

namespace PL;

class Enums
{
    
internal class VolunteerCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.VolunteerInListField.> s_enums =
(Enum.GetValues(typeof(BO.Volunteer)) as IEnumerable<BO.Volunteer>)!;

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

}
