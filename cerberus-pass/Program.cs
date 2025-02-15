﻿// Main UI-Flow
using System.Security.Cryptography;
using cerberus_pass;

var manager = new PasswordManager();

// Load PasswordEntries from File
// manager.LoadFromFile();

Console.ForegroundColor = ConsoleColor.DarkRed;
Console.WriteLine("Willkommen zu Cerberus-Pass!");
Console.ResetColor();

// first start:
// check if file "masterpass.txt" exists
const string masterPassFilePath = "masterpass.txt";
if (!File.Exists(masterPassFilePath)) // first start
{
    // does not exist
    // prompt user for new masterpass
    Console.WriteLine("Um deinen Passwort-Vault einzurichten, gebe ein Master-Passwort ein.");
    Console.WriteLine("Dieses Passwort wird zur verschlüsselung aller deiner anderen Passwörter verwendet. Stelle sicher, dass dein Passwort komplex und möglichst lang ist, aber auch einfach einzutippen und merken.");
    Console.WriteLine("Falls du dieses Passwort vergisst, kommst du nicht mehr an deine gespeicherten Passwörter!");
    var userInput = Console.ReadLine();
    //   -> not emtpy string
    if (String.IsNullOrEmpty(userInput))
    {
        Console.WriteLine("Master-Passwort muss gesetzt sein!");
        Environment.Exit(1);
    }
    // confirm input
    Console.WriteLine("Master-Passwort bestätigen:");
    var userInputConfirm = Console.ReadLine();
    if (userInput != userInputConfirm)
    {
        Console.WriteLine("Eingegebene Passwörter stimmen nicht überein!");
        Environment.Exit(2);
    }
    // hash masterpass
    var salt = String.Empty;
    var hashedPassword = HashPassword(userInput, out salt); // salt generieren -> neuer salt
                                                            // create "masterpass.txt" and write hashed masterpass to it
                                                            // File.Create(masterPassFilePath).Dispose();
    File.WriteAllLines(masterPassFilePath,
    new[] { hashedPassword, salt });
}
else // every other start
{// "masterpass.txt" exists:
 // prompt user for master-password
    Console.WriteLine("Gebe dein Master-Passwort ein:");
    var userInput = Console.ReadLine();
    // read set masterpass from file
    var storedMasterPass = File.ReadAllLines(masterPassFilePath);
    //   -> not empty string -> delete file, end program
    var storedHash = storedMasterPass[0];
    var storedSalt = storedMasterPass[1];
    //  and compare hashed masterpass from file with hashed user input
    // hash userinput
    // ursprünglichen salt verwenden
    if (
      VerifyPassword(userInput, storedHash, storedSalt)
    )
    {
        Console.WriteLine("Master-Passwort korrekt! Anmeldung erfolgt...");
        Thread.Sleep(2000);
        Console.Clear();
    }
    else
    {
        Console.WriteLine("Passwort stimmt nicht überein. Programm wird beendet!");
        Environment.Exit(3);
    }
    // if no match -> end program with some error
}

do
{
    Console.WriteLine("Wähle was du tun willst:");

    Console.WriteLine("""
  1. Passwort-Liste ausgeben
  2. Passwort mit ID ausgeben
  3. Neues Passwort erstellen
  4. Vorhandenes Passwort bearbeiten
  5. Passwort löschen
""");

    var userInput = Console.ReadLine();

    int menuChoice;

    if (!int.TryParse(userInput, out menuChoice))
        continue;

    switch ((MenuOptions)menuChoice)
    {
        case MenuOptions.List:
            var vault = manager.GetAll();
            foreach (var item in vault)
            {
                Console.WriteLine(item);
            }
            break;
        case MenuOptions.GetOne:
            // todo: Exception wenn title nicht existiert
            Console.WriteLine("Welchen Eintrag willst du ansehen? (Title):");
            var titleToPrint = Console.ReadLine();
            var entry = manager.GetEntry(titleToPrint);
            Console.WriteLine(entry + $"\t{entry.Password}");
            break;
        case MenuOptions.Create:
            Console.WriteLine("Gebe einen Titel für den Eintrag an:");
            var title = Console.ReadLine();
            Console.WriteLine("Gebe einen Login für den Eintrag an:");
            var login = Console.ReadLine();
            Console.WriteLine("Gebe ein Passwort für den Eintrag an:");
            var password = Console.ReadLine();
            var newEntry = manager.CreateEntry(title, login, password);
            if (newEntry is null)
            {
                Console.WriteLine($"Eintrag mit {title} existiert bereits.\nWolltest du diesen Updaten? Oder erstelle einen neuen mit einem anderen Titel.");
            }
            else
            {
                Console.WriteLine("Neuer Eintrag erfolgreich erstellt:");
                Console.WriteLine(newEntry); // Gibt Type aus;
                manager.SaveValt();
            }
            break;
        case MenuOptions.Update:
            Console.WriteLine("Welchen Eintrag willst du ändern? (Title):");
            var title_to_change = Console.ReadLine();
            Console.WriteLine(
              "Gebe einen neuen Titel für den Eintrag an (Leer um nichts zu ändern):");
            var new_title = Console.ReadLine();
            Console.WriteLine(
              "Gebe einen neuen Login für den Eintrag an (Leer um nichts zu ändern):");
            var new_login = Console.ReadLine();
            Console.WriteLine(
              "Gebe ein neues Passwort für den Eintrag an (Leer um nichts zu ändern):");
            var new_password = Console.ReadLine();
            var oldEntry = manager.GetEntry(title_to_change);
            var updatedEntry = manager.UpdateEntry(title_to_change, new PasswordEntry(
              String.IsNullOrEmpty(new_title) ? oldEntry.Title : new_title,
              String.IsNullOrEmpty(new_login) ? oldEntry.Login : new_login,
              String.IsNullOrEmpty(new_password) ? oldEntry.Password : new_password
            ));
            Console.WriteLine($"Eintrag {updatedEntry.Title} wurde erfolgreich aktuallisiert.");
            break;
        case MenuOptions.Delete:
            Console.WriteLine("Welchen Eintrag willst du Löschen? (Title):");
            var titleToDelete = Console.ReadLine();
            if (manager.DeleteEntry(titleToDelete))
                Console.WriteLine($"Eintrag {titleToDelete} wurde erfolgreich entfernt");
            else
                Console.WriteLine(
                  $"Fehler beim löschen des Eintrags: {titleToDelete} wurde nicht gefunden!");
            break;
        default:
            // Fehler anzeigen -> Eingabe-Hint (1-5)
            // Eingabe wiederholen
            Console.WriteLine("Falsche Eingabe! Valide Optionen => 1-5");
            break;
    }
    Console.ReadKey();
    Console.Clear();
} while (true);

string HashPassword(string password, out string salt)
{
    byte[] saltBytes = new byte[16];
    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(saltBytes);
    }
    salt = Convert.ToBase64String(saltBytes);
    var pbkdf2Bytes = Rfc2898DeriveBytes.Pbkdf2(
      password,
      saltBytes,
      10_000,
      HashAlgorithmName.SHA256,
      32
    );
    return Convert.ToBase64String(pbkdf2Bytes);
}

bool VerifyPassword(string enteredPassword, string hashedPassword, string salt)
{
    byte[] saltBytes = Convert.FromBase64String(salt);
    var pbkdf2Bytes = Rfc2898DeriveBytes.Pbkdf2(
      enteredPassword,
      saltBytes,
      10_000,
      HashAlgorithmName.SHA256,
      32
    );
    var enteredHashedPassword = Convert.ToBase64String(pbkdf2Bytes);

    return enteredHashedPassword == hashedPassword;
}