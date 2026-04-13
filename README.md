# CYBERSECURITY_CHATBOT

It is designed to educate users about cybersecurity in a simple and interactive way.

The chatbot helps users understand topics like password safety, phishing, malware, and safe browsing.

The system is built using a modular design where each part has a specific role.
The main program controls the flow of the application and handles user interaction.

The ChatBot class processes user input and generates responses.

The User class cleans and formats the user’s name for better interaction.
The GreetingInitializer checks if a greeting audio file exists when the program starts.

If the file does not exist, it generates a WAV file using sound wave properties.

The AudioPlayer plays the greeting sound either synchronously or asynchronously.

The user interface includes an ASCII logo, colour-coded text, and a typing animation.

These features make the chatbot more engaging and user-friendly.
The program runs in a continuous loop to allow ongoing conversation.

The user enters a message, and the system checks if they want to exit.

Input validation ensures empty or incorrect inputs are handled properly.

The chatbot uses keyword matching to understand user input.

It converts input to lowercase and checks for keywords using the Contains function.
If a keyword like "password" or "phishing" is detected, it returns the correct response.

The system includes the Levenshtein Distance algorithm to detect spelling errors.

If a word is misspelled, the chatbot suggests the closest matching topic.

It remembers previous suggestions using a variable called lastSuggestion.

The chatbot can also handle multiple questions in one input.

It splits the input and responds to each question individually.
