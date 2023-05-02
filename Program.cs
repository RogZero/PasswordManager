using Dapper;
using Npgsql;
using System.Text;


namespace TestingPosgres;

public class classO
{
    // ~~~~~~~~~~~~~~~ Converts a string to integer ~~~~~~~~~~~~~~~
    static int StringToInt(string? inputString)
    {
        if (string.IsNullOrWhiteSpace(inputString))
        {
            throw new ArgumentException("Input string is null or whitespace.", nameof(inputString));
        }

        if (!int.TryParse(inputString, out int intValue))
        {
            throw new ArgumentException("Input string is not a valid integer.", nameof(inputString));
        }

        return intValue;
    }

    // ~~~~~~~~~~~~~~~ Genetates a random number between two integer parameters ~~~~~~~~~~~~~~~
    static int GenerateRandomNumber(int firstNumber, int secondNumber)
    {
        Random rand = new Random();

        int randomNumber = rand.Next(firstNumber, secondNumber);

        return randomNumber;
    }

    //~~~~~~~~~~~~~~~ Genetates a random password using the random number generator. It has two parameteres: (1) the desired password length and (2) the password complexity ~~~~~~~~~~~~~~~
    static string GeneratePassword(int passwordLength, int complexityOption)
    {
        string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
        StringBuilder passwordBuilder = new StringBuilder();

        int minimumCharacterIndex = 0;
        int maximumCharacterIndex = 0;

        switch (complexityOption)
        {
            case 1:
                maximumCharacterIndex = 52;
                break;
            case 2:
                maximumCharacterIndex = 62;
                break;
            case 3:
                maximumCharacterIndex = 72;
                break;
            default:
                throw new ArgumentException("Invalid complexity option.");
        }

        for (int i = 0; i < passwordLength; i++)
        {
            int randomIndex = GenerateRandomNumber(minimumCharacterIndex, maximumCharacterIndex);
            char randomCharacter = characters[randomIndex];
            passwordBuilder.Append(randomCharacter);
        }

        string password = passwordBuilder.ToString();
        return password;
    }

    // ~~~~~~~~~~~~~~~ Deletes an account based on the website's name ~~~~~~~~~~~~~~~
    static void deleteAccount(NpgsqlConnection connection)
    {
        Console.Write("Website name of the password to be deleted: ");
        string? websiteToDelete = Console.ReadLine();
        
        if (string.IsNullOrEmpty(websiteToDelete))
        {
            Console.WriteLine("The website name cannot be null or empty.");
            return;
        }

        connection.Open();

        string query = "SELECT EXISTS(SELECT 1 FROM allthepasswords WHERE websitename = @WebsiteName)";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@WebsiteName", websiteToDelete);

        bool webExists = (command.ExecuteScalar() as bool?) ?? false;

        if(webExists)
        {
            connection.Execute($"DELETE FROM allThePasswords WHERE websiteName = '{websiteToDelete}'");
            Console.WriteLine("The password has been successfully deleted.");
        }
        else
        {
            Console.WriteLine("Since no such a website was found, nothing was deleted.");
        }

        connection.Close();
    }

