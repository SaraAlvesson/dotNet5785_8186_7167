
namespace Dal;
using DalApi;
using DO;

public class VolunteerImplementation : IVolunteer
{
    public void Create(Volunteer item)
    {
        if(Read(item.Id)==null)//if didn't find an the list of volunteers the id
            DataSource.Volunteers.Add(item);//add new item to the list of volunteers
        else//found
           throw new Exception($"Volunteer with ID={item.Id} already exists");//throw exception
    }

    public void Delete(int id)
    {
        Volunteer? ToDelete=Read(id);//ToDelete gets id to remove
        if(ToDelete!=null)//checks if the id exists
        { 
            DataSource.Volunteers.Remove(ToDelete);//delete the id
        }
        else//id does not exist
        {
            throw new Exception($"Object with Id {id} not found");//throw exception
        }
       
    }

    public void DeleteAll()
    {
        DataSource.Volunteers.Clear();//delete all items from lists
    }

    public Volunteer? Read(int id)
    {
        return DataSource.Volunteers.Find(obj => obj.Id == id);// return if found id in the list else returns null
    }

    public List<Volunteer> ReadAll()
    {
        return new List<Volunteer>(DataSource.Volunteers);//copies list from datasource and returns it
    }

    public void Update(Volunteer item)
    {

        Delete(item.Id); //if item exists then delete (if not delete will throw an exception)   
        Create(item);//add item to list
    }

  
}
