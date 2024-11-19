

namespace DalApi
{
   public interface IDal
    {
       IVolunteer Volunteer { get; }
        ICall call { get; }
        IAssignment assignment { get; }
        IConfig config { get; }
        void ResetDB();
          

    }
}
