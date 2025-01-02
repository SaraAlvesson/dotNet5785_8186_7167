



namespace PL;

class Enums
{

    internal class VolunteerCollection : IEnumerable
    {
        static readonly IEnumerable<BO.Enums.VolunteerInListField> s_enums =
    (Enum.GetValues(typeof(BO.Enums.VolunteerInListField)) as IEnumerable<BO.Enums.VolunteerInListField>)!;

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }

}
