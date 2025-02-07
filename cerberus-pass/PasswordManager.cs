namespace cerberus_pass;

public class PasswordManager
{
    private List<PasswordEntry> vault;

    public PasswordManager()
    {
        // vault = new List<PasswordEntry>();
        // vault = new();
        // vault = [];
        vault = [];
    }

    public List<PasswordEntry> GetAll() => vault;

    public PasswordEntry? CreateEntry(
      string title,
      string login,
      string password,
      string website = "",
      string note = "")
    {
        if (vault.Any(x => x.Title == title))
        {
            return null;
        }
        var newEntry = new PasswordEntry(
          title,
          login,
          password,
          website,
          note
        );
        vault.Add(newEntry);
        return newEntry;
    }

    // GetEntry
    public PasswordEntry GetEntry(string title) =>
      vault.Find(x => x.Title == title);


    // UpdateEntry
    public PasswordEntry UpdateEntry(string titleToChange, PasswordEntry newEntry)
    {
        var indexToUpdate = vault.FindIndex(
          x => x.Title == titleToChange);
        vault[indexToUpdate] = newEntry;
        return vault[indexToUpdate];

        // var entryToChange = vault.Find(x => x.Title == titleToChange);
        // entryToChange = newEntry;
    }

    // DeleteEntry
    public bool DeleteEntry(string titleToDelete) =>
      vault.RemoveAll(x => x.Title == titleToDelete) > 0;

    /*
    public bool DeleteEntry(string titleToDelete)
    {
      var deleteCount = 0;
      for (int i = 0; i > vault.Count; i++)
      {
        if (vault[i].Title == titleToDelete)
        {
          vault.Remove(vault[i]);
          deleteCount++;
        }
      }
      if (deleteCount > 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    } */
}