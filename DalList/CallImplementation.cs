namespace Dal;
using DalApi;
using DO;

internal class CallImplementation : ICall
{
    public void Create(Call item)
    {
        Call newItem = item with { Id = Config.FuncNextCallId };//creates new item with the next id 
        DataSource.Calls.Add(newItem);//adds new item to list
    }
    public Call? Read(int id)
    {
        return DataSource.Calls.Find(obj => obj.Id == id);// return if found id in the list else returns null
    }
    public List<Call> ReadAll()
    {
        return new List<Call>(DataSource.Calls);//copies list from datasource and returns it
    }
    public void Update(Call item)
    {

        Delete(item.Id); //if item exists then delete (if not delete will throw an exception)   
        Create(item);//add item to list
    }
    public void Delete(int id)
    {
        Call? ToDelete = Read(id);//ToDelete gets id to remove
        if (ToDelete != null)//checks if the id exists
        {
            DataSource.Calls.Remove(ToDelete);// delete the id
        }
        else//id does not exist
        {
            throw new Exception($"Object with Id {id} not found");//throw exception
        }
    }
    public void DeleteAll()
    {
        DataSource.Calls.Clear();//delete all items from lists
        
    }

}
