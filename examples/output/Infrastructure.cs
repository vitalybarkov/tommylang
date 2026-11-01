namespace Tommy.Payments.Infrastructure;

public interface IPaymentRepository
{
    Task InsertAsync(int orderId, double amount, string email, CancellationToken ct = default);
}

public class PaymentRepository : IPaymentRepository
{
    private readonly NpgsqlConnection _conn;
    public PaymentRepository(NpgsqlConnection conn) => _conn = conn;

    // Parameterized SQL — no injection
    public async Task InsertAsync(int orderId, double amount, string email, CancellationToken ct = default)
    {
        const string sql = "INSERT INTO payments (order_id, amount, email) VALUES (@id, @amount, @email)";
        await using var cmd = new NpgsqlCommand(sql, _conn);
        cmd.Parameters.AddWithValue("@id", orderId);
        cmd.Parameters.AddWithValue("@amount", amount);
        cmd.Parameters.AddWithValue("@email", email);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}

public interface ICacheService
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default);
    Task<bool> SetNxAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    public RedisCacheService(IConnectionMultiplexer redis) => _redis = redis;
    private IDatabase Db => _redis.GetDatabase();

    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
        => await Db.StringGetAsync(key);

    public async Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default)
        => await Db.StringSetAsync(key, value, ttl);

    // Idempotency primitive: SET NX (only if not exists)
    public async Task<bool> SetNxAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default)
        => await Db.StringSetAsync(key, value, ttl, When.NotExists);

    public async Task RemoveAsync(string key, CancellationToken ct = default)
        => await Db.KeyDeleteAsync(key);
}

// Outbox event record
public record PaymentCreatedEvent(int OrderId, string EventType = "payment_created");