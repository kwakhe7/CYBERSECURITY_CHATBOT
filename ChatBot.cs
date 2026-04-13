using System;
using System.Threading;

//  this file defines a tiny ChatBot helper class.
// Each comment below explains the next line or block in simple terms for beginners.
public class ChatBot
{
    // Property: stores the user's name so the bot can use it in replies.
    public string UserName { get; private set; }

    // Field: keeps the last spelling suggestion we offered to the user.
    private string lastSuggestion = null;

    // Constructor: runs when a ChatBot is created. It remembers the user's name.
    public ChatBot(string userName)
    {
        UserName = string.IsNullOrWhiteSpace(userName) ? "Friend" : userName;
    }

    // GetResponse: main function. Give it what the user typed and it returns the bot's reply.
    public string GetResponse(string input)
    {
        // If there's no input, there's no reply.
        if (input == null)
            return null;

        // Make a lowercase trimmed version so checks are simple and case-insensitive.
        string lowerInput = input.ToLower().Trim();

        // If we suggested a correction earlier, handle a yes/no answer first.
        if (lastSuggestion != null)
        {
            // If the user said yes (starts with 'y'), return the full info for that topic.
            if (lowerInput.StartsWith("y"))
            {
                string topicAnswer = GetTopicResponse(lastSuggestion);
                lastSuggestion = null; // clear stored suggestion
                return topicAnswer ?? "  Sorry, I don't have more information about that right now.";
            }
            // If the user said no (starts with 'n'), ask them to rephrase.
            else if (lowerInput.StartsWith("n"))
            {
                lastSuggestion = null; // clear stored suggestion
                return "  Okay — please rephrase your question or try one of these topics: password, phishing, malware, safe browsing.";
            }
            // If reply is neither yes nor no, fall through to normal matching.
        }

        // If the user asks how the bot is doing, answer friendly status.
        if (lowerInput.Contains("how are you"))
            return $"  I'm running at 100%, {UserName}! Fully alert and ready to help you stay safe online. ";

        // If the message mentions passwords, return a short password tip.
        if (lowerInput.Contains("password"))
            return "    PASSWORD SAFETY TIPS:\n\n" +
                   "  • Use at least 12 characters — mix letters, numbers and symbols.";

        // If the message mentions phishing (partial match), return a short warning.
        if (lowerInput.Contains("phish"))
            return "   PHISHING AWARENESS:\n\n" +
                   "  Be careful with unexpected emails asking for credentials.";

        // If exact checks didn't match, try to detect a typo and suggest a close topic.
        string suggestion = SuggestCorrection(lowerInput);
        if (suggestion != null)
        {
            lastSuggestion = suggestion; // remember suggestion for follow-up yes/no
            return $"  Did you mean '{suggestion}'? (yes/no)";
        }

        // If the user says hello, greet them back using their name.
        if (lowerInput.Contains("hello") || lowerInput.Contains("hi") || lowerInput.Contains("hey"))
            return $"  Hey there, {UserName}!  How can I help keep you safe online today?";

        // If the user thanks the bot, reply politely.
        if (lowerInput.Contains("thank") || lowerInput.Contains("thanks"))
            return $"  You're very welcome, {UserName}! Stay safe out there. ";

        // If we still don't know, return null so the caller can handle unknown queries.
        return null;
    }

