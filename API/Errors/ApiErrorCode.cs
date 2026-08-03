namespace API.Errors;

public enum ApiErrorCode
{
    Unknown = 0,
    NotFound = 1,
    Conflict = 2,           // e.g., playlist name conflict
    Validation = 3,         // e.g., invalid name
    NotConnected = 4,       // e.g., Lavalink not connected
    NoActivePlayer = 5,     // e.g., no active player
    Forbidden = 6           // e.g., forbidden action
}