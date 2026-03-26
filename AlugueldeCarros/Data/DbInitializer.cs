using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AlugueldeCarros.Loaders;

namespace AlugueldeCarros.Data;

/// <summary>
/// Inicializador automático do banco de dados SQLite.
/// Executa na startup:
/// - Detecta e corrige schema inválido (shadow FKs)
/// - Cria arquivo .db se não existir
/// - Cria schema (tabelas) se necessário
/// - Carrega seed dos JSONs se base está vazia
/// - Garante idempotência
/// </summary>
public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly IJsonDataLoader _jsonDataLoader;
    private readonly ILogger<DbInitializer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DbInitializer(
        AppDbContext context,
        IJsonDataLoader jsonDataLoader,
        ILogger<DbInitializer> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _jsonDataLoader = jsonDataLoader;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Executa o bootstrap automático do banco.
    /// Idempotente: pode rodar múltiplas vezes sem problemas.
    /// Detecta e corrige schema inválido herdado de mapeamento anterior.
    /// </summary>
    public async Task ExecuteAsync()
    {
        // Skip initialization for test environments (in-memory databases)
        if (_serviceProvider.GetService(typeof(ITestEnvironmentMarker)) != null)
        {
            _logger.LogInformation("[DbInitializer] Test environment detected. Skipping initialization.");
            return;
        }

        _logger.LogInformation("[DbInitializer] Iniciando...");

        try
        {
            // 0. VALIDAÇÃO PRÉ-INICIALIZAÇÃO: Detectar schema inválido
            _logger.LogInformation("[DbInitializer] Validando schema...");
            var schemaIsValid = await ValidateSchemaAsync();

            if (!schemaIsValid)
            {
                _logger.LogWarning("[DbInitializer] ⚠️ Schema inválido detectado (shadow FKs legados). Recriando banco...");
                await RecreateDatabase();
                _logger.LogInformation("[DbInitializer] Banco recriado com sucesso.");
            }
            else
            {
                _logger.LogInformation("[DbInitializer] Schema válido. Prosseguindo...");
            }

            // 1. Garantir que arquivo .db existe e schema é criado
            _logger.LogInformation("[DbInitializer] Executando EnsureCreatedAsync()...");
            var created = await _context.Database.EnsureCreatedAsync();

            if (created)
            {
                _logger.LogInformation("[DbInitializer] Banco de dados criado com sucesso.");
            }
            else
            {
                _logger.LogInformation("[DbInitializer] Banco de dados já existia.");
            }

            // 2. Verificar se base tem dados
            if (HasData())
            {
                _logger.LogInformation("[DbInitializer] Base já contém dados. Seed não será executado.");
                return;
            }

            // 3. Se vazio, fazer seed a partir dos JSONs
            _logger.LogInformation("[DbInitializer] Base está vazia. Iniciando seed automático...");
            await SeedAsync();
            _logger.LogInformation("[DbInitializer] Seed concluído com sucesso! ✅");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DbInitializer] ❌ Erro durante inicialização!");
            throw;
        }
    }

    /// <summary>
    /// Valida se o schema do banco é compatível com o modelo EF atual.
    /// Detecta colunas indevidas como (CategoryId1, UserId1, etc.) que indicam
    /// mapeamento EF anterior incorreto com shadow FKs.
    /// Retorna true se schema é válido, false se precisa recriação.
    /// </summary>
    private async Task<bool> ValidateSchemaAsync()
    {
        try
        {
            // Se banco não existe, schema é válido (será criado do zero)
            if (!await _context.Database.CanConnectAsync())
            {
                _logger.LogInformation("[DbInitializer.ValidateSchema] Banco não existe. Schema será criado.");
                return true;
            }

            // Verificar existência de tabelas principais
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
                using (var command = connection.CreateCommand())
                {
                    // Consulta SQLite para verificar colunas da tabela PricingRules
                    // Se CategoryId1 existir, schema é legado inválido
                    command.CommandText = "PRAGMA table_info(PricingRules);";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var columns = new List<string>();
                        while (await reader.ReadAsync())
                        {
                            var columnName = reader.GetString(1); // Coluna 1 = nome da coluna
                            columns.Add(columnName);
                        }

                        // Checklist de shadow FKs indevidas que indicam schema legado
                        var invalidShadowFKs = new[] { "CategoryId1", "UserId1", "RoleId1", "BranchId1", "ReservationId1", "VehicleId1" };
                        var hasInvalidFK = columns.Any(c => invalidShadowFKs.Contains(c));

                        if (hasInvalidFK)
                        {
                            _logger.LogWarning($"[DbInitializer.ValidateSchema] Shadow FK detectado: {string.Join(", ", columns.Where(c => invalidShadowFKs.Contains(c)))}");
                            return false; // Schema inválido, precisa recriação
                        }

                        _logger.LogInformation($"[DbInitializer.ValidateSchema] Colunas PricingRules válidas: {string.Join(", ", columns)}");
                    }
                }

                return true; // Schema válido
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[DbInitializer.ValidateSchema] Erro ao validar schema. Assumindo inválido e recriando.");
            return false; // Em caso de erro, recria por segurança
        }
    }

    /// <summary>
    /// Deleta o arquivo .db físico para permitir recriação com schema correto.
    /// Executa apenas em ambiente de desenvolvimento (SQLite local).
    /// </summary>
    private async Task RecreateDatabase()
    {
        try
        {
            // Obter caminho do arquivo .db a partir da connection string
            var connection = _context.Database.GetDbConnection() as Microsoft.Data.Sqlite.SqliteConnection;
            var dbPath = connection?.DataSource;

            if (string.IsNullOrEmpty(dbPath))
            {
                _logger.LogWarning("[DbInitializer.RecreateDatabase] Caminho do banco não identificado. Tentando delete via EF.");
                var deleted = await _context.Database.EnsureDeletedAsync();
                _logger.LogInformation($"[DbInitializer.RecreateDatabase] EnsureDeletedAsync retornou: {deleted}");
                return;
            }

            // Caminho relativo ou absoluto do arquivo
            var fullPath = Path.IsPathRooted(dbPath) ? dbPath : Path.Combine(Directory.GetCurrentDirectory(), dbPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation($"[DbInitializer.RecreateDatabase] Banco deletado: {fullPath}");
            }
            else
            {
                _logger.LogInformation($"[DbInitializer.RecreateDatabase] Arquivo .db não encontrado em: {fullPath}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DbInitializer.RecreateDatabase] Erro ao deletar banco. Tentando EnsureDeleted...");
            await _context.Database.EnsureDeletedAsync();
        }
    }

    /// <summary>
    /// Verifica se a base já tem dados.
    /// Idempotente: retorna true se qualquer tabela tem registros.
    /// </summary>
    private bool HasData()
    {
        return _context.Users.Any()
            || _context.Roles.Any()
            || _context.Branches.Any()
            || _context.VehicleCategories.Any()
            || _context.Vehicles.Any()
            || _context.PricingRules.Any()
            || _context.Reservations.Any()
            || _context.Payments.Any()
            || _context.CustomerProfiles.Any();
    }

    /// <summary>
    /// Carrega todas as entidades a partir dos JSONs e insere no banco.
    /// Usa transação implícita via SaveChangesAsync para garantir atomicidade.
    /// </summary>
    private async Task SeedAsync()
    {
        // Skip JSON loading for in-memory test databases - factory will seed test data
        var isInMemoryDb = _context.Database.ProviderName?.Contains("InMemory") ?? false;
        
        if (isInMemoryDb)
        {
            _logger.LogInformation("[DbInitializer] In-memory database detected. Skipping JSON seed (factory will handle test data).");
            return;
        }

        _logger.LogInformation("[DbInitializer] Carregando Roles...");
        var roles = _jsonDataLoader.LoadRoles();
        _context.Roles.AddRange(roles);

        _logger.LogInformation("[DbInitializer] Carregando Users...");
        var users = _jsonDataLoader.LoadUsers();
        _context.Users.AddRange(users);

        _logger.LogInformation("[DbInitializer] Carregando UserRoles...");
        var userRoles = _jsonDataLoader.LoadUserRoles();
        _context.UserRoles.AddRange(userRoles);

        _logger.LogInformation("[DbInitializer] Carregando Branches...");
        var branches = _jsonDataLoader.LoadBranches();
        _context.Branches.AddRange(branches);

        _logger.LogInformation("[DbInitializer] Carregando VehicleCategories...");
        var categories = _jsonDataLoader.LoadVehicleCategories();
        _context.VehicleCategories.AddRange(categories);

        _logger.LogInformation("[DbInitializer] Carregando Vehicles...");
        var vehicles = _jsonDataLoader.LoadVehicles();
        _context.Vehicles.AddRange(vehicles);

        _logger.LogInformation("[DbInitializer] Carregando PricingRules...");
        var pricingRules = _jsonDataLoader.LoadPricingRules();
        _context.PricingRules.AddRange(pricingRules);

        _logger.LogInformation("[DbInitializer] Carregando Reservations...");
        var reservations = _jsonDataLoader.LoadReservations();
        _context.Reservations.AddRange(reservations);

        _logger.LogInformation("[DbInitializer] Carregando Payments...");
        var payments = _jsonDataLoader.LoadPayments();
        _context.Payments.AddRange(payments);

        _logger.LogInformation("[DbInitializer] Carregando CustomerProfiles...");
        var customerProfiles = _jsonDataLoader.LoadCustomerProfiles();
        _context.CustomerProfiles.AddRange(customerProfiles);

        _logger.LogInformation("[DbInitializer] Salvando mudanças no banco...");
        await _context.SaveChangesAsync();

        _logger.LogInformation("[DbInitializer] Seed concluído com sucesso!");
    }
}
