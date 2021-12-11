using System.Collections.Generic;

namespace SqlCleanup {
    public record Customer(int Id, string FirstName, string LastName);
    public record Product(int Id, string Name, decimal Price);
    public record Cart(int Id, int CustomerId, ICollection<int> Products);
}
