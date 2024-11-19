
using DalApi;
namespace Dal
{
    sealed public class DalList : IDal

    {
        public IVolunteer Volunteer { get; } = new VolunteerImplementation();

        public ICall call { get; } = new CallImplementation();

        public IAssignment assignment { get; } = new AssignmentImplementation();

        public IConfig config { get; } = new ConfigImplementation();

        public void ResetDB()
        {
            Volunteer.DeleteAll();
            call.DeleteAll();
            assignment.DeleteAll();
            config.Reset();

        }


    }
}



  

