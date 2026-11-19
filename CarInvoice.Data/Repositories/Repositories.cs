using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarInvoice.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;

    public Repository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => Set.FindAsync(new object[] { id }, ct).AsTask();

    public virtual Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => Set.AsNoTracking().ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);
}

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(AppDbContext db) : base(db) { }

    public Task<Vehicle?> FindByChassisNumberAsync(string chassisNumber, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(v => v.ChassisNumber == chassisNumber, ct);

    public Task<Vehicle?> FindByNormalizedAsync(string fullChassisNormalized, CancellationToken ct = default)
        => Set.AsNoTracking().FirstOrDefaultAsync(v => v.FullChassisNormalized == fullChassisNormalized, ct);

    public Task<List<Vehicle>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Set.AsNoTracking().OrderByDescending(v => v.UpdatedAt).Take(500).ToListAsync(ct);

        var q = query.Trim().ToUpperInvariant();
        return Set.AsNoTracking()
            .Where(v =>
                v.ChassisNumber.ToUpper().Contains(q) ||
                v.ChassisCode.ToUpper().Contains(q) ||
                v.FullChassisNormalized.Contains(q) ||
                v.Make.ToUpper().Contains(q) ||
                v.Model.ToUpper().Contains(q))
            .OrderByDescending(v => v.UpdatedAt)
            .Take(500)
            .ToListAsync(ct);
    }
}

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext db) : base(db) { }

    public Task<Customer?> FindByNameOrPhoneAsync(string nameOrPhone, CancellationToken ct = default)
    {
        var q = nameOrPhone.Trim();
        return Set.FirstOrDefaultAsync(c => c.FullName == q || c.Phone == q, ct);
    }

    public Task<List<Customer>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Set.AsNoTracking().OrderBy(c => c.FullName).Take(500).ToListAsync(ct);

        var q = query.Trim().ToUpperInvariant();
        return Set.AsNoTracking()
            .Where(c =>
                c.FullName.ToUpper().Contains(q) ||
                (c.CompanyName != null && c.CompanyName.ToUpper().Contains(q)) ||
                c.Phone.Contains(q) ||
                c.Email.ToUpper().Contains(q))
            .OrderBy(c => c.FullName)
            .Take(500)
            .ToListAsync(ct);
    }
}

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(AppDbContext db) : base(db) { }

    public override Task<List<Invoice>> GetAllAsync(CancellationToken ct = default)
        => Set.AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Vehicle)
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.Id)
            .ToListAsync(ct);

    public Task<Invoice?> GetByNumberAsync(string invoiceNumber, CancellationToken ct = default)
        => Set.AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Vehicle)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, ct);

    public Task<List<Invoice>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAllAsync(ct);

        var q = query.Trim().ToUpperInvariant();
        return Set.AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Vehicle)
            .Where(i =>
                i.InvoiceNumber.ToUpper().Contains(q) ||
                (i.Customer != null && i.Customer.FullName.ToUpper().Contains(q)) ||
                (i.Vehicle != null && (
                    i.Vehicle.ChassisNumber.ToUpper().Contains(q) ||
                    i.Vehicle.ChassisCode.ToUpper().Contains(q) ||
                    i.Vehicle.Make.ToUpper().Contains(q) ||
                    i.Vehicle.Model.ToUpper().Contains(q))))
            .OrderByDescending(i => i.InvoiceDate)
            .Take(500)
            .ToListAsync(ct);
    }

    public async Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.Today.Year;
        var prefix = $"INV-{year}-";
        var last = await Set.AsNoTracking()
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        var next = 1;
        if (last is not null)
        {
            var parts = last.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var n))
                next = n + 1;
        }

        return $"{prefix}{next:D4}";
    }
}

public class ChassisLookupRepository : Repository<ChassisCodeLookup>, IChassisLookupRepository
{
    public ChassisLookupRepository(AppDbContext db) : base(db) { }

    public Task<ChassisCodeLookup?> FindByCodeAsync(string chassisCode, CancellationToken ct = default)
    {
        var code = chassisCode.Trim().ToUpperInvariant();
        return Set.AsNoTracking().FirstOrDefaultAsync(c => c.ChassisCode == code, ct);
    }

    public Task<List<ChassisCodeLookup>> FindAllByCodeAsync(string chassisCode, CancellationToken ct = default)
    {
        var code = chassisCode.Trim().ToUpperInvariant();
        return Set.AsNoTracking()
            .Where(c => c.ChassisCode == code)
            .OrderBy(c => c.DefaultMake)
            .ThenBy(c => c.DefaultModel)
            .ToListAsync(ct);
    }

    public async Task<ChassisCodeLookup?> FindExactAsync(
        string chassisCode,
        string make,
        string model,
        string? serialPrefixOrRange = null,
        CancellationToken ct = default)
    {
        var code = chassisCode.Trim().ToUpperInvariant();
        var mk = make.Trim();
        var md = model.Trim();
        var serial = string.IsNullOrWhiteSpace(serialPrefixOrRange) ? null : serialPrefixOrRange.Trim();
        var rows = await Set.Where(c => c.ChassisCode == code).ToListAsync(ct);
        return rows.FirstOrDefault(c =>
            c.DefaultMake.Equals(mk, StringComparison.OrdinalIgnoreCase) &&
            c.DefaultModel.Equals(md, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.SerialPrefixOrRange, serial, StringComparison.OrdinalIgnoreCase));
    }
}

public class GlossaryRepository : Repository<JapaneseTermGlossary>, IGlossaryRepository
{
    public GlossaryRepository(AppDbContext db) : base(db) { }

    public Task<JapaneseTermGlossary?> FindByJapaneseTermAsync(string term, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(g => g.JapaneseTerm == term, ct);

    public Task<List<JapaneseTermGlossary>> GetAllOrderedAsync(CancellationToken ct = default)
        => Set.AsNoTracking()
            .OrderBy(g => g.Category)
            .ThenByDescending(g => g.JapaneseTerm.Length)
            .ToListAsync(ct);
}

public class BankProfileRepository : Repository<BankProfile>, IBankProfileRepository
{
    public BankProfileRepository(AppDbContext db) : base(db) { }

    public Task<BankProfile?> GetDefaultAsync(CancellationToken ct = default)
        => Set.AsNoTracking()
            .OrderByDescending(b => b.IsDefault)
            .ThenBy(b => b.Id)
            .FirstOrDefaultAsync(ct);

    public Task<List<BankProfile>> GetAllOrderedAsync(CancellationToken ct = default)
        => Set.AsNoTracking()
            .OrderByDescending(b => b.IsDefault)
            .ThenBy(b => b.ProfileName)
            .ToListAsync(ct);
}

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
        Vehicles = new VehicleRepository(db);
        Customers = new CustomerRepository(db);
        Invoices = new InvoiceRepository(db);
        ChassisLookups = new ChassisLookupRepository(db);
        Glossary = new GlossaryRepository(db);
        BankProfiles = new BankProfileRepository(db);
    }

    public IVehicleRepository Vehicles { get; }
    public ICustomerRepository Customers { get; }
    public IInvoiceRepository Invoices { get; }
    public IChassisLookupRepository ChassisLookups { get; }
    public IGlossaryRepository Glossary { get; }
    public IBankProfileRepository BankProfiles { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public ValueTask DisposeAsync() => _db.DisposeAsync();
}
