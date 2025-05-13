# Note Color Change Implementation

Allow users to change the color of a note (between red, yellow, and green) by clicking an interface element within the note's header.

## Proposed Tasks
- [ ] **Task 1: Design Color Picker UI in Note Header**
    -   Determine the visual representation of the color options (e.g., small colored circles, dropdown).
    -   Modify the `addNoteToUI` JavaScript function in `NotepadApp/Pages/Index.cshtml` to include these UI elements in newly created and existing notes.
    -   Ensure the UI elements are styled appropriately and are clearly part of the note header.
- [ ] **Task 2: Implement Frontend Logic for Color Selection**
    -   In `NotepadApp/Pages/Index.cshtml`, add JavaScript event listeners to the new color picker UI elements.
    -   When a color is selected:
        -   Visually update the note's background color on the client-side immediately.
        -   Prepare the note data with the new color.
        -   Call the existing `updateNoteContent` function (or a new dedicated function if cleaner) to send the update to the server via the `UpdateNote` SignalR hub method.
- [ ] **Task 3: Update Note Styling Based on Color**
    -   Modify the CSS in `NotepadApp/Pages/Index.cshtml` (or `site.css` if preferred for organization) to define styles for notes with "red", "yellow", and "green" colors.
    -   Ensure the `addNoteToUI` and `updateNoteInUI` JavaScript functions apply the correct CSS class or inline style based on the `note.color` property.
- [ ] **Task 4: Verify Server-Side Persistence (Existing Functionality)**
    -   Confirm that the existing `NoteHub.UpdateNote(Note note)` method correctly saves the `Color` property to the database. (Based on `docs/app.md`, the `Note` model already has a `Color` property, and `UpdateNote` should handle general updates.)
    -   Test the full flow: change color, refresh page, verify color persists.

## Future Tasks (Optional Enhancements)
- [ ] Add more color choices.
- [ ] Allow custom color input.

## Implementation Plan

1.  **UI Elements:** Start by adding simple clickable colored dots (red, yellow, green) to the note header within the `addNoteToUI` function.
2.  **Client-Side Logic:** Attach event listeners to these dots. On click, change the note's `div` background color directly and then call `updateNoteContent` (or a similar function that invokes `connection.invoke("UpdateNote", noteObject)`), passing the full note object with the updated color.
3.  **Styling:** Ensure the `note.color` property from the server is used to set the initial color and that client-side changes are reflected visually. The current system likely uses inline styles or classes; adapt to that.
4.  **Testing:** Thoroughly test adding notes, changing colors, and ensuring persistence across sessions and for other connected clients.

## Relevant files

-   `NotepadApp/Pages/Index.cshtml` - Primary file for UI (HTML and JavaScript) and SignalR client logic.
-   `NotepadApp/Models/Note.cs` - C# model for the Note, already includes a `Color` property.
-   `NotepadApp/Hubs/NoteHub.cs` - SignalR hub that handles `UpdateNote` calls.
-   `NotepadApp/wwwroot/css/site.css` - Potential location for new CSS styles if not inlined in `Index.cshtml`.
-   `docs/app.md` - Application documentation (already reviewed). 