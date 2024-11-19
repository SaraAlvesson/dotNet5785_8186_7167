
namespace Dal;
using DalApi;
using DO;

internal class AssignmentImplementation : IAssignment
{
    public void Create(Assignment item)
    {
        Assignment newItem = item with { Id = Config.FuncNextAssignmentId };//creates new item with the next id 
        DataSource.Assignments.Add(newItem);//adds new item to list
    }
    public Assignment? Read(int id)
    {
        return DataSource.Assignments.Find(obj=>obj.Id== id);// return if found id in the list else returns null
    }
    public List<Assignment> ReadAll()
    {
        return new List<Assignment>(DataSource.Assignments);//copies list from datasource and returns it
    }
    public void Update(Assignment item)
    {
        Delete(item.Id); //if item exists then delete (if not delete will throw an exception)   
        Create(item);//add item to list
    }
    public void Delete(int id)
    {
        Assignment? ToDelete = Read(id);//ToDelete gets id to remove
        if (ToDelete != null)//checks if the id exists
        {
            DataSource.Assignments.Remove(ToDelete);// delete the id
        }
        else//id does not exist
        {
            throw new Exception($"Object with Id {id} not found");//throw exception
        }
    }
    public void DeleteAll()
    {
        DataSource.Assignments.Clear();//delete all items from lists
    }

}

