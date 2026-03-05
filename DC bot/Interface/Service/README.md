# Service Interfaces

This folder contains service abstraction contracts grouped by domain.

## Subfolders

### IO/
File system service interfaces.

**File:** `IFileSystem.cs`

---

### Localization/
Localization service interfaces.

**File:** `ILocalizationService.cs`

---

### Music/
Music and playback service interfaces.

**Files:**
- `ILavaLinkService.cs`
- `ITrackSearchResolverService.cs`
- `MusicServiceInterface/` - Granular music service interfaces

---

### Presentation/
Response and presentation interfaces.

**File:** `IResponseBuilder.cs`

---

## Related Components

- **Service/** - Implements these interfaces
- **Commands/** - Use service interfaces for business logic

