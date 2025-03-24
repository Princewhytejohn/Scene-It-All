# Scene-It-All
Scene-It-All
============

A Unity-based Android TV application that allows users to explore movies using data from The Movie Database (TMDb) API. The app includes search functionality, movie details view, trending/popular movie listings, robust error handling, and CI/CD integration.

Link to apk: https://drive.google.com/file/d/10LNhOqDb2BoaDzSZ4hzEAFXoN7oBAwP0/view?usp=sharing

Setup Instructions
------------------

1. **Clone the Repository**:
   git clone https://github.com/your-username/scene-it-all.git

2. **Open in Unity**:
   - Use Unity version 2022.3.49f1 or higher
   - Open the project from Unity Hub.

3. **TMDb API Key**:
   - On first launch, enter your TMDb API key.
   - The key will be validated and saved (base64 encoded).

4. **Building APK**:
   - For manual builds, go to `File -> Build Settings -> Android -> Build`.
   - For CI builds, push a release tag to the `main` branch.

5. **Unit Tests**:
   - Located in the `Tests` folder.
   - Can be run using Unity Test Runner.

6. **CI/CD (GitHub Actions)**:
   - Automatically runs unit tests and builds an APK on release.

Architecture Overview
---------------------

- **UIManager**: Centralized screen navigation and transitions.
- **HomeScreenController**: Loads trending and popular movies. Handles loading/error states and retry logic.
- **SearchScreenController**: Handles movie searching, trending and recent search display, and infinite scroll.
- **DetailsScreenController**: Displays movie metadata, genres, and cast using TMDb's additional endpoints.
- **APIKeyController**: Validates and stores the user's API key securely.
- **MovieResult, GenreResponse, etc.**: Data models for TMDb API responses.

Design Decisions and Trade-offs
-------------------------------

- **API Key Obfuscation**: Simple base64 encoding is used. For production, a more secure method is advised.
- **Manual License Activation**: Required for CI builds. ULF is encoded and added as a GitHub secret.
- **Error Handling**: Fallbacks and redirections are built into each API-dependent controller.
- **No Addressables or Asset Bundles**: Due to time constraints and simplicity.

Known Issues / Limitations
---------------------------

- **License Activation Failures**: If Unity license is invalid or incorrect, CI build will fail.
- **No Offline Mode**: App does not cache data between sessions yet.
- **Unit Tests Coverage**: Limited to API key logic and basic UI manager behavior.

Possible Improvements
---------------------

- Implement persistent response caching with expiry.
- Improve unit test coverage for all controllers.
- Add pagination preloading in search results.
- Support offline access for previously viewed content.
