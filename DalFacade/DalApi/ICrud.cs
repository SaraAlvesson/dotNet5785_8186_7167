
namespace DalApi
    
{/// <summary>
/// Generic interface for CRUD operations in the Data Access Layer.
/// </summary>
/// <typeparam name="T"> The type of entity managed by the CRUD operations.</typeparam>
    public interface ICrud<T>where T : class
    {
        void Create(T item); //Creates new entity object in DAL
        T? Read(int id); //Reads entity object by its ID 
        void DeleteAll(); //stage 1  Deletes all entity objects
        void Update(T item); //Updates entity object
        void Delete(int id); //Deletes an object by its Id
        IEnumerable<T>ReadAll (Func<T,bool >?filter=null); //Delete all entity objects
        T? Read(Func<T, bool> filter); // stage 2


    }
}
