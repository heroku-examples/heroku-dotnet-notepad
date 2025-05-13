using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotepadApp.Models;
using NotepadApp.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NotepadApp.Hubs
{
    public class NoteHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public NoteHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            // Send all existing notes to the newly connected client
            var notes = await _context.Notes.ToListAsync();
            foreach (var note in notes)
            {
                await Clients.Caller.SendAsync("ReceiveNote", note);
            }

            // Send all existing note connections to the newly connected client
            var connections = await _context.NoteConnections.ToListAsync();
            foreach (var connection in connections)
            {
                await Clients.Caller.SendAsync("ReceiveNoteConnection", connection);
            }

            await base.OnConnectedAsync();
        }

        public async Task AddNote(Note note)
        {
            note.CreatedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;
            
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            
            await Clients.All.SendAsync("ReceiveNote", note);
        }

        public async Task UpdateNote(Note note)
        {
            var existingNote = await _context.Notes.FindAsync(note.Id);
            if (existingNote != null)
            {
                existingNote.Title = note.Title;
                existingNote.Content = note.Content;
                existingNote.Color = note.Color;
                existingNote.PositionX = note.PositionX;
                existingNote.PositionY = note.PositionY;
                existingNote.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                await Clients.All.SendAsync("UpdateNote", existingNote);
            }
        }

        public async Task DeleteNote(int noteId)
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
                await Clients.All.SendAsync("DeleteNote", noteId);
            }
        }

        public async Task MoveNote(int noteId, double x, double y)
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note != null)
            {
                note.PositionX = x;
                note.PositionY = y;
                note.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                await Clients.All.SendAsync("MoveNote", noteId, x, y);
            }
        }

        public async Task ConnectNotes(int sourceNoteId, int targetNoteId)
        {
            // Prevent connecting a note to itself
            if (sourceNoteId == targetNoteId)
            {
                // Optionally, send an error message back to the caller
                // await Clients.Caller.SendAsync("ConnectionError", "Cannot connect a note to itself.");
                return;
            }

            // Check if connection already exists (either way)
            var existingConnection = await _context.NoteConnections
                .FirstOrDefaultAsync(nc => 
                    (nc.SourceNoteId == sourceNoteId && nc.TargetNoteId == targetNoteId) ||
                    (nc.SourceNoteId == targetNoteId && nc.TargetNoteId == sourceNoteId));

            if (existingConnection != null)
            {
                // Connection already exists, no need to do anything or send an error
                return;
            }

            var sourceNoteExists = await _context.Notes.AnyAsync(n => n.Id == sourceNoteId);
            var targetNoteExists = await _context.Notes.AnyAsync(n => n.Id == targetNoteId);

            if (!sourceNoteExists || !targetNoteExists)
            {
                // One or both notes do not exist
                // await Clients.Caller.SendAsync("ConnectionError", "One or both notes do not exist.");
                return;
            }

            var newConnection = new NoteConnection
            {
                SourceNoteId = sourceNoteId,
                TargetNoteId = targetNoteId,
                CreatedAt = DateTime.UtcNow
            };

            _context.NoteConnections.Add(newConnection);
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveNoteConnection", newConnection);
        }

        public async Task DisconnectNotes(int connectionId)
        {
            var connection = await _context.NoteConnections.FindAsync(connectionId);
            if (connection != null)
            {
                _context.NoteConnections.Remove(connection);
                await _context.SaveChangesAsync();
                await Clients.All.SendAsync("RemoveNoteConnection", connectionId);
            }
        }
    }
} 