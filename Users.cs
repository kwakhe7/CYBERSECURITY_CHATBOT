// Basic system namespace providing Console methods
using System;

// A small class that represents a user of the chatbot.
// Comments explain each element for someone who is new to C#.
public class User
{
    // Property: stores the user's display name. It can be read from outside
    // but only set inside this class (private setter).
    public string Name { get; private set; }

    // Constructor: called when creating a new User object.
    // We call FormatName to normalise the provided name text.
    public User(string name)
    {
        Name = FormatName(name);
    }

    // FormatName: make a neat display name. If the user typed nothing,
    // return the friendly default "Friend". Trim whitespace and capitalise.
    private static string FormatName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Friend"; // fallback when input is empty

        name = name.Trim(); // remove leading/trailing spaces
        return char.ToUpper(name[0]) + (name.Length > 1 ? name.Substring(1).ToLower() : string.Empty);
    }

    // FromConsolePrompt: convenience method that asks the user for their name
    // in the console and returns a User instance with the entered name.
    public static User FromConsolePrompt()
    {
        Console.Write("Enter your name: "); // show a prompt
        string input = Console.ReadLine(); // read a line from the console
        return new User(input); // create and return a User with that input
    }
}
