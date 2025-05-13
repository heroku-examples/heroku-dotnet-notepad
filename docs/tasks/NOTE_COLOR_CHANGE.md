# Feature: Change Note Color

Allow users to change the color of a note (red, yellow, green) by clicking an interface element in the note's header.

## Completed Tasks
- [ ] None

## In Progress Tasks
- [ ] **Task 1: Design Note Header UI for Color Change**
    - Description: Define how the color options (red, yellow, green) will be presented in the note header. This could be small clickable colored circles, a dropdown, or similar.
    - Acceptance Criteria:
        - UI elements for red, yellow, and green are present in the note header.
        - UI elements are visually distinct and clearly indicate their purpose.
- [ ] **Task 2: Implement Client-Side Logic for Color Selection**
    - Description: Add JavaScript in `Index.cshtml` to handle clicks on the new color UI elements. When a color is selected, it should:
        1. Visually update the note's color on the client immediately.
        2. Prepare the updated note data (including the new color).
    - Acceptance Criteria:
        - Clicking a color option in the note header visually changes the note's background color in the UI.
        - The correct color value ("red", "yellow", "green", or hex codes if preferred, aligning with `Note.Color` max length 7) is captured.
- [ ] **Task 3: Update SignalR Hub for Color Change**
    - Description: Modify `NoteHub.cs` to handle the color update. The existing `UpdateNote(Note note)` method should be sufficient if the client sends the complete note object with the new color. Ensure the `Note` model's `Color` property is updated.
    - Acceptance Criteria:
        - The `UpdateNote` method in `NoteHub` correctly receives and processes the note with the updated color.
        - The change is persisted to the database.
        - The updated note (with the new color) is broadcast to all connected clients.
- [ ] **Task 4: Implement Client-Side Logic to Send Color Update via SignalR**
    - Description: After a color is selected and the UI is updated locally (Task 2), modify the JavaScript in `Index.cshtml` to call the `UpdateNote` method on the SignalR hub, sending the entire note object with the new color. This will likely be an adjustment to the existing `updateNoteContent` function or a new similar function.
    - Acceptance Criteria:
        - Selecting a color in the note header triggers a call to `connection.invoke("UpdateNote", updatedNoteObject)` with the correct note ID and new color.
- [ ] **Task 5: Ensure Real-Time Color Update for All Clients**
    - Description: Verify that when one user changes a note's color, the change is reflected in real-time for all other connected clients. The existing `UpdateNote` SignalR handler on the client should already handle this if it correctly processes the `Color` property.
    - Acceptance Criteria:
        - Changes to a note's color by one client are visible to all other clients without a page refresh.
        - The note's color is correctly displayed for all clients.
- [ ] **Task 6: Update Styling for New Colors**
    - Description: Ensure CSS in `Index.cshtml` or site-wide CSS supports the new red and green colors for notes, in addition to the existing yellow.
    - Acceptance Criteria:
        - Notes can be distinctly styled with red, yellow, and green backgrounds.
        - Text color on notes remains legible for all background colors.

## Future Tasks
- [ ] Consider more color options or a color picker.
- [ ] Add persistence for the selected color palette if more options are added.

## Implementation Plan

1.  **UI Design (Task 1):** Decide on the UI elements for color selection (e.g., three small colored circles: red, yellow, green) and add them to the `addNoteToUI` JavaScript function in `Index.cshtml` within the note's header structure.
2.  **Client-Side Click Handling (Task 2 & 4):**
    *   In `addNoteToUI`, attach event listeners to the new color UI elements.
    *   When a color element is clicked:
        *   Get the note ID and the selected color value.
        *   Update the note element's style (e.g., `noteDiv.style.backgroundColor`) immediately.
        *   Find the corresponding note object in the client-side `notes` array (if one exists, or construct as needed).
        *   Update the `Color` property of this client-side note object.
        *   Call `connection.invoke("UpdateNote", noteObjectWithNewColor)`. This leverages the existing `UpdateNote` mechanism.
3.  **Server-Side Update (Task 3):**
    *   The existing `NoteHub.UpdateNote(Note note)` method should handle this automatically as it updates the entire note object. Verify that the `note.Color` property is correctly bound and saved by Entity Framework Core.
    *   The `ApplicationDbContext` already defines `MaxLength(7)` for `Color`, which is sufficient for "red", "yellow", "green" or hex codes.
4.  **Real-Time Propagation (Task 5):**
    *   The existing `connection.on("UpdateNote", function (note) { updateNoteInUI(note); });` in `Index.cshtml` should receive the update.
    *   Ensure `updateNoteInUI(note)` correctly updates the note's background color based on the `note.Color` property.
5.  **Styling (Task 6):**
    *   In `Index.cshtml` (`@section Styles` or linked CSS), add/modify styles for notes. For example:
        ```css
        .note[data-color="red"] { background-color: #ffdddd; border-left: 5px solid #f44336; }
        .note[data-color="yellow"] { background-color: #fff9c4; border-left: 5px solid #ffeb3b; } /* Existing or similar */
        .note[data-color="green"] { background-color: #ddffdd; border-left: 5px solid #4CAF50; }
        ```
    *   The `addNoteToUI` and `updateNoteInUI` functions should set a `data-color` attribute on the note `div` to apply these styles, or directly set the background color. Using a `data-attribute` is cleaner.

## Relevant Files

-   `NotepadApp/Pages/Index.cshtml`: For client-side JavaScript (SignalR interactions, DOM manipulation) and CSS.
-   `NotepadApp/Hubs/NoteHub.cs`: For server-side SignalR logic (receiving updates, broadcasting).
-   `NotepadApp/Models/Note.cs`: The entity model (already contains `Color` property).
-   `NotepadApp/Data/ApplicationDbContext.cs`: EF Core context (already configures `Color` property). 