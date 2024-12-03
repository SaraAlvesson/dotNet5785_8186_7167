
using DalApi;

namespace Dal;

public class DalXml : IDal
{
    public IVolunteer Volunteer => throw new NotImplementedException();

    public ICall call => throw new NotImplementedException();

    public IAssignment assignment => throw new NotImplementedException();

    public IConfig config => throw new NotImplementedException();

    public void ResetDB()
    {
        throw new NotImplementedException();
    }
}
