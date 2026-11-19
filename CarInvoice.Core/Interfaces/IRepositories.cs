using CarInvoice.Core.Entities;

namespace CarInvoice.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> FindByChassisNumberAsync(string chassisNumber, CancellationToken ct = default);
    Task<Vehicle?> FindByNormalizedAsync(string fullChassisNormalized, CancellationToken ct = default);
    Task<List<Vehicle>> SearchAsync(string query, CancellationToken ct = default);
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> FindByNameOrPhoneAsync(string nameOrPhone, CancellationToken ct = default);
    Task<List<Customer>> SearchAsync(string query, CancellationToken ct = default);
}

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByNumberAsync(string invoiceNumber, CancellationToken ct = default);
    Task<List<Invoice>> SearchAsync(string query, CancellationToken ct = default);
    Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct = default);
}

public interface IChassisLookupRepository : IRepository<ChassisCodeLookup>
{
    Task<ChassisCodeLookup?> FindByCodeAsync(string chassisCode, CancellationToken ct = default);
    Task<List<ChassisCodeLookup>> FindAllByCodeAsync(string chassisCode, CancellationToken ct = default);
    Task<ChassisCodeLookup?> FindExactAsync(string chassisCode, string make, string model, string? serialPrefixOrRange = null, CancellationToken ct = default);
}

public interface IGlossaryRepository : IRepository<JapaneseTermGlossary>
{
    Task<JapaneseTermGlossary?> FindByJapaneseTermAsync(string term, CancellationToken ct = default);
    Task<List<JapaneseTermGlossary>> GetAllOrderedAsync(CancellationToken ct = default);
}

public interface IBankProfileRepository : IRepository<BankProfile>
{
    Task<BankProfile?> GetDefaultAsync(CancellationToken ct = default);
    Task<List<BankProfile>> GetAllOrderedAsync(CancellationToken ct = default);
}

public interface IUnitOfWork : IAsyncDisposable
{
    IVehicleRepository Vehicles { get; }
    ICustomerRepository Customers { get; }
    IInvoiceRepository Invoices { get; }
    IChassisLookupRepository ChassisLookups { get; }
    IGlossaryRepository Glossary { get; }
    IBankProfileRepository BankProfiles { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
