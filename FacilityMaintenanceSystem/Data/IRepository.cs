namespace FacilityMaintenanceSystem.Data;

/// <summary>
/// Generic repository abstraction decoupling business logic from physical storage.
/// </summary>
public interface IRepository<T>
{
    List<T> GetAll();
    T? GetById(string id);
    void Add(T entity);
    void Update(T entity);
    void SaveChanges();
}
