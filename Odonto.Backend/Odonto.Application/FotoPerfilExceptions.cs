namespace Odonto.Application;

public sealed class FotoPerfilInvalidaException(string message) : Exception(message);

public sealed class FotoPerfilMuitoGrandeException(string message) : Exception(message);
