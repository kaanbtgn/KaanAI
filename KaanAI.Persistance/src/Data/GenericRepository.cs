using KaanAI.Domain;
using KaanAI.Domain.Repositories;
using KaanAI.Persistance.Context.Main;
using Microsoft.EntityFrameworkCore;

namespace KaanAI.Persistance.Data;
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly MainDbContext _context;

    public GenericRepository(MainDbContext context)
    {
        _context = context;
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await  _context.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}