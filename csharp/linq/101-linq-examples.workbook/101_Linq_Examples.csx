#r "System.Xml.Linq"
#r "System.Data"
#r "System.Data.DataSetExtensions"

using System.Xml.Linq;
using System.Data;


public class Product
{
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
}

public class Order
{
    public int OrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
}

public class Customer
{
    public string CustomerID { get; set; }
    public string CompanyName { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public Order[] Orders { get; set; }
}

// Data
var productList = 
    new List<Product> {
    new Product { ProductID = 1, ProductName = "Chai", Category = "Beverages", UnitPrice = 18.0000M, UnitsInStock = 39 },
    new Product { ProductID = 2, ProductName = "Chang", Category = "Beverages", UnitPrice = 19.0000M, UnitsInStock = 17 },
    new Product { ProductID = 3, ProductName = "Aniseed Syrup", Category = "Condiments", UnitPrice = 10.0000M, UnitsInStock = 13 },
    new Product { ProductID = 4, ProductName = "Chef Anton's Cajun Seasoning", Category = "Condiments", UnitPrice = 22.0000M, UnitsInStock = 53 },
    new Product { ProductID = 5, ProductName = "Chef Anton's Gumbo Mix", Category = "Condiments", UnitPrice = 21.3500M, UnitsInStock = 0 },
    new Product { ProductID = 6, ProductName = "Grandma's Boysenberry Spread", Category = "Condiments", UnitPrice = 25.0000M, UnitsInStock = 120 },
    new Product { ProductID = 7, ProductName = "Uncle Bob's Organic Dried Pears", Category = "Produce", UnitPrice = 30.0000M, UnitsInStock = 15 },
    new Product { ProductID = 8, ProductName = "Northwoods Cranberry Sauce", Category = "Condiments", UnitPrice = 40.0000M, UnitsInStock = 6 },
    new Product { ProductID = 9, ProductName = "Mishi Kobe Niku", Category = "Meat/Poultry", UnitPrice = 97.0000M, UnitsInStock = 29 },
    new Product { ProductID = 10, ProductName = "Ikura", Category = "Seafood", UnitPrice = 31.0000M, UnitsInStock = 31 },
    new Product { ProductID = 11, ProductName = "Queso Cabrales", Category = "Dairy Products", UnitPrice = 21.0000M, UnitsInStock = 22 },
    new Product { ProductID = 12, ProductName = "Queso Manchego La Pastora", Category = "Dairy Products", UnitPrice = 38.0000M, UnitsInStock = 86 },
    new Product { ProductID = 13, ProductName = "Konbu", Category = "Seafood", UnitPrice = 6.0000M, UnitsInStock = 24 },
    new Product { ProductID = 14, ProductName = "Tofu", Category = "Produce", UnitPrice = 23.2500M, UnitsInStock = 35 },
    new Product { ProductID = 15, ProductName = "Genen Shouyu", Category = "Condiments", UnitPrice = 15.5000M, UnitsInStock = 39 },
    new Product { ProductID = 16, ProductName = "Pavlova", Category = "Confections", UnitPrice = 17.4500M, UnitsInStock = 29 },
    new Product { ProductID = 17, ProductName = "Alice Mutton", Category = "Meat/Poultry", UnitPrice = 39.0000M, UnitsInStock = 0 },
    new Product { ProductID = 18, ProductName = "Carnarvon Tigers", Category = "Seafood", UnitPrice = 62.5000M, UnitsInStock = 42 },
    new Product { ProductID = 19, ProductName = "Teatime Chocolate Biscuits", Category = "Confections", UnitPrice = 9.2000M, UnitsInStock = 25 },
    new Product { ProductID = 20, ProductName = "Sir Rodney's Marmalade", Category = "Confections", UnitPrice = 81.0000M, UnitsInStock = 40 },
    new Product { ProductID = 21, ProductName = "Sir Rodney's Scones", Category = "Confections", UnitPrice = 10.0000M, UnitsInStock = 3 },
    new Product { ProductID = 22, ProductName = "Gustaf's Knäckebröd", Category = "Grains/Cereals", UnitPrice = 21.0000M, UnitsInStock = 104 },
    new Product { ProductID = 23, ProductName = "Tunnbröd", Category = "Grains/Cereals", UnitPrice = 9.0000M, UnitsInStock = 61 },
    new Product { ProductID = 24, ProductName = "Guaraná Fantástica", Category = "Beverages", UnitPrice = 4.5000M, UnitsInStock = 20 },
    new Product { ProductID = 25, ProductName = "NuNuCa Nuß-Nougat-Creme", Category = "Confections", UnitPrice = 14.0000M, UnitsInStock = 76 },
    new Product { ProductID = 26, ProductName = "Gumbär Gummibärchen", Category = "Confections", UnitPrice = 31.2300M, UnitsInStock = 15 },
    new Product { ProductID = 27, ProductName = "Schoggi Schokolade", Category = "Confections", UnitPrice = 43.9000M, UnitsInStock = 49 },
    new Product { ProductID = 28, ProductName = "Rössle Sauerkraut", Category = "Produce", UnitPrice = 45.6000M, UnitsInStock = 26 },
    new Product { ProductID = 29, ProductName = "Thüringer Rostbratwurst", Category = "Meat/Poultry", UnitPrice = 123.7900M, UnitsInStock = 0 },
    new Product { ProductID = 30, ProductName = "Nord-Ost Matjeshering", Category = "Seafood", UnitPrice = 25.8900M, UnitsInStock = 10 },
    new Product { ProductID = 31, ProductName = "Gorgonzola Telino", Category = "Dairy Products", UnitPrice = 12.5000M, UnitsInStock = 0 },
    new Product { ProductID = 32, ProductName = "Mascarpone Fabioli", Category = "Dairy Products", UnitPrice = 32.0000M, UnitsInStock = 9 },
    new Product { ProductID = 33, ProductName = "Geitost", Category = "Dairy Products", UnitPrice = 2.5000M, UnitsInStock = 112 },
    new Product { ProductID = 34, ProductName = "Sasquatch Ale", Category = "Beverages", UnitPrice = 14.0000M, UnitsInStock = 111 },
    new Product { ProductID = 35, ProductName = "Steeleye Stout", Category = "Beverages", UnitPrice = 18.0000M, UnitsInStock = 20 },
    new Product { ProductID = 36, ProductName = "Inlagd Sill", Category = "Seafood", UnitPrice = 19.0000M, UnitsInStock = 112 },
    new Product { ProductID = 37, ProductName = "Gravad lax", Category = "Seafood", UnitPrice = 26.0000M, UnitsInStock = 11 },
    new Product { ProductID = 38, ProductName = "Côte de Blaye", Category = "Beverages", UnitPrice = 263.5000M, UnitsInStock = 17 },
    new Product { ProductID = 39, ProductName = "Chartreuse verte", Category = "Beverages", UnitPrice = 18.0000M, UnitsInStock = 69 },
    new Product { ProductID = 40, ProductName = "Boston Crab Meat", Category = "Seafood", UnitPrice = 18.4000M, UnitsInStock = 123 },
    new Product { ProductID = 41, ProductName = "Jack's New England Clam Chowder", Category = "Seafood", UnitPrice = 9.6500M, UnitsInStock = 85 },
    new Product { ProductID = 42, ProductName = "Singaporean Hokkien Fried Mee", Category = "Grains/Cereals", UnitPrice = 14.0000M, UnitsInStock = 26 },
    new Product { ProductID = 43, ProductName = "Ipoh Coffee", Category = "Beverages", UnitPrice = 46.0000M, UnitsInStock = 17 },
    new Product { ProductID = 44, ProductName = "Gula Malacca", Category = "Condiments", UnitPrice = 19.4500M, UnitsInStock = 27 },
    new Product { ProductID = 45, ProductName = "Rogede sild", Category = "Seafood", UnitPrice = 9.5000M, UnitsInStock = 5 },
    new Product { ProductID = 46, ProductName = "Spegesild", Category = "Seafood", UnitPrice = 12.0000M, UnitsInStock = 95 },
    new Product { ProductID = 47, ProductName = "Zaanse koeken", Category = "Confections", UnitPrice = 9.5000M, UnitsInStock = 36 },
    new Product { ProductID = 48, ProductName = "Chocolade", Category = "Confections", UnitPrice = 12.7500M, UnitsInStock = 15 },
    new Product { ProductID = 49, ProductName = "Maxilaku", Category = "Confections", UnitPrice = 20.0000M, UnitsInStock = 10 },
    new Product { ProductID = 50, ProductName = "Valkoinen suklaa", Category = "Confections", UnitPrice = 16.2500M, UnitsInStock = 65 },
    new Product { ProductID = 51, ProductName = "Manjimup Dried Apples", Category = "Produce", UnitPrice = 53.0000M, UnitsInStock = 20 },
    new Product { ProductID = 52, ProductName = "Filo Mix", Category = "Grains/Cereals", UnitPrice = 7.0000M, UnitsInStock = 38 },
    new Product { ProductID = 53, ProductName = "Perth Pasties", Category = "Meat/Poultry", UnitPrice = 32.8000M, UnitsInStock = 0 },
    new Product { ProductID = 54, ProductName = "Tourtière", Category = "Meat/Poultry", UnitPrice = 7.4500M, UnitsInStock = 21 },
    new Product { ProductID = 55, ProductName = "Pâté chinois", Category = "Meat/Poultry", UnitPrice = 24.0000M, UnitsInStock = 115 },
    new Product { ProductID = 56, ProductName = "Gnocchi di nonna Alice", Category = "Grains/Cereals", UnitPrice = 38.0000M, UnitsInStock = 21 },
    new Product { ProductID = 57, ProductName = "Ravioli Angelo", Category = "Grains/Cereals", UnitPrice = 19.5000M, UnitsInStock = 36 },
    new Product { ProductID = 58, ProductName = "Escargots de Bourgogne", Category = "Seafood", UnitPrice = 13.2500M, UnitsInStock = 62 },
    new Product { ProductID = 59, ProductName = "Raclette Courdavault", Category = "Dairy Products", UnitPrice = 55.0000M, UnitsInStock = 79 },
    new Product { ProductID = 60, ProductName = "Camembert Pierrot", Category = "Dairy Products", UnitPrice = 34.0000M, UnitsInStock = 19 },
    new Product { ProductID = 61, ProductName = "Sirop d'érable", Category = "Condiments", UnitPrice = 28.5000M, UnitsInStock = 113 },
    new Product { ProductID = 62, ProductName = "Tarte au sucre", Category = "Confections", UnitPrice = 49.3000M, UnitsInStock = 17 },
    new Product { ProductID = 63, ProductName = "Vegie-spread", Category = "Condiments", UnitPrice = 43.9000M, UnitsInStock = 24 },
    new Product { ProductID = 64, ProductName = "Wimmers gute Semmelknödel", Category = "Grains/Cereals", UnitPrice = 33.2500M, UnitsInStock = 22 },
    new Product { ProductID = 65, ProductName = "Louisiana Fiery Hot Pepper Sauce", Category = "Condiments", UnitPrice = 21.0500M, UnitsInStock = 76 },
    new Product { ProductID = 66, ProductName = "Louisiana Hot Spiced Okra", Category = "Condiments", UnitPrice = 17.0000M, UnitsInStock = 4 },
    new Product { ProductID = 67, ProductName = "Laughing Lumberjack Lager", Category = "Beverages", UnitPrice = 14.0000M, UnitsInStock = 52 },
    new Product { ProductID = 68, ProductName = "Scottish Longbreads", Category = "Confections", UnitPrice = 12.5000M, UnitsInStock = 6 },
    new Product { ProductID = 69, ProductName = "Gudbrandsdalsost", Category = "Dairy Products", UnitPrice = 36.0000M, UnitsInStock = 26 },
    new Product { ProductID = 70, ProductName = "Outback Lager", Category = "Beverages", UnitPrice = 15.0000M, UnitsInStock = 15 },
    new Product { ProductID = 71, ProductName = "Flotemysost", Category = "Dairy Products", UnitPrice = 21.5000M, UnitsInStock = 26 },
    new Product { ProductID = 72, ProductName = "Mozzarella di Giovanni", Category = "Dairy Products", UnitPrice = 34.8000M, UnitsInStock = 14 },
    new Product { ProductID = 73, ProductName = "Röd Kaviar", Category = "Seafood", UnitPrice = 15.0000M, UnitsInStock = 101 },
    new Product { ProductID = 74, ProductName = "Longlife Tofu", Category = "Produce", UnitPrice = 10.0000M, UnitsInStock = 4 },
    new Product { ProductID = 75, ProductName = "Rhönbräu Klosterbier", Category = "Beverages", UnitPrice = 7.7500M, UnitsInStock = 125 },
    new Product { ProductID = 76, ProductName = "Lakkalikööri", Category = "Beverages", UnitPrice = 18.0000M, UnitsInStock = 57 },
    new Product { ProductID = 77, ProductName = "Original Frankfurter grüne Soße", Category = "Condiments", UnitPrice = 13.0000M, UnitsInStock = 32 }
};

List<Product> GetProductList() => productList;

// Customer/Order data read into memory from XML file using XLinq:

var customerList = (
    from e in XDocument.Load("Customers.xml").
              Root.Elements("customer")
    select new Customer
    {
        CustomerID = (string)e.Element("id"),
        CompanyName = (string)e.Element("name"),
        Address = (string)e.Element("address"),
        City = (string)e.Element("city"),
        Region = (string)e.Element("region"),
        PostalCode = (string)e.Element("postalcode"),
        Country = (string)e.Element("country"),
        Phone = (string)e.Element("phone"),
        Fax = (string)e.Element("fax"),
        Orders = (
            from o in e.Elements("orders").Elements("order")
            select new Order
            {
                OrderID = (int)o.Element("id"),
                OrderDate = (DateTime)o.Element("orderdate"),
                Total = (decimal)o.Element("total")
            })
            .ToArray()
    })
    .ToList();

List<Customer> GetCustomerList() => customerList;

public class Supplier
{
    public string SupplierName { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

var supplierList = new List<Supplier>(){
                    new Supplier {SupplierName = "Exotic Liquids", Address = "49 Gilbert St.", City = "London", Country = "UK"},
                    new Supplier {SupplierName = "New Orleans Cajun Delights", Address = "P.O. Box 78934", City = "New Orleans", Country = "USA"},
                    new Supplier {SupplierName = "Grandma Kelly's Homestead", Address = "707 Oxford Rd.", City = "Ann Arbor", Country = "USA"},
                    new Supplier {SupplierName = "Tokyo Traders", Address = "9-8 Sekimai Musashino-shi", City = "Tokyo", Country = "Japan"},
                    new Supplier {SupplierName = "Cooperativa de Quesos 'Las Cabras'", Address = "Calle del Rosal 4", City = "Oviedo", Country = "Spain"},
                    new Supplier {SupplierName = "Mayumi's", Address = "92 Setsuko Chuo-ku", City = "Osaka", Country = "Japan"},
                    new Supplier {SupplierName = "Pavlova, Ltd.", Address = "74 Rose St. Moonie Ponds", City = "Melbourne", Country = "Australia"},
                    new Supplier {SupplierName = "Specialty Biscuits, Ltd.", Address = "29 King's Way", City = "Manchester", Country = "UK"},
                    new Supplier {SupplierName = "PB Knäckebröd AB", Address = "Kaloadagatan 13", City = "Göteborg", Country = "Sweden"},
                    new Supplier {SupplierName = "Refrescos Americanas LTDA", Address = "Av. das Americanas 12.890", City = "Sao Paulo", Country = "Brazil"},
                    new Supplier {SupplierName = "Heli Süßwaren GmbH & Co. KG", Address = "Tiergartenstraße 5", City = "Berlin", Country = "Germany"},
                    new Supplier {SupplierName = "Plutzer Lebensmittelgroßmärkte AG", Address = "Bogenallee 51", City = "Frankfurt", Country = "Germany"},
                    new Supplier {SupplierName = "Nord-Ost-Fisch Handelsgesellschaft mbH", Address = "Frahmredder 112a", City = "Cuxhaven", Country = "Germany"},
                    new Supplier {SupplierName = "Formaggi Fortini s.r.l.", Address = "Viale Dante, 75", City = "Ravenna", Country = "Italy"},
                    new Supplier {SupplierName = "Norske Meierier", Address = "Hatlevegen 5", City = "Sandvika", Country = "Norway"},
                    new Supplier {SupplierName = "Bigfoot Breweries", Address = "3400 - 8th Avenue Suite 210", City = "Bend", Country = "USA"},
                    new Supplier {SupplierName = "Svensk Sjöföda AB", Address = "Brovallavägen 231", City = "Stockholm", Country = "Sweden"},
                    new Supplier {SupplierName = "Aux joyeux ecclésiastiques", Address = "203, Rue des Francs-Bourgeois", City = "Paris", Country = "France"},
                    new Supplier {SupplierName = "New England Seafood Cannery", Address = "Order Processing Dept. 2100 Paul Revere Blvd.", City = "Boston", Country = "USA"},
                    new Supplier {SupplierName = "Leka Trading", Address = "471 Serangoon Loop, Suite #402", City = "Singapore", Country = "Singapore"},
                    new Supplier {SupplierName = "Lyngbysild", Address = "Lyngbysild Fiskebakken 10", City = "Lyngby", Country = "Denmark"},
                    new Supplier {SupplierName = "Zaanse Snoepfabriek", Address = "Verkoop Rijnweg 22", City = "Zaandam", Country = "Netherlands"},
                    new Supplier {SupplierName = "Karkki Oy", Address = "Valtakatu 12", City = "Lappeenranta", Country = "Finland"},
                    new Supplier {SupplierName = "G'day, Mate", Address = "170 Prince Edward Parade Hunter's Hill", City = "Sydney", Country = "Australia"},
                    new Supplier {SupplierName = "Ma Maison", Address = "2960 Rue St. Laurent", City = "Montréal", Country = "Canada"},
                    new Supplier {SupplierName = "Pasta Buttini s.r.l.", Address = "Via dei Gelsomini, 153", City = "Salerno", Country = "Italy"},
                    new Supplier {SupplierName = "Escargots Nouveaux", Address = "22, rue H. Voiron", City = "Montceau", Country = "France"},
                    new Supplier {SupplierName = "Gai pâturage", Address = "Bat. B 3, rue des Alpes", City = "Annecy", Country = "France"},
                    new Supplier {SupplierName = "Forêts d'érables", Address = "148 rue Chasseur", City = "Ste-Hyacinthe", Country = "Canada"},
                };

List<Supplier> GetSupplierList() => supplierList;

#region Projection

var testDS = TestHelper.CreateTestDataset();

internal class TestHelper
    {

        internal static IEnumerable<string> ZipToStrings<T,U>(IEnumerable<T> xs, IEnumerable<U> ys, Func<T,U,string> fn){
            return xs.Zip(ys, fn);
        }

        internal static DataSet CreateTestDataset()
        {
            DataSet ds = new DataSet();

            // Customers Table
            ds.Tables.Add(CreateNumbersTable());
            ds.Tables.Add(CreateLowNumbersTable());
            ds.Tables.Add(CreateEmptyNumbersTable());
            ds.Tables.Add(CreateProductList());
            ds.Tables.Add(CreateDigitsTable());
            ds.Tables.Add(CreateWordsTable());
            ds.Tables.Add(CreateWords2Table());
            ds.Tables.Add(CreateWords3Table());
            ds.Tables.Add(CreateWords4Table());
            ds.Tables.Add(CreateAnagramsTable());
            ds.Tables.Add(CreateNumbersATable());
            ds.Tables.Add(CreateNumbersBTable());
            ds.Tables.Add(CreateFactorsOf300());
            ds.Tables.Add(CreateDoublesTable());
            ds.Tables.Add(CreateScoreRecordsTable());
            ds.Tables.Add(CreateAttemptedWithdrawalsTable());
            ds.Tables.Add(CreateEmployees1Table());
            ds.Tables.Add(CreateEmployees2Table());

            CreateCustomersAndOrdersTables(ds);

            ds.AcceptChanges();
            return ds;
        }


        private static DataTable CreateNumbersTable()
        {

            int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };
            DataTable table = new DataTable("Numbers");
            table.Columns.Add("number", typeof(int));

            foreach (int n in numbers)
            {
                table.Rows.Add(new object[] { n });
            }

            return table;
        }

        private static DataTable CreateEmptyNumbersTable()
        {

            DataTable table = new DataTable("EmptyNumbers");
            table.Columns.Add("number", typeof(int));
            return table;
        }

        private static DataTable CreateDigitsTable()
        {

            string[] digits = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            DataTable table = new DataTable("Digits");
            table.Columns.Add("digit", typeof(string));

            foreach (string digit in digits)
            {
                table.Rows.Add(new object[] { digit });
            }
            return table;
        }

        private static DataTable CreateWordsTable()
        {
            string[] words = { "aPPLE", "BlUeBeRrY", "cHeRry" };
            DataTable table = new DataTable("Words");
            table.Columns.Add("word", typeof(string));

            foreach (string word in words)
            {
                table.Rows.Add(new object[] { word });
            }
            return table;
        }

        private static DataTable CreateWords2Table()
        {
            string[] words = { "believe", "relief", "receipt", "field" };
            DataTable table = new DataTable("Words2");
            table.Columns.Add("word", typeof(string));

            foreach (string word in words)
            {
                table.Rows.Add(new object[] { word });
            }
            return table;
        }

        private static DataTable CreateWords3Table()
        {
            string[] words = { "aPPLE", "AbAcUs", "bRaNcH", "BlUeBeRrY", "ClOvEr", "cHeRry" };
            DataTable table = new DataTable("Words3");
            table.Columns.Add("word", typeof(string));

            foreach (string word in words)
            {
                table.Rows.Add(new object[] { word });
            }
            return table;
        }

        private static DataTable CreateWords4Table()
        {
            string[] words = { "blueberry", "chimpanzee", "abacus", "banana", "apple", "cheese" };
            DataTable table = new DataTable("Words4");
            table.Columns.Add("word", typeof(string));

            foreach (string word in words)
            {
                table.Rows.Add(new object[] { word });
            }
            return table;
        }

        private static DataTable CreateAnagramsTable()
        {
            string[] anagrams = { "from   ", " salt", " earn ", "  last   ", " near ", " form  " };
            DataTable table = new DataTable("Anagrams");
            table.Columns.Add("anagram", typeof(string));

            foreach (string word in anagrams)
            {
                table.Rows.Add(new object[] { word });
            }
            return table;
        }

        private static DataTable CreateScoreRecordsTable()
        {
            var scoreRecords = new[] { new {Name = "Alice", Score = 50},
                                new {Name = "Bob"  , Score = 40},
                                new {Name = "Cathy", Score = 45}
                            };

            DataTable table = new DataTable("ScoreRecords");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Score", typeof(int));

            foreach (var r in scoreRecords)
            {
                table.Rows.Add(new object[] { r.Name, r.Score });
            }
            return table;
        }

        private static DataTable CreateAttemptedWithdrawalsTable()
        {
            int[] attemptedWithdrawals = { 20, 10, 40, 50, 10, 70, 30 };

            DataTable table = new DataTable("AttemptedWithdrawals");
            table.Columns.Add("withdrawal", typeof(int));

            foreach (var r in attemptedWithdrawals)
            {
                table.Rows.Add(new object[] { r });
            }
            return table;
        }

        private static DataTable CreateNumbersATable()
        {
            int[] numbersA = { 0, 2, 4, 5, 6, 8, 9 };
            DataTable table = new DataTable("NumbersA");
            table.Columns.Add("number", typeof(int));

            foreach (int number in numbersA)
            {
                table.Rows.Add(new object[] { number });
            }
            return table;
        }

        private static DataTable CreateNumbersBTable()
        {
            int[] numbersB = { 1, 3, 5, 7, 8 };
            DataTable table = new DataTable("NumbersB");
            table.Columns.Add("number", typeof(int));

            foreach (int number in numbersB)
            {
                table.Rows.Add(new object[] { number });
            }
            return table;
        }

        private static DataTable CreateLowNumbersTable()
        {
            int[] lowNumbers = { 1, 11, 3, 19, 41, 65, 19 };
            DataTable table = new DataTable("LowNumbers");
            table.Columns.Add("number", typeof(int));

            foreach (int number in lowNumbers)
            {
                table.Rows.Add(new object[] { number });
            }
            return table;
        }

        private static DataTable CreateFactorsOf300()
        {
            int[] factorsOf300 = { 2, 2, 3, 5, 5 };

            DataTable table = new DataTable("FactorsOf300");
            table.Columns.Add("factor", typeof(int));

            foreach (int factor in factorsOf300)
            {
                table.Rows.Add(new object[] { factor });
            }
            return table;
        }

        private static DataTable CreateDoublesTable()
        {
            double[] doubles = { 1.7, 2.3, 1.9, 4.1, 2.9 };

            DataTable table = new DataTable("Doubles");
            table.Columns.Add("double", typeof(double));

            foreach (double d in doubles)
            {
                table.Rows.Add(new object[] { d });
            }
            return table;
        }

        private static DataTable CreateEmployees1Table()
        {

            DataTable table = new DataTable("Employees1");
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("worklevel", typeof(int));

            table.Rows.Add(new object[] { 1, "Jones", 5 });
            table.Rows.Add(new object[] { 2, "Smith", 5 });
            table.Rows.Add(new object[] { 2, "Smith", 5 });
            table.Rows.Add(new object[] { 3, "Smith", 6 });
            table.Rows.Add(new object[] { 4, "Arthur", 11 });
            table.Rows.Add(new object[] { 5, "Arthur", 12 });

            return table;
        }

        private static DataTable CreateEmployees2Table()
        {

            DataTable table = new DataTable("Employees2");
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("lastname", typeof(string));
            table.Columns.Add("level", typeof(int));

            table.Rows.Add(new object[] { 1, "Jones", 10 });
            table.Rows.Add(new object[] { 2, "Jagger", 5 });
            table.Rows.Add(new object[] { 3, "Thomas", 6 });
            table.Rows.Add(new object[] { 4, "Collins", 11 });
            table.Rows.Add(new object[] { 4, "Collins", 12 });
            table.Rows.Add(new object[] { 5, "Arthur", 12 });

            return table;
        }

        private static void CreateCustomersAndOrdersTables(DataSet ds)
        {

            DataTable customers = new DataTable("Customers");
            customers.Columns.Add("CustomerID", typeof(string));
            customers.Columns.Add("CompanyName", typeof(string));
            customers.Columns.Add("Address", typeof(string));
            customers.Columns.Add("City", typeof(string));
            customers.Columns.Add("Region", typeof(string));
            customers.Columns.Add("PostalCode", typeof(string));
            customers.Columns.Add("Country", typeof(string));
            customers.Columns.Add("Phone", typeof(string));
            customers.Columns.Add("Fax", typeof(string));

            ds.Tables.Add(customers);

            DataTable orders = new DataTable("Orders");

            orders.Columns.Add("OrderID", typeof(int));
            orders.Columns.Add("CustomerID", typeof(string));
            orders.Columns.Add("OrderDate", typeof(DateTime));
            orders.Columns.Add("Total", typeof(decimal));

            ds.Tables.Add(orders);

            DataRelation co = new DataRelation("CustomersOrders", customers.Columns["CustomerID"], orders.Columns["CustomerID"], true);
            ds.Relations.Add(co);

            var customerList = (
                from e in XDocument.Load("customers.xml").
                          Root.Elements("customer")
                select new Customer
                {
                    CustomerID = (string)e.Element("id"),
                    CompanyName = (string)e.Element("name"),
                    Address = (string)e.Element("address"),
                    City = (string)e.Element("city"),
                    Region = (string)e.Element("region"),
                    PostalCode = (string)e.Element("postalcode"),
                    Country = (string)e.Element("country"),
                    Phone = (string)e.Element("phone"),
                    Fax = (string)e.Element("fax"),
                    Orders = (
                        from o in e.Elements("orders").Elements("order")
                        select new Order
                        {
                            OrderID = (int)o.Element("id"),
                            OrderDate = (DateTime)o.Element("orderdate"),
                            Total = (decimal)o.Element("total")
                        })
                        .ToArray()
                }
                ).ToList();

            foreach (Customer cust in customerList)
            {
                customers.Rows.Add(new object[] {cust.CustomerID, cust.CompanyName, cust.Address, cust.City, cust.Region,
                                                cust.PostalCode, cust.Country, cust.Phone, cust.Fax});
                foreach (Order order in cust.Orders)
                {
                    orders.Rows.Add(new object[] { order.OrderID, cust.CustomerID, order.OrderDate, order.Total });
                }
            }
        }

        private static DataTable CreateProductList()
        {

            DataTable table = new DataTable("Products");
            table.Columns.Add("ProductID", typeof(int));
            table.Columns.Add("ProductName", typeof(string));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("UnitPrice", typeof(decimal));
            table.Columns.Add("UnitsInStock", typeof(int));

            var productList = new[] {
              new { ProductID = 1, ProductName = "Chai", Category = "Beverages", 
                UnitPrice = 18.0000M, UnitsInStock = 39 },
              new { ProductID = 2, ProductName = "Chang", Category = "Beverages", 
                UnitPrice = 19.0000M, UnitsInStock = 17 },
              new { ProductID = 3, ProductName = "Aniseed Syrup", Category = "Condiments", 
                UnitPrice = 10.0000M, UnitsInStock = 13 },
              new { ProductID = 4, ProductName = "Chef Anton's Cajun Seasoning", Category = "Condiments", 
                UnitPrice = 22.0000M, UnitsInStock = 53 },
              new { ProductID = 5, ProductName = "Chef Anton's Gumbo Mix", Category = "Condiments", 
                UnitPrice = 21.3500M, UnitsInStock = 0 },
              new { ProductID = 6, ProductName = "Grandma's Boysenberry Spread", Category = "Condiments", 
                UnitPrice = 25.0000M, UnitsInStock = 120 },
              new { ProductID = 7, ProductName = "Uncle Bob's Organic Dried Pears", Category = "Produce", 
                UnitPrice = 30.0000M, UnitsInStock = 15 },
              new { ProductID = 8, ProductName = "Northwoods Cranberry Sauce", Category = "Condiments", 
                UnitPrice = 40.0000M, UnitsInStock = 6 },
              new { ProductID = 9, ProductName = "Mishi Kobe Niku", Category = "Meat/Poultry", 
                UnitPrice = 97.0000M, UnitsInStock = 29 },
              new { ProductID = 10, ProductName = "Ikura", Category = "Seafood", 
                UnitPrice = 31.0000M, UnitsInStock = 31 },
              new { ProductID = 11, ProductName = "Queso Cabrales", Category = "Dairy Products", 
                UnitPrice = 21.0000M, UnitsInStock = 22 },
              new { ProductID = 12, ProductName = "Queso Manchego La Pastora", Category = "Dairy Products", 
                UnitPrice = 38.0000M, UnitsInStock = 86 },
              new { ProductID = 13, ProductName = "Konbu", Category = "Seafood", 
                UnitPrice = 6.0000M, UnitsInStock = 24 },
              new { ProductID = 14, ProductName = "Tofu", Category = "Produce", 
                UnitPrice = 23.2500M, UnitsInStock = 35 },
              new { ProductID = 15, ProductName = "Genen Shouyu", Category = "Condiments", 
                UnitPrice = 15.5000M, UnitsInStock = 39 },
              new { ProductID = 16, ProductName = "Pavlova", Category = "Confections", 
                UnitPrice = 17.4500M, UnitsInStock = 29 },
              new { ProductID = 17, ProductName = "Alice Mutton", Category = "Meat/Poultry", 
                UnitPrice = 39.0000M, UnitsInStock = 0 },
              new { ProductID = 18, ProductName = "Carnarvon Tigers", Category = "Seafood", 
                UnitPrice = 62.5000M, UnitsInStock = 42 },
              new { ProductID = 19, ProductName = "Teatime Chocolate Biscuits", Category = "Confections", 
                UnitPrice = 9.2000M, UnitsInStock = 25 },
              new { ProductID = 20, ProductName = "Sir Rodney's Marmalade", Category = "Confections", 
                UnitPrice = 81.0000M, UnitsInStock = 40 },
              new { ProductID = 21, ProductName = "Sir Rodney's Scones", Category = "Confections", 
                UnitPrice = 10.0000M, UnitsInStock = 3 },
              new { ProductID = 22, ProductName = "Gustaf's Knäckebröd", Category = "Grains/Cereals", 
                UnitPrice = 21.0000M, UnitsInStock = 104 },
              new { ProductID = 23, ProductName = "Tunnbröd", Category = "Grains/Cereals", 
                UnitPrice = 9.0000M, UnitsInStock = 61 },
              new { ProductID = 24, ProductName = "Guaraná Fantástica", Category = "Beverages", 
                UnitPrice = 4.5000M, UnitsInStock = 20 },
              new { ProductID = 25, ProductName = "NuNuCa Nuß-Nougat-Creme", Category = "Confections", 
                UnitPrice = 14.0000M, UnitsInStock = 76 },
              new { ProductID = 26, ProductName = "Gumbär Gummibärchen", Category = "Confections", 
                UnitPrice = 31.2300M, UnitsInStock = 15 },
              new { ProductID = 27, ProductName = "Schoggi Schokolade", Category = "Confections", 
                UnitPrice = 43.9000M, UnitsInStock = 49 },
              new { ProductID = 28, ProductName = "Rössle Sauerkraut", Category = "Produce", 
                UnitPrice = 45.6000M, UnitsInStock = 26 },
              new { ProductID = 29, ProductName = "Thüringer Rostbratwurst", Category = "Meat/Poultry", 
                UnitPrice = 123.7900M, UnitsInStock = 0 },
              new { ProductID = 30, ProductName = "Nord-Ost Matjeshering", Category = "Seafood", 
                UnitPrice = 25.8900M, UnitsInStock = 10 },
              new { ProductID = 31, ProductName = "Gorgonzola Telino", Category = "Dairy Products", 
                UnitPrice = 12.5000M, UnitsInStock = 0 },
              new { ProductID = 32, ProductName = "Mascarpone Fabioli", Category = "Dairy Products", 
                UnitPrice = 32.0000M, UnitsInStock = 9 },
              new { ProductID = 33, ProductName = "Geitost", Category = "Dairy Products", 
                UnitPrice = 2.5000M, UnitsInStock = 112 },
              new { ProductID = 34, ProductName = "Sasquatch Ale", Category = "Beverages", 
                UnitPrice = 14.0000M, UnitsInStock = 111 },
              new { ProductID = 35, ProductName = "Steeleye Stout", Category = "Beverages", 
                UnitPrice = 18.0000M, UnitsInStock = 20 },
              new { ProductID = 36, ProductName = "Inlagd Sill", Category = "Seafood", 
                UnitPrice = 19.0000M, UnitsInStock = 112 },
              new { ProductID = 37, ProductName = "Gravad lax", Category = "Seafood", 
                UnitPrice = 26.0000M, UnitsInStock = 11 },
              new { ProductID = 38, ProductName = "Côte de Blaye", Category = "Beverages", 
                UnitPrice = 263.5000M, UnitsInStock = 17 },
              new { ProductID = 39, ProductName = "Chartreuse verte", Category = "Beverages", 
                UnitPrice = 18.0000M, UnitsInStock = 69 },
              new { ProductID = 40, ProductName = "Boston Crab Meat", Category = "Seafood", 
                UnitPrice = 18.4000M, UnitsInStock = 123 },
              new { ProductID = 41, ProductName = "Jack's New England Clam Chowder", Category = "Seafood", 
                UnitPrice = 9.6500M, UnitsInStock = 85 },
              new { ProductID = 42, ProductName = "Singaporean Hokkien Fried Mee", Category = "Grains/Cereals", 
                UnitPrice = 14.0000M, UnitsInStock = 26 },
              new { ProductID = 43, ProductName = "Ipoh Coffee", Category = "Beverages", 
                UnitPrice = 46.0000M, UnitsInStock = 17 },
              new { ProductID = 44, ProductName = "Gula Malacca", Category = "Condiments", 
                UnitPrice = 19.4500M, UnitsInStock = 27 },
              new { ProductID = 45, ProductName = "Rogede sild", Category = "Seafood", 
                UnitPrice = 9.5000M, UnitsInStock = 5 },
              new { ProductID = 46, ProductName = "Spegesild", Category = "Seafood", 
                UnitPrice = 12.0000M, UnitsInStock = 95 },
              new { ProductID = 47, ProductName = "Zaanse koeken", Category = "Confections", 
                UnitPrice = 9.5000M, UnitsInStock = 36 },
              new { ProductID = 48, ProductName = "Chocolade", Category = "Confections", 
                UnitPrice = 12.7500M, UnitsInStock = 15 },
              new { ProductID = 49, ProductName = "Maxilaku", Category = "Confections", 
                UnitPrice = 20.0000M, UnitsInStock = 10 },
              new { ProductID = 50, ProductName = "Valkoinen suklaa", Category = "Confections", 
                UnitPrice = 16.2500M, UnitsInStock = 65 },
              new { ProductID = 51, ProductName = "Manjimup Dried Apples", Category = "Produce", 
                UnitPrice = 53.0000M, UnitsInStock = 20 },
              new { ProductID = 52, ProductName = "Filo Mix", Category = "Grains/Cereals", 
                UnitPrice = 7.0000M, UnitsInStock = 38 },
              new { ProductID = 53, ProductName = "Perth Pasties", Category = "Meat/Poultry", 
                UnitPrice = 32.8000M, UnitsInStock = 0 },
              new { ProductID = 54, ProductName = "Tourtière", Category = "Meat/Poultry", 
                UnitPrice = 7.4500M, UnitsInStock = 21 },
              new { ProductID = 55, ProductName = "Pâté chinois", Category = "Meat/Poultry", 
                UnitPrice = 24.0000M, UnitsInStock = 115 },
              new { ProductID = 56, ProductName = "Gnocchi di nonna Alice", Category = "Grains/Cereals", 
                UnitPrice = 38.0000M, UnitsInStock = 21 },
              new { ProductID = 57, ProductName = "Ravioli Angelo", Category = "Grains/Cereals", 
                UnitPrice = 19.5000M, UnitsInStock = 36 },
              new { ProductID = 58, ProductName = "Escargots de Bourgogne", Category = "Seafood", 
                UnitPrice = 13.2500M, UnitsInStock = 62 },
              new { ProductID = 59, ProductName = "Raclette Courdavault", Category = "Dairy Products", 
                UnitPrice = 55.0000M, UnitsInStock = 79 },
              new { ProductID = 60, ProductName = "Camembert Pierrot", Category = "Dairy Products", 
                UnitPrice = 34.0000M, UnitsInStock = 19 },
              new { ProductID = 61, ProductName = "Sirop d'érable", Category = "Condiments", 
                UnitPrice = 28.5000M, UnitsInStock = 113 },
              new { ProductID = 62, ProductName = "Tarte au sucre", Category = "Confections", 
                UnitPrice = 49.3000M, UnitsInStock = 17 },
              new { ProductID = 63, ProductName = "Vegie-spread", Category = "Condiments", 
                UnitPrice = 43.9000M, UnitsInStock = 24 },
              new { ProductID = 64, ProductName = "Wimmers gute Semmelknödel", Category = "Grains/Cereals", 
                UnitPrice = 33.2500M, UnitsInStock = 22 },
              new { ProductID = 65, ProductName = "Louisiana Fiery Hot Pepper Sauce", Category = "Condiments", 
                UnitPrice = 21.0500M, UnitsInStock = 76 },
              new { ProductID = 66, ProductName = "Louisiana Hot Spiced Okra", Category = "Condiments", 
                UnitPrice = 17.0000M, UnitsInStock = 4 },
              new { ProductID = 67, ProductName = "Laughing Lumberjack Lager", Category = "Beverages", 
                UnitPrice = 14.0000M, UnitsInStock = 52 },
              new { ProductID = 68, ProductName = "Scottish Longbreads", Category = "Confections", 
                UnitPrice = 12.5000M, UnitsInStock = 6 },
              new { ProductID = 69, ProductName = "Gudbrandsdalsost", Category = "Dairy Products", 
                UnitPrice = 36.0000M, UnitsInStock = 26 },
              new { ProductID = 70, ProductName = "Outback Lager", Category = "Beverages", 
                UnitPrice = 15.0000M, UnitsInStock = 15 },
              new { ProductID = 71, ProductName = "Flotemysost", Category = "Dairy Products", 
                UnitPrice = 21.5000M, UnitsInStock = 26 },
              new { ProductID = 72, ProductName = "Mozzarella di Giovanni", Category = "Dairy Products", 
                UnitPrice = 34.8000M, UnitsInStock = 14 },
              new { ProductID = 73, ProductName = "Röd Kaviar", Category = "Seafood", 
                UnitPrice = 15.0000M, UnitsInStock = 101 },
              new { ProductID = 74, ProductName = "Longlife Tofu", Category = "Produce", 
                UnitPrice = 10.0000M, UnitsInStock = 4 },
              new { ProductID = 75, ProductName = "Rhönbräu Klosterbier", Category = "Beverages", 
                UnitPrice = 7.7500M, UnitsInStock = 125 },
              new { ProductID = 76, ProductName = "Lakkalikööri", Category = "Beverages", 
                UnitPrice = 18.0000M, UnitsInStock = 57 },
              new { ProductID = 77, ProductName = "Original Frankfurter grüne Soße", Category = "Condiments", 
                UnitPrice = 13.0000M, UnitsInStock = 32 }
            };

            foreach (var x in productList)
            {
                table.Rows.Add(new object[] { x.ProductID, x.ProductName, x.Category, x.UnitPrice, x.UnitsInStock });
            }

            return table;
        }
    }

#endregion
