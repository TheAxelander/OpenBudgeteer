namespace OpenBudgeteer.Core.Data;

public static class ConfigurationKeyConstants
{
    public const string CONNECTION_SERVER = "CONNECTION_SERVER";
    public const string CONNECTION_PORT = "CONNECTION_PORT";
    public const string CONNECTION_DATABASE = "CONNECTION_DATABASE";
    public const string CONNECTION_USER = "CONNECTION_USER";
    public const string CONNECTION_PASSWORD = "CONNECTION_PASSWORD";
    public const string CONNECTION_ROOT_PASSWORD = "CONNECTION_ROOT_PASSWORD";
    public const string CONNECTION_PROVIDER = "CONNECTION_PROVIDER";
    
    public const string CONNECTION_REDIS_SERVER = "CONNECTION_REDIS_SERVER";
    public const string CONNECTION_REDIS_PORT = "CONNECTION_REDIS_PORT";
    public const string CONNECTION_REDIS_USER = "CONNECTION_REDIS_USER";
    public const string CONNECTION_REDIS_PASSWORD = "CONNECTION_REDIS_PASSWORD";
    public const string CONNECTION_REDIS_PREFIX = "CONNECTION_REDIS_PREFIX";

    public const string APPSETTINGS_CULTURE = "APPSETTINGS_CULTURE";
    public const string APPSETTINGS_THEME = "APPSETTINGS_CULTURE";

    public const string PROVIDER_MYSQL = "MYSQL";
    public const string PROVIDER_MARIADB = "MARIADB";
    public const string PROVIDER_POSTGRES = "POSTGRES";
    public const string PROVIDER_POSTGRESQL = "POSTGRESQL";

    public const string LOGLEVEL_DEFAULT_JSON = "Logging:LogLevel:Default";
    public const string LOGLEVEL_DEFAULT_ENV = "Logging__LogLevel__Default";
}