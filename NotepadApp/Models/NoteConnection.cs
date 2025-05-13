using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotepadApp.Models
{
    public class NoteConnection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SourceNoteId { get; set; }

        [ForeignKey("SourceNoteId")]
        public virtual Note? SourceNote { get; set; }

        [Required]
        public int TargetNoteId { get; set; }

        [ForeignKey("TargetNoteId")]
        public virtual Note? TargetNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 