    // GetTopicResponse: given a chosen topic name, return a short explanation string.
    private string GetTopicResponse(string topic)
    {
        // Lowercase the topic to make comparisons easier.
        topic = topic?.ToLower() ?? string.Empty;

        // If topic refers to phishing, return a longer help text.
        if (topic.Contains("phish") || topic.Contains("phishing"))
            return "    PHISHING AWARENESS:\n\n  Phishing is when criminals pretend to be a trusted company to trick you into giving them your login details or personal information.\n\n  HOW TO SPOT A PHISHING ATTEMPT:\n  • The email sender's address looks slightly 'off'\n  • There's a sense of urgency\n  • The link doesn't match the real website\n  • They're asking for your password or card number via email\n\n  Legitimate companies will NEVER ask for your password through email or SMS.";

        // If topic is password-related, return concise tips.
        if (topic.Contains("password"))
            return "    PASSWORD SAFETY TIPS:\n\n  • Use at least 12 characters — mix letters, numbers and symbols.\n  • Never reuse passwords.\n  • Consider a password manager like Bitwarden.";

        // If topic is malware-related, return brief protections.
        if (topic.Contains("malware") || topic.Contains("virus") || topic.Contains("ransomware"))
            return "    MALWARE & VIRUSES:\n\n  Install a trusted antivirus, don't open unknown attachments, and keep software updated. Back up important files.";

        // If topic relates to browsing, return safe browsing tips.
        if (topic.Contains("brows") || topic.Contains("browse") || topic.Contains("web"))
            return "    SAFE BROWSING:\n\n  Use HTTPS sites, avoid unknown downloads, and consider privacy extensions like uBlock Origin.";

        // If topic matches two-factor auth, return a short note.
        if (topic.Contains("2fa") || topic.Contains("two factor") || topic.Contains("two-factor") || topic.Contains("authentication"))
            return "    TWO-FACTOR AUTHENTICATION:\n\n  Enable 2FA on important accounts using an authenticator app or SMS where supported.";

        // For any other topics we don't know, return null.
        return null;
    }

    // SuggestCorrection: try to guess which known topic the user meant if they made a typo.
    private string SuggestCorrection(string input)
    {
        // If the input is too short, don't attempt suggestion.
        if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            return null;

        // Clean function: remove non-alphanumeric chars and lowercase the string to compare fairly.
        string Clean(string s) => System.Text.RegularExpressions.Regex.Replace(s ?? string.Empty, "[^a-z0-9]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).ToLower();

        // Prepare the cleaned user text for matching.
        string probe = Clean(input);
        if (string.IsNullOrEmpty(probe))
            return null;

        // List of canonical topics we can suggest.
        string[] topics = new string[] { "phishing", "password", "malware", "safe browsing", "two-factor", "privacy", "vpn", "social engineering", "updates", "backup", "mobile security", "encryption", "secure shopping" };

        string best = null; // best candidate topic
        int bestDist = int.MaxValue; // smallest edit distance found

        // Compare the cleaned input against each topic.
        foreach (var t in topics)
        {
            string norm = Clean(t);

            // If the cleaned probe is a substring of the topic (or vice versa), return that topic immediately.
            if (norm.Contains(probe) || probe.Contains(norm))
            {
                return t; // confident match
            }

            // Otherwise compute edit distance to measure similarity.
            int d = LevenshteinDistance(norm, probe);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        // If no candidate, give up.
        if (best == null)
            return null;

        // Use an adaptive threshold based on topic length to decide if suggestion is acceptable.
        int threshold = Math.Max(1, Clean(best).Length / 3);
        if (bestDist <= threshold)
            return best;

        // No good suggestion found.
        return null;
    }

    // LevenshteinDistance: compute the minimum edits to transform string a into b.
    private int LevenshteinDistance(string a, string b)
    {
        // dp table where dp[i,j] is the distance between a[0..i-1] and b[0..j-1]
        int[,] dp = new int[a.Length + 1, b.Length + 1];

        // initialize base cases: distance from empty string
        for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

        // fill the table with dynamic programming
        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1; // 0 if chars match, 1 otherwise
                dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
            }
        }

        // return the distance for the full strings
        return dp[a.Length, b.Length];
    }

    // TypeWrite: print characters one at a time to simulate typing.
    public void TypeWrite(string text, int delayMs = 30)
    {
        Console.ForegroundColor = ConsoleColor.Cyan; // set color
        foreach (char letter in text) // write each character
        {
            Console.Write(letter);
            Thread.Sleep(delayMs); // wait a bit
        }
        Console.WriteLine(); // finish the line
        Console.ResetColor(); // reset color to default
    }

    // DisplayAsciiLogo: print a small identifying line for this helper class.
    public void DisplayAsciiLogo()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("  SafeNet Assistant — ChatBot class (separate from Program.cs)");
        Console.ResetColor();
    }
}
