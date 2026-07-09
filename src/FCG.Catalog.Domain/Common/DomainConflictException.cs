namespace FCG.Catalog.Domain.Common;

public sealed class DomainConflictException(string message) : Exception(message);
