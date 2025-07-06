using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HealthyCoding_Agentic.Model
{
    public class Customer {

        //[JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        //[JsonPropertyName("last_nname")]
        public string LastName { get; set; }

        //[JsonPropertyName("email")]
        public string Email { get; set; }

        //[JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }

        //[JsonPropertyName("date_registered")]
        public DateTime RegistrationDate { get; set; }

       //[JsonPropertyName("id")]
        public int Id { get; set; }
        public static Customer[] GetDefaultCustomers() => [
            new Customer { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", PhoneNumber = "(324) 344-1234", RegistrationDate = new DateTime(2025, 1, 5) },
            new Customer { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@example.com", PhoneNumber = "(357) 234-5678", RegistrationDate = new DateTime(2025, 2, 10) },
            new Customer { Id = 3, FirstName = "Carol", LastName = "Anderson", Email = "carol.anderson@example.com", PhoneNumber = "(193) 333-8765", RegistrationDate = new DateTime(2025, 3, 12) },
            new Customer { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@example.com", PhoneNumber = "(898) 243-2345", RegistrationDate = new DateTime(2025, 4, 1) },
            new Customer { Id = 5, FirstName = "Emma", LastName = "Davis", Email = "emma.davis@example.com", PhoneNumber = "(454) 375-3456", RegistrationDate = new DateTime(2025, 5, 3) },
            new Customer { Id = 6, FirstName = "Frank", LastName = "Evans", Email = "frank.evans@example.com", PhoneNumber = "(894) 555-4567", RegistrationDate = new DateTime(2025, 6, 15) },
            new Customer { Id = 7, FirstName = "Grace", LastName = "Hall", Email = "grace.hall@example.com", PhoneNumber = "(137) 837-6789", RegistrationDate = new DateTime(2025, 1, 20) },
            new Customer { Id = 8, FirstName = "Henry", LastName = "King", Email = "henry.king@example.com", PhoneNumber = "(368) 112-7890", RegistrationDate = new DateTime(2025, 2, 25) },
            new Customer { Id = 9, FirstName = "Ivy", LastName = "Lewis", Email = "ivy.lewis@example.com", PhoneNumber = "(742) 993-8901", RegistrationDate = new DateTime(2025, 3, 30) },
            new Customer { Id = 10, FirstName = "Jack", LastName = "Martinez", Email = "jack.martinez@example.com", PhoneNumber = "(442) 443-9012", RegistrationDate = new DateTime(2025, 4, 5) },
            new Customer { Id = 11, FirstName = "Karen", LastName = "Nelson", Email = "karen.nelson@example.com", PhoneNumber = "(256) 555-0123", RegistrationDate = new DateTime(2025, 5, 10) },
            new Customer { Id = 12, FirstName = "Liam", LastName = "Owens", Email = "liam.owens@example.com", PhoneNumber = "(334) 632-6543", RegistrationDate = new DateTime(2025, 6, 18) },
            new Customer { Id = 13, FirstName = "Mia", LastName = "Parker", Email = "mia.parker@example.com", PhoneNumber = "(112) 256-3210", RegistrationDate = new DateTime(2025, 7, 2) },
            new Customer { Id = 14, FirstName = "Noah", LastName = "Quinn", Email = "noah.quinn@example.com", PhoneNumber = "(345) 342-1478", RegistrationDate = new DateTime(2025, 8, 8) },
            new Customer { Id = 15, FirstName = "Olivia", LastName = "Reed", Email = "olivia.reed@example.com", PhoneNumber = "(657) 555-9632", RegistrationDate = new DateTime(2025, 9, 1) },
        ];
    }
}
