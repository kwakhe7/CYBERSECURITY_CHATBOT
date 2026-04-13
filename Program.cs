// Basic system namespaces. They provide Console I/O, audio playback, and small delays.
using System;
using System.Media;   // for playing WAV files with SoundPlayer
using System.Threading;   // for Thread.Sleep used in the typing effect

// Top-level class that runs the console chatbot application.
// Comments are written in plain English so people who are new to C# can follow along.
class SafeNetAssistant
{
    // Helper method: prints a line of text in cyan color, then resets the color.
    static void PrintCyan(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan; // change text color
        Console.WriteLine(text); // write the text and newline
        Console.ResetColor(); // return console color to default
    }

    // --------------------------------------------------------
    //  MULTI-QUESTION HANDLER
    //  If the user asks multiple questions in one message (e.g. "What is
    //  phishing? How do I protect my passwords?"), split the input and
    //  answer each part. If any sub-question cannot be answered, include
    //  a polite apology and ask if the user has another question.
    // --------------------------------------------------------
    static string GetResponseMulti(string input, string userName)
    {
        if (input == null)
            return null;

        char[] separators = new char[] { '?', ';', '\n' };
        string[] parts = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1 && input.ToLower().Contains(" and "))
        {
            parts = input.Split(new string[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
        }

        if (parts.Length <= 1)
        {
            // Single question — delegate to existing GetResponse
            return GetResponse(input, userName);
        }

        bool anyUnknown = false;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var raw in parts)
        {
            string part = raw.Trim();
            if (string.IsNullOrEmpty(part))
                continue;

            string single = GetResponse(part, userName);
            if (single == null)
            {
                anyUnknown = true;
            }
            else
            {
                sb.AppendLine(single);
                sb.AppendLine();
            }
        }

        if (anyUnknown)
        {
            sb.AppendLine("  sorry i can't answer that at the moment can you please wait i am still improving");
            sb.AppendLine();
            sb.AppendLine("  Is there any question you want?");
        }

        string result = sb.ToString().TrimEnd();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    // Helper: print a line using the green color (useful for headings).
    static void PrintGreen(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    // Helper: print a line in yellow (good for prompts and warnings).
    static void PrintYellow(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    // Helper: print a line in red (for errors or important warnings).
    static void PrintRed(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    // Helper: print a decorative divider line to visually separate screen sections.
    static void PrintDivider()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("══════════════════════════════════════════════════ ");
        Console.ResetColor();
    }

    // Print text one character at a time to create a "typing" animation.
    // 'delayMs' controls the speed (milliseconds per character).
    static void TypeWrite(string text, int delayMs = 30)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        foreach (char letter in text)
        {
            Console.Write(letter); // print single character without newline
            Thread.Sleep(delayMs); // small pause to simulate typing
        }
        Console.WriteLine(); // finish the line
        Console.ResetColor();
    }

    // The voice greeting is played at startup if a file named "greeting.wav"
    // exists in the program folder. We check for the file and play it
    // synchronously so the user hears the greeting before the logo appears.

    // Display a simple ASCII logo and a small boxed title.
    // This is purely cosmetic and helps the user recognise the app.
    static void DisplayAsciiLogo()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine(@"
   ███████╗ █████╗ ███████╗███████╗███╗   ██╗███████╗████████╗
   ██╔════╝██╔══██╗██╔════╝██╔════╝████╗  ██║██╔════╝╚══██╔══╝
   ███████╗███████║█████╗  █████╗  ██╔██╗ ██║█████╗     ██║   
   ╚════██║██╔══██║██╔══╝  ██╔══╝  ██║╚██╗██║██╔══╝     ██║   
   ███████║██║  ██║██║     ███████╗██║ ╚████║███████╗   ██║   
   ╚══════╝╚═╝  ╚═╝╚═╝     ╚══════╝╚═╝  ╚═══╝╚══════╝   ╚═╝   
        ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(@"
          ╔══════════════════════════════════════════╗
          ║     S A F E N E T   A S S I S T A N T    ║
          ║      Your Cybersecurity Guide & Buddy    ║
          ╚══════════════════════════════════════════╝
        ");
        Console.ResetColor();
    }

 
    // Ask the user for their name, tidy it up, and display a welcome message.
    static string GreetUser()
    {
        PrintDivider();
        TypeWrite("  Hello! I'm SafeNet Assistant, your personal cybersecurity guide.");
        TypeWrite("  I'm here to help you stay safe in the digital world. ");
        PrintDivider();

        Console.WriteLine();
        PrintYellow("   What's your name? ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(">> ");
        string name = Console.ReadLine(); // read user input

        // If the user typed nothing, use a friendly default name.
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Friend";
        }
        else
        {
            // Trim spaces and capitalise the first letter for nicer display.
            name = name.Trim();
            name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
        }

        Console.ResetColor();
        Console.WriteLine();
        PrintDivider();
        TypeWrite($"  Welcome, {name}! Great to have you here. ");
        TypeWrite("  I'm fully loaded and ready to answer your cybersecurity questions.");
        PrintDivider();
        Console.WriteLine();

        return name; // return the cleaned name for later use
    }

    // --------------------------------------------------------
    //  QUESTION 4 — BASIC RESPONSE SYSTEM
    //  This is the "brain" of the chatbot. It looks at what
    //  the user typed and decides which answer to give back.
    //  We use .Contains() to check for keywords — so even
    //  "tell me about phishing" will match the "phishing" topic.
    // --------------------------------------------------------
    static string GetResponse(string input, string userName)
    {
        // Convert to lowercase so we don't miss "Phishing" vs "phishing"
        string lowerInput = input.ToLower().Trim();

        // --- General / Small-talk responses ---

        if (lowerInput.Contains("how are you"))
            return $"  I'm running at 100%, {userName}! Fully alert and ready to help you stay safe online. ";

        if (lowerInput.Contains("what's your purpose") || lowerInput.Contains("what is your purpose") || lowerInput.Contains("why are you here"))
            return $"  My purpose is to help people like you, {userName}, understand cybersecurity.\n" +
                   "  I'll teach you how to protect your passwords, spot phishing scams,\n" +
                   "  and browse the web safely — in plain, easy-to-understand language!";

        if (lowerInput.Contains("what can i ask") || lowerInput.Contains("what do you know") || lowerInput.Contains("help"))
            return "  Great question! Here are the topics I can help you with:\n\n" +
                   "      Password safety\n" +
                   "      Phishing scams\n" +
                   "      Safe browsing\n" +
                   "      Two-factor authentication (2FA)\n" +
                   "      Malware & viruses\n" +
                   "      Privacy & data protection\n" +
                   "      VPNs & public Wi-Fi safety\n" +
                   "      Social engineering & scams\n" +
                   "      Software updates & patching\n" +
                   "      Backup strategies\n" +
                   "      Mobile device security\n" +
                   "      Encryption basics\n" +
                   "      Secure online shopping\n\n" +
                   "  Just type a topic or ask me a question (for example: 'Tell me about phishing').";

        if (lowerInput.Contains("hello") || lowerInput.Contains("hi") || lowerInput.Contains("hey"))
            return $"  Hey there, {userName}!  How can I help keep you safe online today?";

        if (lowerInput.Contains("thank") || lowerInput.Contains("thanks"))
            return $"  You're very welcome, {userName}! Stay safe out there. ";

        // --- Cybersecurity Topics ---

        if (lowerInput.Contains("password"))
            return "    PASSWORD SAFETY TIPS:\n\n" +
                   "  • Use at least 12 characters — mix letters, numbers and symbols.\n" +
                   "  • Never use the same password on more than one website.\n" +
                   "  • Avoid obvious choices like your birthday, name or 'password123'.\n" +
                   "  • Use a trusted Password Manager (like Bitwarden or 1Password)\n" +
                   "    to generate and store strong passwords for you.\n" +
                   "  • Change your passwords immediately if you suspect a breach.";

        if (lowerInput.Contains("phish"))
            return "    PHISHING AWARENESS:\n\n" +
                   "  Phishing is when criminals pretend to be a trusted company\n" +
                   "  (like your bank or Gmail) to trick you into giving them your\n" +
                   "  login details or personal information.\n\n" +
                   "  HOW TO SPOT A PHISHING ATTEMPT:\n" +
                   "  • The email sender's address looks slightly 'off' (e.g. support@g00gle.com)\n" +
                   "  • There's a sense of urgency: 'Act NOW or your account will be closed!'\n" +
                   "  • The link in the email doesn't match the real website\n" +
                   "  • They're asking for your password or card number via email\n\n" +
                   "  GOLDEN RULE: Legitimate companies will NEVER ask for your\n" +
                   "  password through email or SMS.";

        if (lowerInput.Contains("brows") || lowerInput.Contains("internet") || lowerInput.Contains("web"))
            return "    SAFE BROWSING TIPS:\n\n" +
                   "  • Always look for the padlock  in your browser's address bar.\n" +
                   "    This means the website uses HTTPS (encrypted connection).\n" +
                   "  • Don't click on pop-up ads claiming you've won something.\n" +
                   "  • Avoid downloading software from unknown websites.\n" +
                   "  • Use a reputable browser like Firefox or Chrome with\n" +
                   "    built-in security features.\n" +
                   "  • Consider a privacy extension like uBlock Origin to block\n" +
                   "    dangerous ads and trackers.";

        if (lowerInput.Contains("two factor") || lowerInput.Contains("2fa") || lowerInput.Contains("two-factor") || lowerInput.Contains("authentication"))
            return "    TWO-FACTOR AUTHENTICATION (2FA):\n\n" +
                   "  2FA adds a second lock to your accounts. Even if someone\n" +
                   "  steals your password, they still can't get in without\n" +
                   "  the second verification step.\n\n" +
                   "  HOW IT WORKS:\n" +
                   "  1. You enter your password (something you KNOW)\n" +
                   "  2. You confirm with a code sent to your phone, or\n" +
                   "     generated by an app like Google Authenticator\n" +
                   "     (something you HAVE)\n\n" +
                   "   Enable 2FA on your email, banking, and social media ASAP!";

        if (lowerInput.Contains("malware") || lowerInput.Contains("virus") || lowerInput.Contains("ransomware"))
            return "    MALWARE & VIRUSES:\n\n" +
                   "  Malware is any software designed to harm your computer\n" +
                   "  or steal your data. Viruses, ransomware, and spyware\n" +
                   "  are all types of malware.\n\n" +
                   "  HOW TO PROTECT YOURSELF:\n" +
                   "  • Install a trusted antivirus programme (e.g. Windows Defender,\n" +
                   "    Malwarebytes) and keep it updated.\n" +
                   "  • Never open email attachments from people you don't know.\n" +
                   "  • Keep your operating system and apps up to date —\n" +
                   "    updates often patch security holes.\n" +
                   "  • Back up your important files regularly (external drive or cloud).";

        // --- If nothing matched, return null so we can handle it separately ---
        return null;
    }

    // --------------------------------------------------------
    //  QUESTION 5 — INPUT VALIDATION
    //  This handles cases where the user types nothing,
    //  or something the bot doesn't recognise.
    //  A chatbot should never just crash or go silent!
    // --------------------------------------------------------
    static void HandleInvalidInput(string userName, bool isEmpty)
    {
        Console.WriteLine();
        if (isEmpty)
        {
            // The user pressed Enter without typing anything
            PrintRed("    Oops! You didn't type anything.");
            TypeWrite($"  Please type a question or topic, {userName}. I'm listening! ", 25);
        }
        else
        {
            // The user typed something but we don't have a matching response
            PrintRed("    Hmm, I didn't quite understand that.");
            TypeWrite("  Could you rephrase? Try asking about:", 25);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("• passwords   • phishing   • safe browsing   • 2FA");
            Console.WriteLine("• malware     • privacy    • VPNs/public Wi-Fi  • social engineering");
            Console.WriteLine("• updates     • backups    • mobile security    • encryption");
            Console.WriteLine("• secure shopping  • what can I ask");
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    // --------------------------------------------------------
    //  QUESTION 6 — ENHANCED CONSOLE UI
    //  This displays the "prompt bar" — the line where the user
    //  types their message. It's styled to look like a chat input.
    // --------------------------------------------------------
    static void ShowInputPrompt(string userName)
    {
        PrintDivider();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"[{userName}] ➤ ");
        Console.ForegroundColor = ConsoleColor.White;
    }

    // --------------------------------------------------------
    //  MAIN METHOD — THE STARTING POINT
    //  This is where the programme begins.
    //  Think of it like the "on switch" — everything starts here.
    // --------------------------------------------------------
    static void Main(string[] args)
    {
        // Set the console to look clean and dark — Cyan on Black
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Clear(); // Clear any old text on the screen
        Console.Title = "SafeNet Assistant — Cybersecurity Chatbot"; // Sets the window title bar

        // --- STEP 1: Play the voice greeting (if the WAV file exists) ---
        // Try to play a WAV file named "greeting.wav" from the application's
        // output folder (e.g. bin/Debug/net8.0). If the file is missing or an
        // error occurs, the program will continue silently.
        try
        {
            string exeDir = AppContext.BaseDirectory; // runtime folder of the executable
            string greetingPath = System.IO.Path.Combine(exeDir, "greeting.wav");

            if (System.IO.File.Exists(greetingPath))
            {
                using (var player = new SoundPlayer(greetingPath))
                {
                    // PlaySync blocks until the WAV finishes, so the ASCII logo
                    // will be shown after the greeting is played.
                    player.PlaySync();
                }
            }
        }
        catch
        {
            // Ignore any errors related to audio playback so the app keeps running.
        }

        // --- STEP 2: Show the ASCII logo title screen ---
        DisplayAsciiLogo();

        // Small pause to let the user take in the logo before we continue
        Thread.Sleep(800);

        // --- STEP 3: Greet the user and get their name ---
        string userName = GreetUser();

        // --------------------------------------------------------
        //  MAIN CHAT LOOP
        //  This keeps the chatbot running until the user decides to exit.
        //  Each time around the loop = one conversation turn.
        // --------------------------------------------------------
        while (true)
        {
            // Show the styled input prompt with the user's name
            ShowInputPrompt(userName);

            // Read whatever the user types and store it
            string userInput = Console.ReadLine();

            Console.ResetColor();
            Console.WriteLine();

            // --- STEP 4: Check if the user wants to quit ---
            if (userInput != null &&
               (userInput.Trim().ToLower() == "exit" ||
                userInput.Trim().ToLower() == "quit" ||
                userInput.Trim().ToLower() == "bye"))
            {
                PrintDivider();
                TypeWrite($"  Goodbye, {userName}! Stay safe online. ", 35);
                TypeWrite("  Remember: Think before you click! ", 35);
                PrintDivider();
                Console.WriteLine();
                break; // Exit the loop and close the programme
            }

            // --- STEP 5: Input validation — did they type anything? ---
            if (string.IsNullOrWhiteSpace(userInput))
            {
                HandleInvalidInput(userName, isEmpty: true);
                continue; // Skip to the next loop cycle, don't try to get a response
            }

            // --- STEP 6: Get a response from our response system ---
            // Use the multi-question handler so users can ask two or more
            // questions in one message (e.g. "What is phishing? How to stay safe?").
            string response = GetResponseMulti(userInput, userName);

            // --- STEP 7: If no match was found, handle the unknown input ---
            if (response == null)
            {
                HandleInvalidInput(userName, isEmpty: false);
                continue;
            }

            // --- STEP 8: Display the chatbot's response with a typing effect ---
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("┌─ SafeNet Assistant ────────────────────────────── ");
            Console.ResetColor();

            // We split the response line by line to apply the typing effect to each line
            foreach (string line in response.Split('\n'))
            {
                TypeWrite(line, 18);
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("└────────────────────────────────────────────────── ");
            Console.ResetColor();
            Console.WriteLine();
        }

        // Pause before closing so the user can read the goodbye message
        Console.WriteLine();
        PrintYellow("  Press any key to close SafeNet Assistant...");
        Console.ReadKey();
    }
}
