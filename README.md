# Lucid API .NET Client

This project is a .NET client for interacting with the Lucid API. It provides functionality to access Lucid's services using C#.

## Prerequisites

- .NET 8.0 or later
- A Lucid API account with appropriate credentials

## Environment Setup

This project uses environment variables to manage sensitive information such as API keys. To run the project, you need to set up a `.env` file in the root of the `LucidClientTestApp` directory.

### Creating the .env file

1. In the `LucidClientTestApp` directory, create a new file named `.env`.
2. Add the following content to the file, replacing the placeholder values with your actual Lucid API credentials:

```
CLIENT_ID=your_client_id_here
CLIENT_SECRET=your_client_secret_here
REDIRECT_URI=your_redirect_uri_here
```

Replace `your_client_id_here`, `your_client_secret_here`, and `your_redirect_uri_here` with your actual Lucid API credentials.

### Important Notes

- Never commit your `.env` file to version control. It's included in the `.gitignore` file to prevent accidental commits.
- If you're collaborating with others, consider providing a `.env.example` file with placeholder values to show the required structure.

## Running the Project

1. Ensure you have set up the `.env` file as described above.
2. Open the solution in your preferred IDE (e.g., Visual Studio, VS Code with C# extension).
3. Build the solution to restore NuGet packages.
4. Run the `LucidClientTestApp` project.

## Available Scopes

The project is set up with the following scopes:

### User Scopes
- lucidchart.document.content
- lucidchart.document.content:readonly
- offline_access
- user.profile
- account.user:readonly
- account.info

### Account Scopes
- offline_access
- account.user:readonly
- account.info

## Troubleshooting

If you encounter issues related to missing environment variables:

1. Verify that your `.env` file is in the correct location (`LucidClientTestApp` directory).
2. Check that the variable names in your `.env` file match exactly with what's expected (CLIENT_ID, CLIENT_SECRET, REDIRECT_URI).
3. Ensure there are no spaces around the '=' sign in your `.env` file.
4. If running from an IDE, try restarting it to ensure it picks up the new environment variables.

## Contributing

If you'd like to contribute to this project, please fork the repository and create a pull request with your changes.

## License

[Specify your license here, e.g., MIT, Apache 2.0, etc.]