    // ~~~~~~~~~~~~~~~ Creates a new password based on the parameters and writes it on the console ~~~~~~~~~~~~~~~
    static void createPassword(NpgsqlConnection connection)
    {
        Console.Write("Input the password's length: ");
        string? readToInt = Console.ReadLine();
        int passwordLength = StringToInt(readToInt);

        Console.Write("Input the complexity of the password simple(1), moderate(2) or complex(3): ");

        readToInt = Console.ReadLine();
        int complexityOption = StringToInt(readToInt);

        try
        {
            string password = GeneratePassword(passwordLength, complexityOption);
            Console.WriteLine("Your generated password is: " + password);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine();
    }

    // ~~~~~~~~~~~~~~~ Updates an account's password ~~~~~~~~~~~~~~~
    static void updateAccount(NpgsqlConnection connection)
    {
        Console.Write("Website to change the password: ");
        string? webUpdate = Console.ReadLine();
        Console.Write("New password: ");
        string? newPassword = Console.ReadLine();

        connection.Open();

        connection.Execute($"UPDATE allthepasswords SET websitepassword = '{newPassword}' WHERE websitename = '{webUpdate}';");

        connection.Close();
    }

    // ~~~~~~~~~~~~~~~ Writes the database in the console ~~~~~~~~~~~~~~~
    static void writeDatabase(NpgsqlConnection connection)
    {
        connection.Open();
        var result = connection.Query<passwordDesc>("select * from allThePasswords order by websiteName;").ToList();
        connection.Close();

        if (!result.Any())
        {
            Console.WriteLine("No elements found in the database");

            return;
        }

        // ~~~~~~~~~~~~~~~ Writes the columns ~~~~~~~~~~~~~~~
        {
            string ptxtWebsiteName = ("Website").PadRight(20);
            string ptxtUsername = ("Username").PadRight(40);
            string ptxtPassword = ("Password").PadRight(25);
            Console.WriteLine(ptxtWebsiteName + ptxtUsername + ptxtPassword + "URL");
            Console.WriteLine();
        }

        foreach (passwordDesc pass in result)
        {
            string txtWebsiteName = $"{pass.websiteName}";
            string txtUsername = $"{pass.websiteUsername}";           
            string txtPassword = $"{pass.websitePassword}";
            string ptxtWebsiteName = txtWebsiteName.PadRight(20);
            string ptxtUsername = txtUsername.PadRight(40);
            string ptxtPassword = txtPassword.PadRight(25);
            Console.WriteLine(ptxtWebsiteName + ptxtUsername + ptxtPassword + pass.websiteUrl);
            Thread.Sleep(100);

        }

        Console.WriteLine();
    }

    // ~~~~~~~~~~~~~~~ Adds a new password based on the parameters ~~~~~~~~~~~~~~~
    static void addAccount(NpgsqlConnection connection)
    {
        // ~~~~~~~~~~~~~~~ Getting input from the user ~~~~~~~~~~~~~~~
        Console.Write("Type the website's name: ");
        string? iWbName = Console.ReadLine();
        Console.Write("Type the account's username: ");
        string? iUsername = Console.ReadLine();
        Console.Write("Type the account's password: ");
        string? iPassword = Console.ReadLine();
        Console.Write("Type the website's url: ");
        string? iUrl = Console.ReadLine();
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        connection.Open();
        connection.Execute("INSERT INTO allthepasswords (websiteName, websiteUsername, websitePassword, websiteUrl) VALUES (@wbName, @username, @password, @url)",
            new { wbName = iWbName, username = iUsername, password = iPassword, url = iUrl });
        connection.Close();
    }


// ~~~~~~~~~~~~~~~ MAIN PROGRAM ~~~~~~~~~~~~~~~
    static void Main(string[] args)
    
    {
        var connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=mynotsosecretpassword;Port=5431;";
        var connection = new NpgsqlConnection(connectionString);


        while (true)
        {
            Console.Write("Check the accounts(1), add a new account(2), create a new pasword(3), delete an account(4), update an account's password(5) or teminate the program(0): ");
            
            string? readToInt = Console.ReadLine();
            int option = StringToInt(readToInt);

            Console.WriteLine();

            switch(option)
            {
                case 0:
                return;

                case 1:
                writeDatabase(connection);
                break;

                case 2:            
                addAccount(connection);
                break;    

                case 3:
                createPassword(connection);
                break;      

                case 4:
                deleteAccount(connection);
                break;   

                case 5:
                updateAccount(connection);
                break;

            }

        }
        

    }
}


public class passwordDesc
{
    public string? websiteName {get; set;} // PRIMARY KEY 
    public string? websiteUsername {get; set;}
    public string? websitePassword {get; set;}
    public string? websiteUrl {get; set;}
}