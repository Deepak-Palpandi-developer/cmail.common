using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cgmail.Common;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChanges();
}
public abstract class UnitOfWork : IUnitOfWork
{
    protected readonly DbContext Context;

    protected UnitOfWork(DbContext context)
    {
        Context = context;
    }

    public async Task<int> SaveChanges()
    {
        return await Context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Context.Dispose();
    }
}