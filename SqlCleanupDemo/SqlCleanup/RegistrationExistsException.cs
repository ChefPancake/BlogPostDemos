namespace SqlCleanup;

public class RegistrationExistsException : Exception {
    public RegistrationExistsException(Type type)
        : base($"Type {type.Name} already registered") { }
}
