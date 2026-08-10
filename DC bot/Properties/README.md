# Properties

This folder contains assembly-level project properties.

## Why This Folder Exists

The `Properties` folder is the conventional place for assembly-level metadata in .NET projects. It is not part of bot runtime behavior; it only describes compiled assembly attributes.

Most metadata can now live in the `.csproj`, so this folder should stay small.

## Files

### AssemblyInfo.cs

**Purpose:** Assembly metadata and attributes.

**Contains:**

- Assembly version information
- Company and product information
- Copyright and license details

**Note:** Most metadata is now defined in `.csproj` file rather than code.

## When To Change This

Change this folder only when assembly-level attributes are needed and they cannot be represented cleanly in `DC bot.csproj`.

Do not add runtime configuration, DI setup, bot settings, or command metadata here.

---

## Related Components

- **DC bot.csproj** - Project configuration
- **Program.cs** - Application entry point

