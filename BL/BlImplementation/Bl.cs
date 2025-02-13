

namespace BlImplementation;
using BlApi;
internal class Bl : IBl
{
    public IVolunteer Volunteer { get; } = new VolunteerImplementation();

    public IAdmin Admin { get; } = new AdminImplementation();

    public ICall Call { get; } = new CallImplementation();
}
